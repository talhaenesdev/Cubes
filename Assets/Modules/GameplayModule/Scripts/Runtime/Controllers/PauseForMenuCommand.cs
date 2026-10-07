using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    // The HUD's MENU: the run holds still behind the main menu instead of ending.
    internal class PauseForMenuCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            if (_model.MenuOpen) return;
            _model.MenuOpen = true;
            _model.PausedBeforeMenu = _model.Run.Paused;
            _model.Run.Paused = true;
            _model.SaveProgress();
            _model.SaveRun();
        }
    }
}
