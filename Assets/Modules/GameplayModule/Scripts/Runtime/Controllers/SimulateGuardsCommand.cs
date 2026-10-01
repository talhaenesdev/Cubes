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
    // through the stairwell doors - and shoot once they have fully seen them.
    internal class SimulateGuardsCommand : Command
    {
        // How long a guard keeps tracking the player after losing sight of them.
        private const float ChaseMemory = 3f;
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
                float range = playerDark && run.NoiseRemaining <= 0 ? 2.2f : run.Crouching ? guard.SightRange * 0.6f : guard.SightRange;
                bool sameFloor = Mathf.Abs(run.Player.y - guard.Position.y) < 1.8f;
                bool sees = !run.InVent && !run.Hidden && sameFloor && Mathf.Abs(dx) < range
                    && (Mathf.Sign(dx) == guard.Facing || guard.Alerted || Mathf.Abs(dx) < 1.3f)
                    && Clear(eye, target);
                if (run.NoiseRemaining > 0 && Vector2.Distance(run.NoisePosition, guard.Position) < run.NoiseRadius)
                {
                    guard.Alerted = true;
                    Investigate(guard, run.NoisePosition);
                }
                if (!guard.Alerted && SeesBody(guard, run)) guard.Alerted = true;
                if (!guard.Investigating && room != null && !room.On) Investigate(guard, new Vector2(room.Switch, room.Floor));
                guard.Suspicion = Mathf.Clamp01(guard.Suspicion + dt * (sees ? 1.6f : -0.6f));
                if (sees && guard.Suspicion >= 1f) guard.Chase = ChaseMemory;
                else guard.Chase = Mathf.Max(0f, guard.Chase - dt);
                // A spotted player is hunted wherever they go, until they vanish into a vent or the dark, or the trail goes cold.
                if (!sees && guard.Chase > 0f && !run.InVent && !run.Hidden) Investigate(guard, run.Player);
                if (guard.Suspicion >= 1f && sees)
                {
                    guard.Alerted = true;
                    guard.Facing = dx >= 0 ? 1 : -1;
                    guard.FireCooldown -= dt;
                    WeaponCVO weapon = _model.Weapon(guard.Weapon);
                    if (guard.FireCooldown <= 0 && weapon != null)
                    {
                        // Guards fire real bullets at where the player is now; ducking or leaving their path dodges them.
                        // They are slower than the player's, so a shot seen coming can be escaped.
                        Vector2 direction = (target - eye).normalized;
                        for (int p = 0; p < weapon.Pellets; p++)
                        {
                            Vector2 pellet = Quaternion.Euler(0f, 0f, Random.Range(-weapon.Spread, weapon.Spread)) * direction;
                            run.Bullets.Add(new BulletRVO
                            {
                                Position = eye, Velocity = pellet * weapon.BulletSpeed * 0.6f * guard.BulletSpeedScale, Damage = 1,
                                Life = weapon.BulletLife / 0.6f,
                            });
                        }
                        run.GuardShots++;
                        run.GuardShotFrom = eye + direction * 0.7f;
                        run.GuardShotDirection = direction;
                        guard.FireCooldown = Mathf.Max(0.2f, weapon.FireInterval * 3f * guard.FireRateScale);
                    }
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
            guard.Wait = 1.5f;
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
