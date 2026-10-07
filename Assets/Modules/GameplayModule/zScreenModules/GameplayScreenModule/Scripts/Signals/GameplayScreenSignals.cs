using FlowIoC.BaseModule.Signals;

namespace Modules.GameplayModule.GameplayScreenModule.Signals
{
    public class GameplayScreenSignals : ISignalHolder
    {
        public GameplayScreenSignalsIncoming Incoming = new();
        public GameplayScreenSignalsOutgoing Outgoing = new();
    }

    public class GameplayScreenSignalsIncoming
    {
        public Signal OpenGameplayScreen = new();
        public Signal<string, string> RefreshHud = new();
        public Signal<string> ShowLocation = new();
        public Signal<string, string> ShowTerminal = new();
        public Signal HideTerminal = new();
        public Signal<string, string, string[]> ShowMenu = new();
        public Signal HideMenu = new();
        public Signal<string, string> ShowNowPlaying = new();
        public Signal<string, string, bool> ShowDeck = new();
        public Signal HideDeck = new();
        public Signal<float> ShowMusicVolume = new();
        public Signal<string, string> ShowCall = new();
        public Signal HideCall = new();
        public Signal<string> ShowCallRinging = new();
        public Signal<string> ShowBanner = new();
    }

    public class GameplayScreenSignalsOutgoing
    {
        public Signal RetryRequested = new();
        public Signal MenuRequested = new();
        public Signal<string> TerminalSaved = new();
        public Signal TerminalCloseRequested = new();
        public Signal<int> MenuChosen = new();
        public Signal MenuCloseRequested = new();
        // 0 play/pause, 1 next, 2 previous.
        public Signal<int> DeckButton = new();
        public Signal DeckCloseRequested = new();
        // The deck's volume slider moved, 0 to 1.
        public Signal<float> DeckVolumeChanged = new();
    }
}
