using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LocalizationModule.Services;
namespace Modules.GameplayModule.Controllers
{
    // A button on the tape deck: play or pause where the tape is, or wind on to the next or previous tape.
    // The panel is refreshed after every press.
    internal class ControlTapeCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }
        [SignalParam] private int _button { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            MusicTrackCVO loaded = _model.Music != null ? _model.Music.Find(_model.PlayingTrack) : null;
            bool inDeck = loaded != null && loaded.Category == MusicCategory.Hideout;
            var control = (TapeControl) _button;
            if (control == TapeControl.TogglePlay && inDeck)
            {
                run.TapePlaying = !run.TapePlaying;
                _model.TapePaused = !run.TapePlaying;
                _internalSignals.MusicPaused.Dispatch(!run.TapePlaying);
            }
            else
            {
                int step = control == TapeControl.Next ? 1 : control == TapeControl.Previous ? -1 : 0;
                MusicTrackCVO tape = _model.Tape(inDeck ? step : 0);
                if (tape != null)
                {
                    _model.PlayingTrack = tape.Id;
                    _model.TapePaused = false;
                    run.TapePlaying = true;
                    _internalSignals.MusicRequested.Dispatch(tape.Clip, tape.Volume);
                    _signals.Outgoing.MusicStarted.Dispatch(tape.Title, tape.Artist);
                }
            }
            Refresh(run);
        }
        private void Refresh(ContractRVO run)
        {
            MusicTrackCVO loaded = _model.Music != null ? _model.Music.Find(_model.PlayingTrack) : null;
            bool tape = loaded != null && loaded.Category == MusicCategory.Hideout;
            _signals.Outgoing.DeckShown.Dispatch(tape ? loaded.Title : _loc.Get("NO TAPE"), tape ? loaded.Artist : _loc.Get("Drop music into Music/Hideout"), run.TapePlaying);
        }
    }
}
