using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    internal class StartContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        public override void Execute()
        {
            _model.Reset();
            // A retry creates a fresh run, so close any dialogue card left by the previous run.
            _signals.Outgoing.CallEnded.Dispatch();
        }
    }
}
