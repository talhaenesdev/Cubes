using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    internal class StartContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute() => _model.Reset();
    }
}
