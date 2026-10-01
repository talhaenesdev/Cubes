using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    // The level prefab in the scene describes the layout; every run from now on starts from it.
    internal class StoreLevelCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [SignalParam] private ContractLevelVO _level { get; set; }
        public override void Execute() => _model.SetLevel(_level);
    }
}
