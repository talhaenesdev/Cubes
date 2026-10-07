using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // What a broken prop does next: a lamp falls and crushes whoever is under it, a barrel explodes -
    // killing in its radius, blowing doors open and setting off the barrels beside it.
    internal class ResolvePropsCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (!run.Alive || run.Won || run.Paused) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            foreach (PropRVO prop in run.Props)
            {
                if (prop.Broken && !prop.Resolved)
                {
                    prop.Resolved = true;
                    if (prop.Kind == PropKind.CeilingLamp) prop.Falling = true;
                    else if (prop.Kind == PropKind.Barrel) Explode(run, prop.Position + prop.Hit);
                    else Noise(run, prop.Position + prop.Hit, 4f);
                }
                if (!prop.Falling) continue;
                prop.FallSpeed += 22f * dt;
                prop.Lift -= prop.FallSpeed * dt;
                float floor = -(prop.Hit.y - prop.Size.y * 0.5f);
                if (prop.Lift > floor) continue;
                prop.Lift = floor;
                prop.Falling = false;
                Crush(run, prop);
            }
        }
        private void Crush(ContractRVO run, PropRVO prop)
        {
            Vector2 landed = prop.Position + new Vector2(prop.Hit.x, 0.1f);
            AddImpact(run, landed, Vector2.up, ImpactKind.Glass);
            Noise(run, landed, 7f);
            foreach (GuardRVO guard in run.Guards)
                if (guard.Alive && Mathf.Abs(guard.Position.y - prop.Position.y) < 0.2f && Mathf.Abs(guard.Position.x - landed.x) < _model.Tuning.CrushHalfWidth)
                    _model.DamageGuard(guard, _model.Tuning.CrushDamage);
            if (!run.InVent && Mathf.Abs(run.Player.y - prop.Position.y) < 0.2f && Mathf.Abs(run.Player.x - landed.x) < _model.Tuning.CrushHalfWidth)
                _model.DamagePlayer(_model.Tuning.CrushDamage, "Crushed under a falling lamp.");
        }
        private void Explode(ContractRVO run, Vector2 center)
        {
            AddImpact(run, center, Vector2.up, ImpactKind.Explosion);
            Noise(run, center, 16f);
            foreach (GuardRVO guard in run.Guards)
                if (guard.Alive && Vector2.Distance(guard.Position + Vector2.up * 0.8f, center) < _model.Tuning.BlastRadius)
                    _model.DamageGuard(guard, _model.Tuning.BlastDamage);
            if (!run.InVent && Vector2.Distance(run.Player + Vector2.up * 0.8f, center) < _model.Tuning.BlastRadius)
                _model.DamagePlayer(_model.Tuning.BlastDamage, "Caught in the blast.");
            foreach (DoorRVO door in run.Doors)
                if (!door.Open && Vector2.Distance(door.Position + Vector2.up, center) < _model.Tuning.BlastRadius + 0.5f) door.Open = true;
            // The chain goes off next frame, one barrel after another.
            foreach (PropRVO other in run.Props)
                if (!other.Broken && other.Kind == PropKind.Barrel && Vector2.Distance(other.Position + other.Hit, center) < _model.Tuning.ChainRadius) other.Broken = true;
        }
        private void Noise(ContractRVO run, Vector2 at, float radius)
        {
            run.NoiseRemaining = 0.5f;
            run.NoiseRadius = Mathf.Max(radius, run.NoiseRemaining > 0f ? run.NoiseRadius : 0f);
            run.NoisePosition = at;
        }
        private void AddImpact(ContractRVO run, Vector2 at, Vector2 normal, ImpactKind kind)
        {
            run.Impacts.Add(new ImpactRVO {Id = ++run.ImpactCount, Position = at, Normal = normal, Kind = kind});
            if (run.Impacts.Count > 80) run.Impacts.RemoveAt(0);
        }
    }
}
