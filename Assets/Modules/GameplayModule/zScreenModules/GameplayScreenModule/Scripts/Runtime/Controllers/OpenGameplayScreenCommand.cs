using System;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using FlowIoC.ScreenModule.Service;
using Modules.LocalizationModule.Services;
using Modules.GameplayModule.GameplayScreenModule.ViewsMediators;

namespace Modules.GameplayModule.GameplayScreenModule.Controllers
{
    internal class OpenGameplayScreenCommand : Command
    {
        [Inject] private IScreenService _screenService { get; set; }
        [Inject] private ILocalizationService _localization { get; set; }

        /// <summary>
        /// Three ways out, and every one of them resolves the retain: the screen opened, the screen
        /// came back null, and the await threw. A retain nobody resolves hangs the group for ever,
        /// with no timeout and nothing logged.
        /// </summary>
        public override async void Execute()
        {
            Retain();

            FlowLogger.Log("Execute - OpenGameplayScreenCommand");

            try
            {
                if (_screenService.TryGet.Screen<GameplayScreenView>(out GameplayScreenView open))
                {
                    open.Localize(_localization.Get);
                    Release();
                    return;
                }
                GameplayScreenView screen = await _screenService.Open<GameplayScreenView>().Show<GameplayScreenView>();

                if (screen == null)
                {
                    FlowLogger.LogError("OpenGameplayScreenCommand - the screen did not open.");
                    Stop();
                    return;
                }

                screen.Localize(_localization.Get);
                Release();
            }
            catch (Exception exception)
            {
                FlowLogger.LogError($"OpenGameplayScreenCommand threw while opening the screen: {exception}");
                Stop();
            }
        }
    }
}
