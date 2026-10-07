using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ScreenModule.Service;
using Modules.AudioSettingsModule.Services;
using Modules.MainModule.MainScreenModule.ViewsMediators;

namespace Modules.MainModule.MainScreenModule.Controllers
{
    // The settings icon: the panel opens on the volumes the player saved.
    internal class OpenSettingsCommand : Command
    {
        [Inject] private IAudioSettingsService _audio { get; set; }
        [Inject] private IScreenService _screens { get; set; }

        public override void Execute()
        {
            if (_screens.TryGet.Screen<MainScreenView>(out MainScreenView screen)) screen.ShowSettings(_audio.Master, _audio.Music);
        }
    }
}
