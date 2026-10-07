using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // The music fades out: leaving for the menu, or heading home behind the loading screen.
    internal class FadeMusicCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public override void Execute()
        {
            _model.PlayingTrack = null;
            _signals.MusicRequested.Dispatch(null, 0f);
        }
    }
}
