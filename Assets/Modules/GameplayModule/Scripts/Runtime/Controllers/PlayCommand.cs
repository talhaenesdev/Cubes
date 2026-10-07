using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // Play on the main menu: a run paused behind the menu carries on where it was; otherwise the game opens
    // where the save left it - the contract in progress, or home.
    internal class PlayCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }
        public override void Execute()
        {
            if (_model.MenuOpen && _model.Run.Active)
            {
                _internalSignals.Resume.Dispatch();
                return;
            }
            string saved = _model.Progress.CurrentMission;
            MissionCVO mission = _model.Missions != null ? _model.Missions.Find(saved) : null;
            _model.Mission = mission?.Id;
            _model.RestoreNextRun = mission != null;
            _signals.Incoming.StartContract.Dispatch();
        }
    }
}
