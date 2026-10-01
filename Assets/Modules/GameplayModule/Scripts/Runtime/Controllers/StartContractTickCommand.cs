using System;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Update;
using FlowIoC.BaseModule.Signals;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    internal class StartContractTickCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private IUpdateProvider _updates { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public override void Execute()
        {
            _model.IsTicking = true;
            if (_model.IsHooked) return;
            _model.IsHooked = true;
            INightShiftModel model = _model;
            IUpdateProvider updates = _updates;
            Signal tick = _signals.Tick;
            Action onFrame = null;
            onFrame = () =>
            {
                if (!model.IsTicking)
                {
                    updates.RemoveUpdate(onFrame);
                    model.IsHooked = false;
                    return;
                }
                tick.Dispatch();
            };
            updates.AddUpdate(onFrame);
        }
    }
}
