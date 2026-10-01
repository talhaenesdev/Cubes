using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    internal class MoveAgentCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (!run.Alive || run.Won || run.Paused) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            ContractInputVO input = _model.Input;
            run.Elapsed += dt;
            run.Aim = input.Aim;
            run.Crouching = input.Crouch;
            run.ShotCooldown = Mathf.Max(0, run.ShotCooldown - dt);
            run.MeleeCooldown = Mathf.Max(0, run.MeleeCooldown - dt);
            run.NoiseRemaining = Mathf.Max(0, run.NoiseRemaining - dt);
            run.FlashRemaining = Mathf.Max(0, run.FlashRemaining - dt);
            if (run.ReloadRemaining > 0)
            {
                run.ReloadRemaining = Mathf.Max(0, run.ReloadRemaining - dt);
                WeaponCVO weapon = _model.Weapon(run.Weapon);
                if (run.ReloadRemaining == 0 && weapon != null && run.Ammo < weapon.Magazine && run.Reserve > 0)
                {
                    // Round by round, holding R to keep going; or the whole magazine at once.
                    int rounds = weapon.Reload == ReloadMode.PerRound ? 1 : Mathf.Min(weapon.Magazine - run.Ammo, run.Reserve);
                    run.Ammo += rounds;
                    run.Reserve -= rounds;
                    if (weapon.Reload == ReloadMode.PerRound && input.ReloadHeld && run.Ammo < weapon.Magazine && run.Reserve > 0)
                        run.ReloadRemaining = weapon.ReloadTime * run.ReloadScale;
                }
            }
            if (run.InVent)
            {
                if (Mathf.Abs(input.Move) < 0.1f && !input.Jump) { run.Hidden = true; return; }
                run.InVent = false;
                run.Vent = -1;
            }
            {
                float next = Mathf.Clamp(run.Player.x + input.Move * (run.Crouching ? 2.25f : 4.7f) * run.MoveScale * dt, 0.4f - run.HalfWidth, run.HalfWidth - 0.4f);
                foreach (DoorRVO door in run.Doors)
                {
                    if (door.Open || Mathf.Abs(run.Player.y - door.Position.y) > 1.8f) continue;
                    if (Mathf.Abs(next - door.Position.x) < 0.5f)
                        next = door.Position.x + Mathf.Sign(run.Player.x - door.Position.x) * 0.5f;
                }
                run.Player.x = next;
                float ground = run.Floor * run.FloorHeight;
                if (input.Jump && run.Player.y <= ground + 0.02f) run.VerticalSpeed = 7f;
                run.VerticalSpeed -= 22f * dt;
                run.Player.y = Mathf.Max(ground, run.Player.y + run.VerticalSpeed * dt);
                if (run.Player.y <= ground) run.VerticalSpeed = 0;
            }
            run.Hidden = false;
            LightRVO room = _model.LightAt(run.Player);
            if (run.Crouching && room != null && !room.On && run.NoiseRemaining <= 0) run.Hidden = true;
            // Cover no longer makes the player invisible; the crate itself blocks sight and bullets (TraceContractFunction).
            run.InCover = false;
            foreach (Vector2 cover in run.Covers)
                if (run.Crouching && Mathf.Abs(run.Player.y - cover.y) < 0.5f && Mathf.Abs(run.Player.x - cover.x) < 1.2f)
                    run.InCover = true;
        }
    }
}
