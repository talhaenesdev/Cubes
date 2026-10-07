using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    // Back from the main menu: the run is as it was left, paused only if it was paused before.
    internal class ResumeFromMenuCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            _model.MenuOpen = false;
            _model.Run.Paused = _model.PausedBeforeMenu;
        }
    }
}
