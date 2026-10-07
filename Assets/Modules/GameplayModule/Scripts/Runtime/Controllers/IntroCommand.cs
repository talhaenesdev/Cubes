using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // The phone moment: on the tap the track's card slides in, a beat later the music starts, and the run
    // starts when the phone is away.
    internal class IntroCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (run.IntroRemaining <= 0f || _model.MenuOpen) return;
            run.IntroRemaining = Mathf.Max(0f, run.IntroRemaining - Mathf.Min(Time.deltaTime, 0.05f));
            float elapsed = ContractRVO.IntroLength - run.IntroRemaining;
            MusicTrackCVO track = _model.Music != null ? _model.Music.Find(_model.QueuedTrack) : null;
            if (!run.IntroTapped && elapsed >= ContractRVO.IntroTap)
            {
                run.IntroTapped = true;
                if (track != null) _signals.Outgoing.MusicStarted.Dispatch(track.Title, track.Artist);
            }
            if (!run.IntroPlaying && elapsed >= ContractRVO.IntroMusic)
            {
                run.IntroPlaying = true;
                if (track != null && track.Clip != null)
                {
                    _model.PlayingTrack = track.Id;
                    _internalSignals.MusicRequested.Dispatch(track.Clip, track.Volume);
                }
            }
            if (run.IntroRemaining <= 0f) run.Paused = false;
        }
    }
}
