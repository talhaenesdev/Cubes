using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.ViewsMediators.Mediator;
using FlowIoC.ScreenModule.Enums;
using FlowIoC.ScreenModule.Extensions;
using FlowIoC.ScreenModule.ViewsMediators.Screen;
using Modules.MainModule.MainScreenModule.Signals;

namespace Modules.MainModule.MainScreenModule.ViewsMediators
{
    public class MainScreenMediator : IMediator
    {
        [Inject] private MainScreenView _view { get; set; }
        [InjectSignal] private MainScreenSignals _signals { get; set; }
        [InjectSignal] private MainScreenInternalSignals _internalSignals { get; set; }

        public virtual void OnRegister()
        {
            _view.ShowCompleted += OnScreenShown;
            _view.HideCompleted += OnScreenHidden;
        }

        public virtual void OnRemove()
        {
            _view.ShowCompleted -= OnScreenShown;
            _view.HideCompleted -= OnScreenHidden;
            _view.PlayClicked -= OnPlayClicked;
            _view.LanguageClicked -= OnLanguageClicked;
            Unwire();
        }

        private void Unwire()
        {
            _view.SettingsClicked -= OnSettingsClicked;
            _view.SettingsCloseClicked -= _view.HideSettings;
            _view.MasterVolumeChanged -= OnMasterVolume;
            _view.MusicVolumeChanged -= OnMusicVolume;
        }

        private void OnScreenShown(IScreenBody screen)
        {
            _view.PlayClicked -= OnPlayClicked;
            _view.PlayClicked += OnPlayClicked;
            _view.LanguageClicked -= OnLanguageClicked;
            _view.LanguageClicked += OnLanguageClicked;
            Unwire();
            _view.SettingsClicked += OnSettingsClicked;
            _view.SettingsCloseClicked += _view.HideSettings;
            _view.MasterVolumeChanged += OnMasterVolume;
            _view.MusicVolumeChanged += OnMusicVolume;
        }

        private void OnScreenHidden(IScreenBody screen)
        {
            _view.PlayClicked -= OnPlayClicked;
            _view.LanguageClicked -= OnLanguageClicked;
            Unwire();
            _view.HideSettings();
        }

        private void OnSettingsClicked()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _internalSignals.OpenSettings.Dispatch();
        }

        private void OnMasterVolume(float volume)
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _internalSignals.MasterVolume.Dispatch(volume);
        }

        private void OnMusicVolume(float volume)
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _internalSignals.MusicVolume.Dispatch(volume);
        }

        private void OnLanguageClicked()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _internalSignals.NextLanguage.Dispatch();
        }

        private void OnPlayClicked()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal))
                return;

            _view.Hide();
            _signals.Outgoing.PlayClicked.Dispatch();
        }
    }
}
