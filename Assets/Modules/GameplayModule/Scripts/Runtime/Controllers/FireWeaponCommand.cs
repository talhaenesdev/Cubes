using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Function.Provider;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    internal class FireWeaponCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (!run.Alive || run.Won || run.Paused || run.InVent) return;
            Vector2 origin = run.Player + Vector2.up * (run.Crouching ? 0.65f : 1.15f);
            if (_model.Input.Melee && run.MeleeCooldown <= 0)
            {
                run.MeleeCooldown = 0.45f;
                foreach (GuardRVO guard in run.Guards)
                {
                    if (!guard.Alive || Vector2.Distance(run.Player, guard.Position) > 1.6f) continue;
                    float clear = _functions.Call<TraceContractFunction>().AddParams(origin, guard.Position + Vector2.up).ExecuteAndGetResult<float>();
                    if (clear < 0.99f) continue;
                    // An unaware, turned-away or knocked-down guard dies; a guard squaring up to you is only shoved.
                    bool facingPlayer = Mathf.Sign(run.Player.x - guard.Position.x) == guard.Facing;
                    bool lethal = guard.StunRemaining > 0 || !facingPlayer || guard.Suspicion < 1f;
                    run.ShotFrom = origin;
                    run.ShotTo = guard.Position + Vector2.up;
                    run.FlashRemaining = 0.12f;
                    if (lethal)
                    {
                        guard.Alive = false;
                        run.SilentKills++;
                    }
                    else guard.StunRemaining = 1.2f;
                    break;
                }
            }
            WeaponCVO weapon = _model.Weapon(run.Weapon);
            if (weapon == null) return;
            bool trigger = weapon.Automatic ? _model.Input.FireHeld || _model.Input.Fire : _model.Input.Fire;
            if (!trigger || run.Ammo <= 0 || run.ShotCooldown > 0) return;
            // A round-by-round reload is cut short by firing; a magazine change has to finish.
            if (run.ReloadRemaining > 0 && weapon.Reload == ReloadMode.Magazine) return;
            run.ReloadRemaining = 0;
            run.Ammo--;
            run.Shots++;
            run.ShotCooldown = weapon.FireInterval;
            run.NoiseRemaining = weapon.NoiseRadius > 0f ? 0.65f : 0f;
            run.NoiseRadius = weapon.NoiseRadius;
            run.NoisePosition = run.Player;
            run.Hidden = false;
            Vector2 direction = (run.Aim - origin).normalized;
            if (direction.sqrMagnitude < 0.1f) direction = Vector2.right;
            Vector2 muzzle = origin + direction * 0.85f;
            for (int i = 0; i < weapon.Pellets; i++)
            {
                Vector2 pellet = Quaternion.Euler(0f, 0f, Random.Range(-weapon.Spread, weapon.Spread)) * direction;
                run.Bullets.Add(new BulletRVO
                {
                    Position = origin, Velocity = pellet * weapon.BulletSpeed * Random.Range(0.94f, 1.06f),
                    FromPlayer = true, Damage = weapon.Damage, Life = weapon.BulletLife,
                });
            }
            run.ShotFrom = muzzle;
            run.ShotTo = muzzle + direction;
            run.FlashRemaining = 0.085f;
        }
    }
}
