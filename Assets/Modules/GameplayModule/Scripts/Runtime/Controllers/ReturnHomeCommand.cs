using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Constants;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LoadingModule.Services;
namespace Modules.GameplayModule.Controllers
{
    // Forgets the finished contract and starts a run in the hideout, behind the loading screen.
    internal class ReturnHomeCommand : Command<string>
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILoadingService _loading { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        // The loading step this trip home reports, bound in the sequence: each set that goes home has its own.
        public override void Execute(string stepKey)
        {
            ILoadingStep step = _loading.Report(stepKey);
            step.Start();
            _model.Mission = null;
            _signals.Incoming.StartContract.Dispatch();
            step.Complete();
        }
    }
}
