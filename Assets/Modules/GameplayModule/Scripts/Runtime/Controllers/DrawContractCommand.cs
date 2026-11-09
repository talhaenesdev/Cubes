using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LocalizationModule.Services;
namespace Modules.GameplayModule.Controllers
{
    internal class DrawContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            _internalSignals.Draw.Dispatch(run);
            // Short: the gun and its rounds on top; underneath, what is happening or what E does here.
            WeaponCVO weapon = _model.Weapon(run.Weapon);
            string gun = weapon == null ? "" : weapon.Magazine <= 12
                ? _loc.Get(weapon.DisplayName) + "  " + new string('|', run.Ammo) + new string('.', weapon.Magazine - run.Ammo) + "  +" + run.Reserve
                : _loc.Get(weapon.DisplayName) + "  " + run.Ammo + "/" + weapon.Magazine + "  +" + run.Reserve;
            WeaponCVO pocket = _model.Weapon(run.Holster);
            if (pocket != null) gun += "     Q " + _loc.Get(pocket.DisplayName);
            string status = run.InHideout ? (_model.Tuning.UseMoney ? "$" + run.Cash : "")
                : gun + (_model.Tuning.UseMoney ? "     $" + run.CashCarried : "");
            string state = !run.Alive ? _loc.Get("DEAD  -  R to retry") : run.Won ? _loc.Get("CONTRACT COMPLETE") : run.Paused && run.Menu < 0 && run.Terminal < 0 && !run.DeckOpen && run.IntroRemaining <= 0f && run.Call < 0 && !run.BossDialogueOpen && !_model.MenuOpen ? _loc.Get("PAUSED") : "";
            string count = run.InHideout || Enemies(run) == 0 ? "" : (Enemies(run) - _model.Remaining) + " / " + Enemies(run);
            string objective = state != "" ? state : run.Hint != "" ? _loc.Get(run.Hint) : count;
            // A raid on one of our shops stays on the HUD, with its countdown, until it is over - not just a banner.
            string raid = state != "" ? "" : RaidAlert();
            if (raid != "") objective = objective == "" || run.InHideout ? raid : objective + "     " + raid;
            _signals.Outgoing.HudChanged.Dispatch(status, objective);
            // Where the player is, for the HUD's corner: HOME, or the contract's name.
            MissionCVO mission = _model.Missions != null ? _model.Missions.Find(_model.Mission) : null;
            _signals.Outgoing.LocationChanged.Dispatch(run.InHideout ? _loc.Get("HOME") : mission != null ? _loc.Get(mission.Title).Split('/')[0].Trim() : "");
            _model.ClearEdges();
        }
        // The shop under attack that ends soonest, unless it is the one being defended right now.
        private string RaidAlert()
        {
            ITerritoryModel territory = _model.Territory;
            if (territory == null || _model.Missions == null || _model.Run.IsLocationDefense) return "";
            long now = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            MissionCVO first = null;
            long left = long.MaxValue;
            foreach (MissionCVO mission in _model.Missions.Missions)
            {
                if (!territory.IsUnderAttack(mission.Id)) continue;
                long ends = territory.Location(mission.Id).AttackExpiresUnixMilliseconds - now;
                if (ends < left) { left = ends; first = mission; }
            }
            if (first == null) return "";
            left = System.Math.Max(0L, left);
            string name = _loc.Get(first.Title);
            if (name.Contains("/")) name = name[(name.IndexOf('/') + 1)..].Trim();
            return _loc.Format("! RAID ON {0}  {1}:{2:00}  -  MAP TO HELP", name, left / 60000, left / 1000 % 60);
        }

        private int Enemies(ContractRVO run)
        {
            int count = 0;
            foreach (GuardRVO guard in run.Guards) if (guard.Faction == Faction.Enemy) count++;
            return count;
        }
    }
}
