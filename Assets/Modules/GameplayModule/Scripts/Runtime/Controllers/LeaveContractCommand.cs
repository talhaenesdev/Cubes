using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    internal class LeaveContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            _model.IsTicking = false;
            _model.Run.Active = false;
            _model.Mission = null;
            _model.ClearEdges();
        }
    }
}
