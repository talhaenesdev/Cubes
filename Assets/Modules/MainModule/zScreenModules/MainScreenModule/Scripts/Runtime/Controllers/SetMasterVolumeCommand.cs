using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.AudioSettingsModule.Services;

namespace Modules.MainModule.MainScreenModule.Controllers
{
    internal class SetMasterVolumeCommand : Command
    {
        [Inject] private IAudioSettingsService _audio { get; set; }
        [SignalParam] private float _volume { get; set; }

        public override void Execute() => _audio.SetMaster(_volume);
    }
}
