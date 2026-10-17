using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // Every few seconds of a contract, the run is written down, so quitting never costs more than that.
    internal class SaveRunCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            if (_model.Run.InHideout || _model.Run.Paused) return;
            _model.RunSaveTimer += Time.deltaTime;
            if (_model.RunSaveTimer >= _model.Tuning.RunSaveInterval) _model.SaveRun();
        }
    }
}
