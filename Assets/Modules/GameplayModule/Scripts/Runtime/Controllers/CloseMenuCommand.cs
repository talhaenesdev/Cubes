using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    internal class CloseMenuCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            _model.Run.Menu = -1;
            _model.GuardAssignmentMission = null;
            _model.Run.Paused = false;
            _model.ClearEdges();
        }
    }
}
