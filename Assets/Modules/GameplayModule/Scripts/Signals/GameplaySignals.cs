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
        public Signal LeaveContract = new();
        public Signal<string> SaveTerminal = new();
        public Signal CloseTerminal = new();
        public Signal<int> ChooseMenu = new();
        public Signal CloseMenu = new();
    }

    public class GameplaySignalsOutgoing
    {
        public Signal OpenHud = new();
        public Signal<string, string> HudChanged = new(hideCommandLog: true);
        public Signal ReturnedToMenu = new();
        public Signal<string, string> TerminalOpened = new();
        public Signal TerminalClosed = new();
        public Signal<string, string, string[]> MenuOpened = new();
        public Signal MenuClosed = new();
    }
}
