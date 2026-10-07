using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Function.Provider;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // Guards patrol, investigate noise, bodies and dark rooms, hunt a player they have seen - across floors,
    // through the stairwell doors - and shoot once they have fully seen them. In a fight a guard that uses cover
    // runs to a crate between it and the player and pops up to shoot; one that raises the alarm first runs to
    // the others and tells them where the player is.
    internal class SimulateGuardsCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (!run.Alive || run.Won || run.Paused) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            LightRVO playerRoom = _model.LightAt(run.Player);
            bool playerDark = playerRoom != null && !playerRoom.On;
            foreach (GuardRVO guard in run.Guards)
            {
                if (!guard.Alive) continue;
                if (guard.StunRemaining > 0) { guard.StunRemaining -= dt; continue; }
                // A friendly goes about its patrol and pays the player no attention.
                if (guard.Faction == Faction.Friendly)
                {
                    Patrol(guard, run, dt, guard.WalkSpeed);
                    continue;
                }
                Vector2 eye = guard.Position + Vector2.up * 1.15f;
                Vector2 target = run.Player + Vector2.up * (run.Crouching ? 0.5f : 1f);
                float dx = run.Player.x - guard.Position.x;
                LightRVO room = _model.LightAt(guard.Position);
                float range = playerDark && run.NoiseRemaining <= 0 ? guard.DarkSight : run.Crouching ? guard.SightRange * guard.CrouchSight : guard.SightRange;
                bool sameFloor = Mathf.Abs(run.Player.y - guard.Position.y) < 1.8f;
                bool sees = !run.InVent && !run.Hidden && sameFloor && Mathf.Abs(dx) < range
                    && (Mathf.Sign(dx) == guard.Facing || guard.Alerted || Mathf.Abs(dx) < guard.CloseSense)
                    && Clear(eye, target);
                if (run.NoiseRemaining > 0 && Vector2.Distance(run.NoisePosition, guard.Position) < run.NoiseRadius * guard.Hearing)
                {
                    guard.Alerted = true;
                    Investigate(guard, run.NoisePosition);
                }
                if (!guard.Alerted && SeesBody(guard, run)) guard.Alerted = true;
                // A bullet striking a wall or a crate nearby: it comes to look, uneasy but not yet sure.
                foreach (DisturbanceRVO disturbance in run.Disturbances)
                {
                    if (guard.Investigating || Mathf.Abs(disturbance.Position.y - guard.Position.y) > 2f) continue;
                    if (Vector2.Distance(disturbance.Position, guard.Position) > guard.Curiosity) continue;
                    guard.Suspicion = Mathf.Max(guard.Suspicion, _model.Tuning.ImpactSuspicion);
                    Investigate(guard, new Vector2(disturbance.Position.x, guard.Position.y));
                }
                if (!guard.Investigating && room != null && !room.On) Investigate(guard, new Vector2(room.Switch, room.Floor));
                // While it is hunting, it stays sure of what it saw.
                guard.Suspicion = Mathf.Clamp01(guard.Suspicion + dt * (sees ? guard.SuspicionRate : guard.Chase > 0f ? 0f : -guard.SuspicionDecay));
                if (sees && guard.Suspicion >= 1f)
                {
                    guard.Chase = guard.ChaseMemory;
                    guard.LastSeen = run.Player;
                }
                else guard.Chase = Mathf.Max(0f, guard.Chase - dt);
                bool fighting = guard.Suspicion >= 1f && guard.Chase > 0f;
                if (!fighting) { guard.HasCover = false; guard.Crouching = false; }
                if (fighting && guard.RaisesAlarm && !guard.AlarmDone && RaiseAlarm(guard, run, dt)) continue;
                if (fighting && guard.UsesCover && TakeCover(guard, run, sees, dx, eye, target, dt)) continue;
                // A spotted player is hunted wherever they go, until they vanish into a vent or the dark, or the trail goes cold.
                if (!sees && guard.Chase > 0f && !run.InVent && !run.Hidden && !guard.HasCover) Investigate(guard, run.Player);
                if (guard.Suspicion >= 1f && sees)
                {
                    guard.Alerted = true;
                    guard.Facing = dx >= 0 ? 1 : -1;
                    Shoot(guard, run, eye, target, dt);
                    continue;
                }
                guard.FireCooldown = guard.ReactionTime;
                if (sees) { guard.Facing = dx >= 0 ? 1 : -1; continue; }
                float speed = guard.Alerted ? guard.AlertSpeed : guard.WalkSpeed;
                if (guard.Investigating)
                {
                    float goal = guard.Target.x;
                    StairRVO stair = null;
                    if (!OnFloorOf(guard.Position, guard.Target, run))
                    {
                        stair = StairTowards(guard, run);
                        if (stair == null) { guard.Investigating = false; continue; }
                        goal = stair.Position.x;
                    }
                    float gap = goal - guard.Position.x;
                    if (Mathf.Abs(gap) < 0.15f)
                    {
                        if (stair != null)
                        {
                            guard.Position = run.Stairs[stair.Link].Position;
                            continue;
                        }
                        if (room != null && !room.On && Mathf.Abs(room.Switch - guard.Position.x) < 0.3f) room.On = true;
                        guard.Wait -= dt;
                        if (guard.Wait <= 0) guard.Investigating = false;
                        continue;
                    }
                    guard.Facing = gap > 0 ? 1 : -1;
                }
                else
                {
                    Patrol(guard, run, dt, speed);
                    continue;
                }
                Step(guard, run, dt, speed);
            }
        }
        private void Shoot(GuardRVO guard, ContractRVO run, Vector2 eye, Vector2 target, float dt)
        {
            guard.FireCooldown -= dt;
            WeaponCVO weapon = _model.Weapon(guard.Weapon);
            if (guard.FireCooldown > 0 || weapon == null) return;
            // Guards fire real bullets at where the player is now; ducking or leaving their path dodges them.
            // They are slower than the player's, so a shot seen coming can be escaped.
            Vector2 direction = (target - eye).normalized;
            for (int p = 0; p < weapon.Pellets; p++)
            {
                Vector2 pellet = Quaternion.Euler(0f, 0f, Random.Range(-weapon.Spread, weapon.Spread)) * direction;
                run.Bullets.Add(new BulletRVO
                {
                    Position = eye, Velocity = pellet * weapon.BulletSpeed * guard.BulletSpeedScale, Damage = 1,
                    Life = weapon.BulletLife / Mathf.Max(0.2f, guard.BulletSpeedScale),
                });
            }
            run.GuardShots++;
            run.GuardShotFrom = eye + direction * 0.7f;
            run.GuardShotDirection = direction;
            guard.FireCooldown = Mathf.Max(0.15f, weapon.FireInterval * guard.FirePace * guard.FireRateScale);
        }

        // Runs to a crate between itself and the player, then crouches behind it and pops up to shoot in turns.
        // False when there is no crate worth having, and the guard fights in the open as before.
        private bool TakeCover(GuardRVO guard, ContractRVO run, bool sees, float dx, Vector2 eye, Vector2 target, float dt)
        {
            if (!OnFloorOf(guard.Position, run.Player, run)) { guard.HasCover = false; guard.Crouching = false; return false; }
            // A crate is only cover while it stands between the guard's spot and the player.
            if (guard.HasCover && (Mathf.Sign(run.Player.x - guard.CoverSpot) != Mathf.Sign(run.Player.x - CoverNear(guard.CoverSpot, run))
                                   || Mathf.Abs(run.Player.x - guard.CoverSpot) < 1.2f))
                guard.HasCover = false;
            if (!guard.HasCover && !FindCover(guard, run)) { guard.Crouching = false; return false; }
            float gap = guard.CoverSpot - guard.Position.x;
            if (Mathf.Abs(gap) > 0.1f)
            {
                // On the way there: upright, running, not stopping to shoot.
                guard.Crouching = false;
                guard.Alerted = true;
                guard.Facing = gap > 0 ? 1 : -1;
                bool was = guard.Investigating;
                guard.Investigating = true;
                float move = Mathf.Min(Mathf.Abs(gap), guard.AlertSpeed * dt);
                guard.Position.x += Mathf.Sign(gap) * move;
                guard.Investigating = was;
                guard.PeekTimer = 0f;
                return true;
            }
            guard.Investigating = false;
            guard.Facing = dx >= 0 ? 1 : -1;
            // Down, then up to shoot, then down again.
            guard.PeekTimer += dt;
            float cycle = guard.HideTime + guard.PeekTime;
            if (guard.PeekTimer >= cycle) guard.PeekTimer -= cycle;
            guard.Crouching = guard.PeekTimer < guard.HideTime;
            if (guard.Crouching) guard.FireCooldown = Mathf.Max(guard.FireCooldown, guard.ReactionTime * 0.5f);
            else if (sees) Shoot(guard, run, eye, target, dt);
            return true;
        }

        // The nearest crate on this floor, within reach, that has the player on its far side.
        private bool FindCover(GuardRVO guard, ContractRVO run)
        {
            float best = float.MaxValue;
            foreach (Vector2 cover in run.Covers)
            {
                if (!OnFloorOf(cover, guard.Position, run)) continue;
                float toPlayer = run.Player.x - cover.x;
                if (Mathf.Abs(toPlayer) < 1.6f) continue;
                float spot = cover.x - Mathf.Sign(toPlayer) * 0.6f;
                float distance = Mathf.Abs(spot - guard.Position.x);
                if (distance > guard.CoverSearch || distance >= best || Taken(spot, guard, run)) continue;
                // Never run past the player to get to it.
                if (Mathf.Sign(run.Player.x - guard.Position.x) == Mathf.Sign(spot - guard.Position.x)
                    && Mathf.Abs(spot - guard.Position.x) > Mathf.Abs(run.Player.x - guard.Position.x) - 1f) continue;
                best = distance;
                guard.CoverSpot = spot;
            }
            guard.HasCover = best < float.MaxValue;
            return guard.HasCover;
        }

        // One guard to a crate side: a spot another fighting guard already holds is not free.
        private static bool Taken(float spot, GuardRVO guard, ContractRVO run)
        {
            foreach (GuardRVO other in run.Guards)
                if (other != guard && other.Alive && other.HasCover && Mathf.Abs(other.CoverSpot - spot) < 0.5f
                    && Mathf.Abs(other.Position.y - guard.Position.y) < 1f) return true;
            return false;
        }

        private float CoverNear(float spot, ContractRVO run)
        {
            float best = spot;
            float gap = float.MaxValue;
            foreach (Vector2 cover in run.Covers)
                if (Mathf.Abs(cover.x - spot) < gap) { gap = Mathf.Abs(cover.x - spot); best = cover.x; }
            return best;
        }

        // Runs to the nearest guard who does not know yet and tells them - and anyone beside them - where the
        // player was seen. True while it is busy with that; false once nobody is left to warn.
        private bool RaiseAlarm(GuardRVO guard, ContractRVO run, float dt)
        {
            GuardRVO[] guards = run.Guards;
            if (guard.Warning < 0 || guard.Warning >= guards.Length || !Unwarned(guards[guard.Warning]))
            {
                guard.Warning = -1;
                float best = float.MaxValue;
                for (int i = 0; i < guards.Length; i++)
                {
                    GuardRVO other = guards[i];
                    if (other == guard || !Unwarned(other)) continue;
                    float distance = Mathf.Abs(other.Position.x - guard.Position.x) + Mathf.Abs(other.Position.y - guard.Position.y) * 3f;
                    if (distance < best) { best = distance; guard.Warning = i; }
                }
                if (guard.Warning < 0)
                {
                    guard.AlarmDone = true;
                    return false;
                }
            }
            GuardRVO told = guards[guard.Warning];
            guard.Crouching = false;
            guard.HasCover = false;
            guard.Alerted = true;
            if (OnFloorOf(guard.Position, told.Position, run) && Mathf.Abs(told.Position.x - guard.Position.x) < 1.4f)
            {
                foreach (GuardRVO other in guards)
                {
                    if (!other.Alive || other.Faction != Faction.Enemy || other == guard) continue;
                    if (other != told && (!OnFloorOf(other.Position, told.Position, run) || Mathf.Abs(other.Position.x - told.Position.x) > guard.AlarmRadius)) continue;
                    other.Alerted = true;
                    other.Suspicion = 1f;
                    other.Chase = other.ChaseMemory + 2f;
                    other.LastSeen = guard.LastSeen;
                    other.StunRemaining = 0f;
                    // Told, not seen: they come to fight; only one who saw the player for themselves runs to warn.
                    other.AlarmDone = true;
                    Investigate(other, guard.LastSeen);
                }
                guard.Warning = -1;
                return true;
            }
            // Keeps the fight alive while it runs: it still knows where the player was.
            guard.Chase = Mathf.Max(guard.Chase, 1f);
            Investigate(guard, told.Position);
            guard.Wait = 1f;
            float goal = told.Position.x;
            if (!OnFloorOf(guard.Position, told.Position, run))
            {
                StairRVO stair = StairTowards(guard, run);
                if (stair == null) { guard.AlarmDone = true; return false; }
                goal = stair.Position.x;
                if (Mathf.Abs(goal - guard.Position.x) < 0.15f)
                {
                    guard.Position = run.Stairs[stair.Link].Position;
                    return true;
                }
            }
            guard.Facing = goal > guard.Position.x ? 1 : -1;
            Step(guard, run, dt, guard.AlertSpeed);
            return true;
        }

        private static bool Unwarned(GuardRVO other) => other.Alive && other.Faction == Faction.Enemy && other.Suspicion < 1f;

        private void Patrol(GuardRVO guard, ContractRVO run, float dt, float speed)
        {
            if (guard.Position.x < guard.Left) guard.Facing = 1;
            else if (guard.Position.x > guard.Right) guard.Facing = -1;
            Step(guard, run, dt, speed);
        }
        private void Step(GuardRVO guard, ContractRVO run, float dt, float speed)
        {
            float next = guard.Position.x + guard.Facing * dt * speed;
            foreach (DoorRVO door in run.Doors)
            {
                if (door.Open || Mathf.Abs(door.Position.y - guard.Position.y) > 0.1f || Mathf.Abs(next - door.Position.x) > 0.65f) continue;
                // A guard with somewhere to be opens the door; a patrolling guard turns back.
                if (guard.Investigating) door.Open = true;
                else guard.Facing *= -1;
            }
            guard.Position.x += guard.Facing * dt * speed;
        }
        private void Investigate(GuardRVO guard, Vector2 target)
        {
            guard.Investigating = true;
            guard.Target = target;
            guard.Wait = guard.InvestigateTime;
        }
        private bool OnFloorOf(Vector2 a, Vector2 b, ContractRVO run) => FloorY(a.y, run) == FloorY(b.y, run);
        private float FloorY(float y, ContractRVO run) => y > run.FloorHeight * 0.5f ? run.FloorHeight : 0f;
        // The nearest stairwell door on the guard's floor that leads to the floor they are heading for.
        private StairRVO StairTowards(GuardRVO guard, ContractRVO run)
        {
            float here = FloorY(guard.Position.y, run);
            float there = FloorY(guard.Target.y, run);
            StairRVO best = null;
            foreach (StairRVO stair in run.Stairs)
            {
                if (FloorY(stair.Position.y, run) != here || FloorY(run.Stairs[stair.Link].Position.y, run) != there) continue;
                if (best == null || Mathf.Abs(stair.Position.x - guard.Position.x) < Mathf.Abs(best.Position.x - guard.Position.x))
                    best = stair;
            }
            return best;
        }
        private bool SeesBody(GuardRVO guard, ContractRVO run)
        {
            LightRVO room = _model.LightAt(guard.Position);
            if (room != null && !room.On) return false;
            foreach (GuardRVO body in run.Guards)
            {
                if (body.Alive || Mathf.Abs(body.Position.y - guard.Position.y) > 0.1f) continue;
                float dx = body.Position.x - guard.Position.x;
                if (Mathf.Abs(dx) > 6f || Mathf.Sign(dx) != guard.Facing) continue;
                if (!Clear(guard.Position + Vector2.up * 1.15f, body.Position + Vector2.up * 0.2f)) continue;
                Investigate(guard, new Vector2(body.Position.x - guard.Facing * 0.8f, body.Position.y));
                return true;
            }
            return false;
        }
        private bool Clear(Vector2 from, Vector2 to) =>
            _functions.Call<TraceContractFunction>().AddParams(from, to).ExecuteAndGetResult<float>() > 0.99f;
    }
}
