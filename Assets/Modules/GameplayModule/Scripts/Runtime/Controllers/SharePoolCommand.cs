using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.PoolModule.Services;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // A Mediator can be injected with its View alone, so the pool the view spawns from is handed over by signal.
    internal class SharePoolCommand : Command
    {
        [Inject] private IPoolService _pool { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public override void Execute() => _signals.PoolShared.Dispatch(_pool);
    }
}
