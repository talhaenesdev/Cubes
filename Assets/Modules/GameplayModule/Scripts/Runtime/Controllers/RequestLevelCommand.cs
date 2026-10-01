using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // Asks the view for the level the next run is played in - the chosen contract's, or the hideout - so its
    // layout is stored before the run resets. Without CD_Missions the level already loaded stays.
    internal class RequestLevelCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public override void Execute()
        {
            LevelAuthoring level = _model.RequestedLevel;
            if (level != null) _signals.LevelRequested.Dispatch(level);
        }
    }
}
