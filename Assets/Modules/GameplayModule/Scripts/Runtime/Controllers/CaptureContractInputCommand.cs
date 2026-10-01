using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    internal class CaptureContractInputCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [SignalParam] private ContractInputVO _input { get; set; }
        public override void Execute()
        {
            // At a terminal the keyboard types into the screen, not into the run.
            if (!_model.Run.Active || _model.Run.Terminal >= 0 || _model.Run.Menu >= 0) return;
            // A failed contract is tried again; a finished one goes home on its own, behind the loading screen.
            if (_input.Reload && !_model.Run.Alive)
            {
                _signals.Incoming.StartContract.Dispatch();
                return;
            }
            if (_input.Pause) _model.Run.Paused = !_model.Run.Paused;
            _model.Capture(_input);
        }
    }
}
