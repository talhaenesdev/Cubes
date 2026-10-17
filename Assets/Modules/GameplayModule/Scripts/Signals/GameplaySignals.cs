using FlowIoC.BaseModule.Signals;

namespace Modules.GameplayModule.Signals
{
    public class GameplaySignals : ISignalHolder
    {
        public GameplaySignalsIncoming Incoming = new();
        public GameplaySignalsOutgoing Outgoing = new();
    }

    public class GameplaySignalsIncoming
    {
        public Signal StartContract = new();
        // The main menu's Play: back to the paused run, or into the saved one.
        public Signal Play = new();
        public Signal LeaveContract = new();
        public Signal<string> SaveTerminal = new();
        public Signal CloseTerminal = new();
        public Signal<int> ChooseMenu = new();
        // A button on the city map's shop card: the shop's index in CD_Missions, and MapActionCommand's action.
        public Signal<int, int> MapAction = new();
        public Signal CloseMenu = new();
        // A tape deck button: 0 play/pause, 1 next, 2 previous.
        public Signal<int> ControlTape = new();
        public Signal CloseDeck = new();
        // The deck's volume slider, 0 to 1.
        public Signal<float> SetMusicVolume = new();
    }

    public class GameplaySignalsOutgoing
    {
        public Signal OpenHud = new();
        public Signal<string, string> HudChanged = new(hideCommandLog: true);
        // Where the player is: HOME, or the contract's name.
        public Signal<string> LocationChanged = new(hideCommandLog: true);
        public Signal ReturnedToMenu = new();
        public Signal<string, string> TerminalOpened = new();
        public Signal TerminalClosed = new();
        public Signal<string, string, string[], bool> MenuOpened = new();
        public Signal MenuClosed = new();
        public Signal<string, string> MusicStarted = new();
        // The deck panel opens or refreshes: tape title, artist, whether it is playing.
        public Signal<string, string, bool> DeckShown = new();
        public Signal DeckClosed = new();
        // The music volume the deck's slider should show, 0 to 1.
        public Signal<float> MusicVolumeShown = new();
        // The phone call at home: who is calling and the line on screen; then that it has ended.
        public Signal<string, string> CallShown = new();
        public Signal CallEnded = new();
        // The phone rings: who is calling, before the player answers.
        public Signal<string> CallRinging = new();
        // A big line across the middle of the screen: DOCUMENTS SECURED.
        public Signal<string> BannerShown = new();
    }
}
