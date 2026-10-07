using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // T on the job: the player stops shooting, puts the weapon away, taps the headphones - and on the tap the
    // next contract track fades in with its card - then takes the weapon back.
    internal class SwitchTrackCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (run.InHideout || !run.Alive || run.Won || run.Paused || _model.MenuOpen) return;
            if (run.TrackSwitch <= 0f)
            {
                if (!_model.Input.NextTrack || run.InVent || run.IntroRemaining > 0f || run.ReloadRemaining > 0f) return;
                run.TrackSwitch = ContractRVO.TrackSwitchLength;
                run.TrackSwitchTapped = false;
                return;
            }
            run.TrackSwitch = Mathf.Max(0f, run.TrackSwitch - Mathf.Min(Time.deltaTime, 0.05f));
            if (run.TrackSwitchTapped || ContractRVO.TrackSwitchLength - run.TrackSwitch < ContractRVO.TrackSwitchTap) return;
            run.TrackSwitchTapped = true;
            MusicTrackCVO next = Next();
            if (next == null) return;
            _model.PlayingTrack = next.Id;
            _internalSignals.MusicRequested.Dispatch(next.Clip, next.Volume);
            _signals.Outgoing.MusicStarted.Dispatch(next.Title, next.Artist);
        }

        // The contract track after the one playing, round to the first.
        private MusicTrackCVO Next()
        {
            if (_model.Music == null) return null;
            var tracks = _model.Music.Tracks.FindAll(track => track.Category == MusicCategory.Contract && track.Clip != null);
            if (tracks.Count == 0) return null;
            int playing = tracks.FindIndex(track => track.Id == _model.PlayingTrack);
            return tracks[(playing + 1) % tracks.Count];
        }
    }
}
