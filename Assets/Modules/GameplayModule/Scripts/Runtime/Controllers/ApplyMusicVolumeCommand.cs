using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.AudioSettingsModule.Services;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // The saved music volume, handed to the music player and shown on the deck's slider.
    internal class ApplyMusicVolumeCommand : Command
    {
        [Inject] private IAudioSettingsService _audio { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }
        public override void Execute()
        {
            _internalSignals.MusicVolume.Dispatch(_audio.Music);
            _signals.Outgoing.MusicVolumeShown.Dispatch(_audio.Music);
        }
    }
}
