using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // On a fresh launch into a saved contract, the run picks up as it was saved: no intro, the contract's music
    // straight on.
    internal class RestoreRunCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public override void Execute()
        {
            bool restore = _model.RestoreNextRun;
            _model.RestoreNextRun = false;
            if (!restore || !_model.RestoreRun()) return;
            MusicTrackCVO track = _model.Music != null ? _model.Music.Find(_model.QueuedTrack) : null;
            if (track == null || track.Clip == null) return;
            _model.PlayingTrack = track.Id;
            _signals.MusicRequested.Dispatch(track.Clip, track.Volume);
        }
    }
}
