using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LocalizationModule.Services;
namespace Modules.GameplayModule.Controllers
{
    internal class JudgeContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        [InjectSignal] private GameplaySignals _publicSignals { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        public override void Execute()
        {
            if (_model.Run.IsLocationDefense)
            {
                JudgeLocationDefense();
                return;
            }
            if (_model.Run.Won)
            {
                _model.CompleteMission();
                _model.Run.Hint = _loc.Get("CONTRACT COMPLETE  /  heading home");
                if (_model.Territory.AllOurs) _publicSignals.Outgoing.BannerShown.Dispatch(_loc.Get("THE WHOLE CITY WORKS FOR US"));
            }
            else if (!_model.Run.Alive && string.IsNullOrEmpty(_model.Run.Hint)) _model.Run.Hint = _loc.Get("YOU WERE SEEN. ONE SHOT IS ALL IT TAKES.  /  R to retry");
            if (_model.Run.Won || !_model.Run.Alive || _model.Run.Abandoned) _model.IsTicking = false;
            // The tick stops here, so this is the only frame a win is judged in.
            if (_model.Run.Won && !_model.Run.InHideout) _signals.Extracted.Dispatch();
            else if (_model.Run.Abandoned && !_model.Run.InHideout) _signals.Abandoned.Dispatch();
        }

        private void JudgeLocationDefense()
        {
            ContractRVO run = _model.Run;
            // Every raider down wins it; there is no door to walk out of first.
            if (!run.Won && run.Alive && !run.Abandoned && _model.Remaining == 0) run.Won = true;
            if (run.Won)
            {
                int fallen = 0;
                foreach (GuardRVO guard in run.Guards)
                    if (guard.Faction == Faction.Friendly && !guard.Civilian && !guard.Alive) fallen++;
                _model.Territory.DefenseWon(fallen);
                run.Hint = fallen > 0 ? _loc.Format("LOCATION DEFENDED  /  {0} of our men fell", fallen) : _loc.Get("LOCATION DEFENDED  /  heading home");
                _publicSignals.Outgoing.BannerShown.Dispatch(_loc.Get("LOCATION DEFENDED"));
                _model.IsTicking = false;
                _signals.Extracted.Dispatch();
                return;
            }
            if (run.Alive && !run.Abandoned) return;
            _model.Territory.DefenseLost();
            run.Hint = _loc.Get("LOCATION LOST  /  heading home");
            _model.IsTicking = false;
            _signals.Abandoned.Dispatch();
        }
    }
}
