using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Function.Provider;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // E does one thing per press: the first interaction in reach, in this order, owns both the hint and the press.
    internal class InteractContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        private bool _press;
        private string _hint;
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (!run.Alive || run.Won || run.Paused) return;
            _press = _model.Input.Interact;
            _hint = null;
            CollectRounds(run);
            if (_model.Input.Swap && !run.InVent) SwapWeapons(run);
            // Crouching over a weapon you do not carry makes E pick it up before anything else in reach.
            if (run.Crouching && !run.InVent) OfferPickup(run);
            if (run.InVent && Offer("IN VENT  /  E  —  crawl to the linked vent    A / D  —  climb out"))
            {
                run.Vent = run.Vents[run.Vent].Link;
                run.Player = run.Vents[run.Vent].Position;
                run.Floor = run.Player.y > run.FloorHeight * 0.5f ? 1 : 0;
            }
            foreach (StairRVO stair in run.Stairs)
            {
                if (run.InVent || Vector2.Distance(run.Player, stair.Position) > 0.8f) continue;
                StairRVO exit = run.Stairs[stair.Link];
                if (!Offer("E  —  take the stairs " + (exit.Position.y > stair.Position.y ? "up" : "down"))) continue;
                run.Player = exit.Position;
                run.Floor = run.Player.y > run.FloorHeight * 0.5f ? 1 : 0;
                run.VerticalSpeed = 0;
                break;
            }
            for (int i = 0; i < run.Vents.Length && !run.InVent; i++)
            {
                if (Vector2.Distance(run.Player, run.Vents[i].Position) > 0.8f) continue;
                if (!Offer("E  —  enter vent")) continue;
                run.InVent = true;
                run.Hidden = true;
                run.Vent = i;
                run.Player = run.Vents[i].Position;
                run.Floor = run.Player.y > run.FloorHeight * 0.5f ? 1 : 0;
                run.VerticalSpeed = 0;
            }
            foreach (LightRVO light in run.Lights)
            {
                if (run.InVent || Mathf.Abs(run.Player.y - light.Floor) > 1.8f || Mathf.Abs(run.Player.x - light.Switch) > 0.7f) continue;
                if (Offer("E  —  " + (light.On ? "kill the lights" : "turn the lights on"))) light.On = !light.On;
            }
            for (int i = 0; i < run.Stations.Length; i++)
            {
                StationRVO station = run.Stations[i];
                if (run.InVent || Mathf.Abs(run.Player.y - station.Position.y) > 0.5f || Mathf.Abs(run.Player.x - station.Position.x) > 1f) continue;
                if (!Offer("E  —  " + (station.Kind == Enums.StationKind.MissionBoard ? "mission board" : "upgrade bench"))) continue;
                run.Menu = i;
                run.Paused = true;
                MenuVO menu = _functions.Call<BuildMenuFunction>().AddParams(station.Kind).ExecuteAndGetResult<MenuVO>();
                _signals.Outgoing.MenuOpened.Dispatch(menu.Title, menu.Subtitle, menu.Entries);
            }
            for (int i = 0; i < run.Terminals.Length; i++)
            {
                TerminalRVO terminal = run.Terminals[i];
                if (run.InVent || Mathf.Abs(run.Player.y - terminal.Position.y) > 0.5f || Mathf.Abs(run.Player.x - terminal.Position.x) > 1f) continue;
                if (!Offer("E  —  use the computer: " + terminal.Title)) continue;
                run.Terminal = i;
                run.Paused = true;
                _signals.Outgoing.TerminalOpened.Dispatch(terminal.Title, terminal.Text);
            }
            foreach (DoorRVO door in run.Doors)
            {
                if (run.InVent || Vector2.Distance(run.Player, door.Position) > 1.6f) continue;
                if (!Offer("E  —  " + (door.Open ? "close door / break line of sight" : "kick door / knock down whoever stands behind it"))) continue;
                door.Open = !door.Open;
                if (!door.Open) continue;
                run.NoiseRemaining = 0.3f;
                run.NoiseRadius = 4f;
                run.NoisePosition = door.Position;
                foreach (GuardRVO guard in run.Guards)
                    if (guard.Alive && Mathf.Abs(guard.Position.y - door.Position.y) < 0.1f && Mathf.Abs(guard.Position.x - door.Position.x) < 1.8f)
                        guard.StunRemaining = 2.5f;
            }
            OfferPickup(run);
            WeaponCVO held = _model.Weapon(run.Weapon);
            if (_model.Input.Reload && held != null && run.Ammo < held.Magazine && run.Reserve > 0 && run.ReloadRemaining == 0)
                run.ReloadRemaining = held.ReloadTime * run.ReloadScale;
            if (!run.InHideout && _model.Remaining == 0 && !run.InVent)
            {
                if (Mathf.Abs(run.Player.x - run.ExitX) < 1.3f && run.Player.y < 0.5f)
                {
                    if (Offer("E  —  extract")) run.Won = true;
                }
                else _hint ??= "BUILDING CLEAR  /  Return to the green exit.";
            }
            run.Hint = _hint ?? (run.Hidden
                ? "HIDDEN  /  Guards cannot see you. F behind an unaware guard is a silent kill."
                : run.InCover
                    ? "IN COVER  /  Safe from the far side only. Your shots hit the crate - stand up to fire over it."
                    : run.InHideout ? "HIDEOUT  /  Take a contract from the mission board. Spend cash at the upgrade bench."
                    : "Eliminate every enemy, then return to the green exit.");
        }
        // Walking over a weapon you already carry, in hand or in the pocket, empties its rounds into yours.
        private void CollectRounds(ContractRVO run)
        {
            for (int i = run.Pickups.Count - 1; i >= 0; i--)
            {
                PickupRVO pickup = run.Pickups[i];
                if (run.InVent || !InReach(run, pickup)) continue;
                if (pickup.Weapon == run.Weapon) run.Reserve += pickup.Ammo + pickup.Reserve;
                else if (pickup.Weapon == run.Holster) run.HolsterReserve += pickup.Ammo + pickup.Reserve;
                else continue;
                run.Pickups.RemoveAt(i);
            }
        }

        private void SwapWeapons(ContractRVO run)
        {
            if (string.IsNullOrEmpty(run.Holster)) return;
            (run.Weapon, run.Holster) = (run.Holster, run.Weapon);
            (run.Ammo, run.HolsterAmmo) = (run.HolsterAmmo, run.Ammo);
            (run.Reserve, run.HolsterReserve) = (run.HolsterReserve, run.Reserve);
            run.ReloadRemaining = 0;
            run.ShotCooldown = 0.3f;
        }

        // A weapon you do not carry: into the hand, with the one you held going to an empty pocket, or to the floor.
        private void OfferPickup(ContractRVO run)
        {
            for (int i = 0; i < run.Pickups.Count; i++)
            {
                PickupRVO pickup = run.Pickups[i];
                if (run.InVent || !InReach(run, pickup) || pickup.Weapon == run.Weapon || pickup.Weapon == run.Holster) continue;
                WeaponCVO found = _model.Weapon(pickup.Weapon);
                if (found == null) continue;
                WeaponCVO held = _model.Weapon(run.Weapon);
                bool pocket = string.IsNullOrEmpty(run.Holster) && held != null;
                string then = held == null ? "" : pocket ? "  (" + held.DisplayName + " to the pocket)" : "  (drops the " + held.DisplayName + ")";
                if (!Offer("E  —  pick up the " + found.DisplayName + then)) continue;
                run.Pickups.RemoveAt(i);
                if (pocket)
                {
                    run.Holster = run.Weapon;
                    run.HolsterAmmo = run.Ammo;
                    run.HolsterReserve = run.Reserve;
                }
                else if (held != null) _model.Drop(run.Player, run.Weapon, run.Ammo, run.Reserve);
                run.Weapon = pickup.Weapon;
                run.Ammo = pickup.Ammo;
                run.Reserve = pickup.Reserve;
                run.ReloadRemaining = 0;
                run.ShotCooldown = 0.25f;
                return;
            }
        }

        private bool InReach(ContractRVO run, PickupRVO pickup) =>
            Mathf.Abs(run.Player.y - pickup.Position.y) < 0.6f && Mathf.Abs(run.Player.x - pickup.Position.x) < 1f;

        private bool Offer(string text)
        {
            if (_hint != null) return false;
            _hint = text;
            if (!_press) return false;
            _press = false;
            return true;
        }
    }
}
