using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.ViewsMediators.Mediator;
using FlowIoC.ScreenModule.Enums;
using FlowIoC.ScreenModule.Extensions;
using FlowIoC.ScreenModule.ViewsMediators.Screen;
using Modules.GameplayModule.GameplayScreenModule.Signals;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    public class GameplayScreenMediator : IMediator
    {
        [Inject] private GameplayScreenView _view { get; set; }
        [InjectSignal] private GameplayScreenSignals _signals { get; set; }

        public void OnRegister()
        {
            _view.ShowCompleted += OnScreenShown;
            _view.HideCompleted += OnScreenHidden;
        }

        public void OnRemove()
        {
            OnScreenHidden(_view);
            _view.ShowCompleted -= OnScreenShown;
            _view.HideCompleted -= OnScreenHidden;
        }

        private void OnScreenShown(IScreenBody screen)
        {
            OnScreenHidden(screen);
            _view.RetryClicked += OnRetryClicked;
            _view.MenuClicked += OnMenuClicked;
            _signals.Incoming.RefreshHud.AddListener(OnRefreshHud);
            _signals.Incoming.ShowLocation.AddListener(_view.ShowLocation);
            _view.TerminalSaveClicked += OnTerminalSave;
            _view.TerminalCloseClicked += OnTerminalClose;
            _signals.Incoming.ShowTerminal.AddListener(_view.ShowTerminal);
            _signals.Incoming.HideTerminal.AddListener(_view.HideTerminal);
            _view.MenuEntryClicked += OnMenuEntry;
            _view.MenuCloseClicked += OnMenuClose;
            _signals.Incoming.ShowMenu.AddListener(_view.ShowMenu);
            _signals.Incoming.HideMenu.AddListener(_view.HideMenu);
            _signals.Incoming.ShowNowPlaying.AddListener(_view.ShowNowPlaying);
            _view.DeckButtonClicked += OnDeckButton;
            _view.DeckCloseClicked += OnDeckClose;
            _view.DeckVolumeChanged += OnDeckVolume;
            _signals.Incoming.ShowMusicVolume.AddListener(_view.ShowMusicVolume);
            _signals.Incoming.ShowCall.AddListener(_view.ShowCall);
            _signals.Incoming.HideCall.AddListener(_view.HideCall);
            _signals.Incoming.ShowCallRinging.AddListener(_view.ShowCallRinging);
            _signals.Incoming.ShowBanner.AddListener(_view.ShowBanner);
            _signals.Incoming.ShowDeck.AddListener(_view.ShowDeck);
            _signals.Incoming.HideDeck.AddListener(_view.HideDeck);
        }

        private void OnScreenHidden(IScreenBody screen)
        {
            _view.RetryClicked -= OnRetryClicked;
            _view.MenuClicked -= OnMenuClicked;
            _signals.Incoming.RefreshHud.RemoveListener(OnRefreshHud);
            _signals.Incoming.ShowLocation.RemoveListener(_view.ShowLocation);
            _view.TerminalSaveClicked -= OnTerminalSave;
            _view.TerminalCloseClicked -= OnTerminalClose;
            _signals.Incoming.ShowTerminal.RemoveListener(_view.ShowTerminal);
            _signals.Incoming.HideTerminal.RemoveListener(_view.HideTerminal);
            _view.MenuEntryClicked -= OnMenuEntry;
            _view.MenuCloseClicked -= OnMenuClose;
            _signals.Incoming.ShowMenu.RemoveListener(_view.ShowMenu);
            _signals.Incoming.HideMenu.RemoveListener(_view.HideMenu);
            _signals.Incoming.ShowNowPlaying.RemoveListener(_view.ShowNowPlaying);
            _view.DeckButtonClicked -= OnDeckButton;
            _view.DeckCloseClicked -= OnDeckClose;
            _view.DeckVolumeChanged -= OnDeckVolume;
            _signals.Incoming.ShowMusicVolume.RemoveListener(_view.ShowMusicVolume);
            _signals.Incoming.ShowCall.RemoveListener(_view.ShowCall);
            _signals.Incoming.HideCall.RemoveListener(_view.HideCall);
            _signals.Incoming.ShowCallRinging.RemoveListener(_view.ShowCallRinging);
            _signals.Incoming.ShowBanner.RemoveListener(_view.ShowBanner);
            _signals.Incoming.ShowDeck.RemoveListener(_view.ShowDeck);
            _signals.Incoming.HideDeck.RemoveListener(_view.HideDeck);
        }

        private void OnTerminalSave(string text)
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _signals.Outgoing.TerminalSaved.Dispatch(text);
            _signals.Outgoing.TerminalCloseRequested.Dispatch();
        }

        private void OnDeckButton(int button)
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _signals.Outgoing.DeckButton.Dispatch(button);
        }

        private void OnDeckVolume(float volume)
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _signals.Outgoing.DeckVolumeChanged.Dispatch(volume);
        }

        private void OnDeckClose()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _signals.Outgoing.DeckCloseRequested.Dispatch();
        }

        private void OnMenuEntry(int index)
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _signals.Outgoing.MenuChosen.Dispatch(index);
        }

        private void OnMenuClose()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _signals.Outgoing.MenuCloseRequested.Dispatch();
        }

        private void OnTerminalClose()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _signals.Outgoing.TerminalCloseRequested.Dispatch();
        }

        private void OnRefreshHud(string status, string objective)
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _view.ShowHud(status, objective);
        }

        private void OnRetryClicked()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _signals.Outgoing.RetryRequested.Dispatch();
        }

        private void OnMenuClicked()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal)) return;
            _view.Hide();
            _signals.Outgoing.MenuRequested.Dispatch();
        }
    }
}
