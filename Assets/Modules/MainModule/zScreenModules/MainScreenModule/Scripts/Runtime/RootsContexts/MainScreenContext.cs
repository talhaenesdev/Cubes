using FlowIoC.ScreenModule.Data;
using FlowIoC.ScreenModule.Enums;
using FlowIoC.ScreenModule.RootsContexts;
using Modules.MainModule.MainScreenModule.Controllers;
using Modules.MainModule.MainScreenModule.Signals;
using Modules.MainModule.MainScreenModule.ViewsMediators;

namespace Modules.MainModule.MainScreenModule.RootsContexts
{
    public class MainScreenContext : ScreenSubContext<MainScreenView, MainScreenMediator>
    {
        private MainScreenSignals _signals;
        private MainScreenInternalSignals _internalSignals;

        protected override ScreenCVO Screen => new()
        {
            ManagerId = 0,
            Layer = 0,
            Tag = ScreenTag.Default,
            Load = ScreenLoadCVO.Addressable("MainScreen"),
            HasShowAnimation = false,
            HasHideAnimation = false,
        };

        public override void SignalBindings()
        {
            base.SignalBindings();
            _signals = InjectionBinderCrossContext.Bind<MainScreenSignals>();
            _internalSignals = InjectionBinder.Bind<MainScreenInternalSignals>();
        }

        public override void CommandBindings()
        {
            base.CommandBindings();

            CommandBinder.Bind(_signals.Incoming.OpenMainScreen).ToSequence<OpenMainScreenCommand>();
            CommandBinder.Bind(_signals.Incoming.OpenPauseMenu).ToSequence<OpenPauseMenuCommand>();
            CommandBinder.Bind(_internalSignals.NextLanguage).ToSequence<NextLanguageCommand>();
            CommandBinder.Bind(_internalSignals.OpenSettings).ToSequence<OpenSettingsCommand>();
            CommandBinder.Bind(_internalSignals.MasterVolume).ToSequence<SetMasterVolumeCommand>();
            CommandBinder.Bind(_internalSignals.MusicVolume).ToSequence<SetMusicVolumeCommand>();
        }
    }
}