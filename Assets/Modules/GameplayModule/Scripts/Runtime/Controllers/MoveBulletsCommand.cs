using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Function.Provider;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // Steps every bullet in flight: the first body or wall along this frame's segment stops it and leaves a mark.
    internal class MoveBulletsCommand : Command
    {
        private const int MaxImpacts = 80;
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (!run.Alive || run.Won || run.Paused) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            for (int d = run.Disturbances.Count - 1; d >= 0; d--)
                if ((run.Disturbances[d].Life -= dt) <= 0f) run.Disturbances.RemoveAt(d);
            for (int i = run.Bullets.Count - 1; i >= 0; i--)
            {
                BulletRVO bullet = run.Bullets[i];
                bullet.Life -= dt;
                Vector2 step = bullet.Velocity * dt;
                Vector2 direction = step.normalized;
                float fraction = _functions.Call<TraceContractFunction>().AddParams(bullet.Position, bullet.Position + step).ExecuteAndGetResult<float>();
                float length = step.magnitude * fraction;
                float nearest = length;
                GuardRVO hitGuard = null;
                bool hitPlayer = false;
                if (bullet.FromPlayer)
                {
                    foreach (GuardRVO guard in run.Guards)
                        if (guard.Alive && !guard.Escaped && Crosses(bullet.Position, direction, guard.Position, guard.Crouching ? 1.0f : 1.9f, ref nearest))
                            hitGuard = guard;
                }
                else if (!run.InVent)
                    hitPlayer = Crosses(bullet.Position, direction, run.Player, run.Crouching ? 1.15f : 1.9f, ref nearest);
                // A prop nearer than any body takes the bullet instead.
                PropRVO hitProp = null;
                foreach (PropRVO prop in run.Props)
                {
                    if (prop.Broken || !Box(bullet.Position, direction, prop.Position + prop.Hit + Vector2.up * prop.Lift, prop.Size, ref nearest)) continue;
                    hitProp = prop;
                    hitGuard = null;
                    hitPlayer = false;
                }
                Vector2 point = bullet.Position + direction * nearest;
                if (hitProp != null)
                {
                    hitProp.Broken = true;
                    AddImpact(run, point, -direction, false, PropImpact(hitProp.Kind));
                    run.Bullets.RemoveAt(i);
                    continue;
                }
                if (hitGuard != null || hitPlayer)
                {
                    if (hitGuard != null) _model.DamageGuard(hitGuard, bullet.Damage);
                    else _model.DamagePlayer(bullet.Damage, "A guard's bullet found you.");
                    Vector2 feet = hitGuard != null ? hitGuard.Position : run.Player;
                    AddImpact(run, point, -direction, true);
                    AddImpact(run, new Vector2(feet.x + direction.x * 0.9f, feet.y + 0.02f), Vector2.up, true);
                    run.Bullets.RemoveAt(i);
                    continue;
                }
                if (fraction < 1f)
                {
                    AddImpact(run, point, -direction, false, Surface(run, point, direction));
                    // Where the player's bullet struck is worth a look to whoever is near enough.
                    if (bullet.FromPlayer && _model.Tuning.ImpactCuriosityTime > 0f)
                        run.Disturbances.Add(new DisturbanceRVO {Position = point, Life = _model.Tuning.ImpactCuriosityTime});
                    run.Bullets.RemoveAt(i);
                    continue;
                }
                bullet.Position += step;
                if (bullet.Life <= 0) run.Bullets.RemoveAt(i);
            }
        }
        // True when the segment passes through a standing body before the current nearest hit.
        private bool Crosses(Vector2 start, Vector2 direction, Vector2 feet, float height, ref float nearest)
        {
            if (Mathf.Abs(direction.x) < 0.0001f) return false;
            float along = (feet.x - Mathf.Sign(direction.x) * 0.3f - start.x) / direction.x;
            if (along < 0 || along > nearest) return false;
            float y = start.y + direction.y * along;
            if (y < feet.y || y > feet.y + height) return false;
            nearest = along;
            return true;
        }
        // True when the segment enters the box before the current nearest hit.
        private bool Box(Vector2 start, Vector2 direction, Vector2 center, Vector2 size, ref float nearest)
        {
            float near = 0f;
            float far = nearest;
            for (int axis = 0; axis < 2; axis++)
            {
                float min = center[axis] - size[axis] * 0.5f;
                float max = center[axis] + size[axis] * 0.5f;
                if (Mathf.Abs(direction[axis]) < 0.0001f)
                {
                    if (start[axis] < min || start[axis] > max) return false;
                    continue;
                }
                float a = (min - start[axis]) / direction[axis];
                float b = (max - start[axis]) / direction[axis];
                near = Mathf.Max(near, Mathf.Min(a, b));
                far = Mathf.Min(far, Mathf.Max(a, b));
                if (near > far) return false;
            }
            nearest = near;
            return true;
        }
        private ImpactKind PropImpact(PropKind kind) => kind switch
        {
            PropKind.Monitor => ImpactKind.Metal,
            PropKind.Barrel => ImpactKind.Metal,
            PropKind.Plant => ImpactKind.Dirt,
            _ => ImpactKind.Glass,
        };
        // What the bullet stopped in: the ceiling above it, the floor below, a crate or door, or a wall.
        private ImpactKind Surface(ContractRVO run, Vector2 point, Vector2 direction)
        {
            float h = run.FloorHeight;
            if (direction.y > 0.2f && (Mathf.Abs(point.y - (h - 0.25f)) < 0.2f || Mathf.Abs(point.y - (h * 2f - 0.05f)) < 0.2f)) return ImpactKind.Ceiling;
            if (direction.y < -0.2f && (Mathf.Abs(point.y) < 0.2f || Mathf.Abs(point.y - h) < 0.2f)) return ImpactKind.Floor;
            foreach (Vector2 cover in run.Covers)
                if (Mathf.Abs(point.x - cover.x) < 0.2f && point.y >= cover.y - 0.05f && point.y <= cover.y + ContractRVO.CoverHeight + 0.05f) return ImpactKind.Wood;
            foreach (DoorRVO door in run.Doors)
                if (!door.Open && Mathf.Abs(point.x - door.Position.x) < 0.35f && point.y >= door.Position.y && point.y <= door.Position.y + 2.8f) return ImpactKind.Wood;
            return ImpactKind.Wall;
        }
        private void AddImpact(ContractRVO run, Vector2 position, Vector2 normal, bool blood, ImpactKind kind = ImpactKind.Blood)
        {
            run.Impacts.Add(new ImpactRVO {Id = ++run.ImpactCount, Position = position, Normal = normal, Blood = blood, Kind = blood ? ImpactKind.Blood : kind});
            if (run.Impacts.Count > MaxImpacts) run.Impacts.RemoveAt(0);
        }
    }
}
