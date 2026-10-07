using FlowIoC.BaseModule.Connectors;
using FlowIoC.BaseModule.Contexts;
using Modules.GameplayModule.Signals;
using Modules.GameplayModule.GameplayScreenModule.Signals;

namespace Modules.ConnectorModule.RootsContexts
{
    public class GameplayConnectorSubContext : Context
    {
        private const string GROUP = nameof(GameplayConnectorSubContext);
        private GameplaySignals _gameplaySignals;
        private GameplayScreenSignals _screenSignals;

        public override void Setup()
        {
            base.Setup();
            _gameplaySignals = InjectionBinderCrossContext.GetInstance<GameplaySignals>();
            _screenSignals = InjectionBinderCrossContext.GetInstance<GameplayScreenSignals>();
            IncomingSignals();
            OutgoingSignals();
        }

        private void IncomingSignals()
        {
            _screenSignals.Outgoing.RetryRequested.Connect(_gameplaySignals.Incoming.StartContract, GROUP);
            _screenSignals.Outgoing.MenuRequested.Connect(_gameplaySignals.Incoming.LeaveContract, GROUP);
            _screenSignals.Outgoing.TerminalSaved.Connect(_gameplaySignals.Incoming.SaveTerminal, GROUP);
            _screenSignals.Outgoing.TerminalCloseRequested.Connect(_gameplaySignals.Incoming.CloseTerminal, GROUP);
            _screenSignals.Outgoing.MenuChosen.Connect(_gameplaySignals.Incoming.ChooseMenu, GROUP);
            _screenSignals.Outgoing.MenuCloseRequested.Connect(_gameplaySignals.Incoming.CloseMenu, GROUP);
            _screenSignals.Outgoing.DeckButton.Connect(_gameplaySignals.Incoming.ControlTape, GROUP);
            _screenSignals.Outgoing.DeckCloseRequested.Connect(_gameplaySignals.Incoming.CloseDeck, GROUP);
            _screenSignals.Outgoing.DeckVolumeChanged.Connect(_gameplaySignals.Incoming.SetMusicVolume, GROUP);
        }

        private void OutgoingSignals()
        {
            _gameplaySignals.Outgoing.OpenHud.Connect(_screenSignals.Incoming.OpenGameplayScreen, GROUP);
            _gameplaySignals.Outgoing.HudChanged.Connect(_screenSignals.Incoming.RefreshHud, GROUP);
            _gameplaySignals.Outgoing.LocationChanged.Connect(_screenSignals.Incoming.ShowLocation, GROUP);
            _gameplaySignals.Outgoing.TerminalOpened.Connect(_screenSignals.Incoming.ShowTerminal, GROUP);
            _gameplaySignals.Outgoing.TerminalClosed.Connect(_screenSignals.Incoming.HideTerminal, GROUP);
            _gameplaySignals.Outgoing.MenuOpened.Connect(_screenSignals.Incoming.ShowMenu, GROUP);
            _gameplaySignals.Outgoing.MenuClosed.Connect(_screenSignals.Incoming.HideMenu, GROUP);
            _gameplaySignals.Outgoing.MusicStarted.Connect(_screenSignals.Incoming.ShowNowPlaying, GROUP);
            _gameplaySignals.Outgoing.DeckShown.Connect(_screenSignals.Incoming.ShowDeck, GROUP);
            _gameplaySignals.Outgoing.DeckClosed.Connect(_screenSignals.Incoming.HideDeck, GROUP);
            _gameplaySignals.Outgoing.MusicVolumeShown.Connect(_screenSignals.Incoming.ShowMusicVolume, GROUP);
            _gameplaySignals.Outgoing.CallShown.Connect(_screenSignals.Incoming.ShowCall, GROUP);
            _gameplaySignals.Outgoing.CallEnded.Connect(_screenSignals.Incoming.HideCall, GROUP);
            _gameplaySignals.Outgoing.CallRinging.Connect(_screenSignals.Incoming.ShowCallRinging, GROUP);
            _gameplaySignals.Outgoing.BannerShown.Connect(_screenSignals.Incoming.ShowBanner, GROUP);
        }

        public override void DestroyContext()
        {
            SignalConnector.DisconnectGroup(GROUP);
            base.DestroyContext();
        }
    }
}
