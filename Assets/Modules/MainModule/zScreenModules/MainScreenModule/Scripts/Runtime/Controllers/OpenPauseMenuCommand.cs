using System;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using FlowIoC.ScreenModule.Service;
using Modules.LocalizationModule.Services;
using Modules.MainModule.MainScreenModule.ViewsMediators;

namespace Modules.MainModule.MainScreenModule.Controllers
{
    internal class OpenPauseMenuCommand : Command
    {
        [Inject] private IScreenService _screenService { get; set; }
        [Inject] private ILocalizationService _localization { get; set; }

        /// <summary>
        /// Three ways out, and every one of them resolves the retain. The success path is the one
        /// everybody writes; the screen coming back null and the await throwing are the two that
        /// hang the group for ever if they are forgotten, because a retain nobody resolves has no
        /// timeout and logs nothing.
        ///
        /// What Stop() does here is this game's answer and not the framework's - another game
        /// releases and carries on, or dispatches a signal that opens something else instead.
        /// </summary>
        public override async void Execute()
        {
            Retain();

            FlowLogger.Log("Execute - OpenPauseMenuCommand");

            try
            {
                // Show<T>() rather than Show(): a typed view compares against null through Unity's
                // own operator, and an IScreenBody does not.
                MainScreenView screen = await _screenService.Open<MainScreenView>().Show<MainScreenView>();

                if (screen == null)
                {
                    FlowLogger.LogError("OpenPauseMenuCommand - the screen did not open.");
                    Stop();
                    return;
                }

                screen.ShowAsPause(true);
                screen.Localize(_localization.Get);
                Release();
            }
            catch (Exception exception)
            {
                FlowLogger.LogError($"OpenPauseMenuCommand threw while opening the screen: {exception}");
                Stop();
            }
        }
    }
}
