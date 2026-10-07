using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    internal class JudgeContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public override void Execute()
        {
            if (_model.Run.Won)
            {
                _model.CompleteMission();
                _model.Run.Hint = "CONTRACT COMPLETE  /  heading home";
            }
            else if (!_model.Run.Alive && string.IsNullOrEmpty(_model.Run.Hint)) _model.Run.Hint = "YOU WERE SEEN. ONE SHOT IS ALL IT TAKES.  /  R to retry";
            if (_model.Run.Won || !_model.Run.Alive || _model.Run.Abandoned) _model.IsTicking = false;
            // The tick stops here, so this is the only frame a win is judged in.
            if (_model.Run.Won && !_model.Run.InHideout) _signals.Extracted.Dispatch();
            else if (_model.Run.Abandoned && !_model.Run.InHideout) _signals.Abandoned.Dispatch();
        }
    }
}
