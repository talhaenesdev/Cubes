using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // The mouse wheel zooms the camera between CD_Gameplay's limits; the zoom chosen carries over to the next run.
    internal class ZoomCameraCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [SignalParam] private ContractInputVO _input { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (_input.Zoom == 0f || !run.Active || run.Terminal >= 0 || run.Menu >= 0 || run.DeckOpen) return;
            CD_Gameplay tuning = _model.Tuning;
            float low = Mathf.Min(tuning.ZoomMin, tuning.ZoomMax);
            float high = Mathf.Max(tuning.ZoomMin, tuning.ZoomMax);
            run.CameraZoom = Mathf.Clamp(run.CameraZoom + Mathf.Sign(_input.Zoom) * tuning.ZoomStep, low, high);
            _model.PlayerZoom = run.CameraZoom;
        }
    }
}
