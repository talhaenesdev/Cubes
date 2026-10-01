using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    internal class DrawContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            _internalSignals.Draw.Dispatch(run);
            string stance = run.Menu >= 0 ? "AT STATION" : run.Terminal >= 0 ? "AT TERMINAL" : run.Paused ? "PAUSED / ESC TO RESUME" : !run.Alive ? "KILLED / R TO RETRY" :
                run.Won ? "CONTRACT COMPLETE" : run.InVent ? "IN VENT" : run.ReloadRemaining > 0 ? "RELOADING" : run.Hidden ? "HIDDEN" : run.InCover ? "IN COVER" : "EXPOSED";
            WeaponCVO weapon = _model.Weapon(run.Weapon);
            string cylinder = weapon == null ? "UNARMED" : weapon.Magazine <= 12
                ? weapon.DisplayName + " [" + new string('|', run.Ammo) + new string('.', weapon.Magazine - run.Ammo) + "]"
                : weapon.DisplayName + " " + run.Ammo + "/" + weapon.Magazine;
            WeaponCVO pocket = _model.Weapon(run.Holster);
            if (pocket != null) cylinder += "   Q: " + pocket.DisplayName;
            string status = $"{stance}    |    {cylinder}  +{run.Reserve:00}    |    {run.Elapsed:0.0}s";
            if (run.InHideout) status = $"HIDEOUT    |    CASH ${run.Cash}    |    {cylinder}";
            string objective = run.InHideout ? run.Hint : run.Won ? $"CONTRACT COMPLETE  /  {run.Elapsed:0.0}s  /  {run.Shots} shots  /  {run.SilentKills} silent takedowns  /  heading home" :
                !run.Alive ? run.Hint : $"{Enemies(run) - _model.Remaining}/{Enemies(run)} ENEMIES DOWN    —    {run.Hint}";
            _signals.Outgoing.HudChanged.Dispatch(status, objective);
            _model.ClearEdges();
        }
        private int Enemies(ContractRVO run)
        {
            int count = 0;
            foreach (GuardRVO guard in run.Guards) if (guard.Faction == Faction.Enemy) count++;
            return count;
        }
    }
}
