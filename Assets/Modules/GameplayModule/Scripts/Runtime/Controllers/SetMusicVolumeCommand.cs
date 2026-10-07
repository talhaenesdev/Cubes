using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.AudioSettingsModule.Services;
namespace Modules.GameplayModule.Controllers
{
    // The deck's slider moved: the new music volume is saved.
    internal class SetMusicVolumeCommand : Command
    {
        [Inject] private IAudioSettingsService _audio { get; set; }
        [SignalParam] private float _volume { get; set; }
        public override void Execute() => _audio.SetMusic(_volume);
    }
}
