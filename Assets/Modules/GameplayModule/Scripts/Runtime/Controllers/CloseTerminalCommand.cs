using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    // Stepping away from a terminal lets the run carry on from where it stood.
    internal class CloseTerminalCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            _model.Run.Terminal = -1;
            _model.Run.Paused = false;
            _model.ClearEdges();
        }
    }
}
