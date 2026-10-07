using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // A contract opens on the player starting its music from the phone; a retry keeps what is playing.
    // The hideout starts silent - the music there is whatever the player puts on the tape deck.
    internal class QueueMusicCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            MusicTrackCVO playing = _model.Music != null ? _model.Music.Find(_model.PlayingTrack) : null;
            if (run.InHideout)
            {
                // Coming home from a job, the job's music fades; a tape keeps playing.
                if (playing != null && playing.Category == Enums.MusicCategory.Hideout) run.TapePlaying = !_model.TapePaused;
                else if (playing != null)
                {
                    _model.PlayingTrack = null;
                    _signals.MusicRequested.Dispatch(null, 0f);
                }
                return;
            }
            if (playing != null && playing.Category == Enums.MusicCategory.Contract) return;
            MusicTrackCVO track = _model.TrackFor(_model.Mission);
            if (track == null || track.Clip == null) return;
            _model.QueuedTrack = track.Id;
            _model.PlayingTrack = null;
            _signals.MusicRequested.Dispatch(null, 0f);
            run.IntroRemaining = ContractRVO.IntroLength;
            run.IntroTapped = false;
            run.IntroPlaying = false;
            run.Paused = true;
        }
    }
}
