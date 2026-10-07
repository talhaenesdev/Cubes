using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ScreenModule.Service;
using Modules.LocalizationModule.Services;
using Modules.MainModule.MainScreenModule.ViewsMediators;

namespace Modules.MainModule.MainScreenModule.Controllers
{
    // The language button: the next language, and the main screen written again in it.
    internal class NextLanguageCommand : Command
    {
        [Inject] private ILocalizationService _localization { get; set; }
        [Inject] private IScreenService _screens { get; set; }

        public override void Execute()
        {
            _localization.NextLanguage();
            if (_screens.TryGet.Screen<MainScreenView>(out MainScreenView screen)) screen.Localize(_localization.Get);
        }
    }
}
