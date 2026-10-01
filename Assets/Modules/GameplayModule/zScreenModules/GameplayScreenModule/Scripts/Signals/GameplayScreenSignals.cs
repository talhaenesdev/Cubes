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
        public Signal<string, string> ShowTerminal = new();
        public Signal HideTerminal = new();
        public Signal<string, string, string[]> ShowMenu = new();
        public Signal HideMenu = new();
    }

    public class GameplayScreenSignalsOutgoing
    {
        public Signal RetryRequested = new();
        public Signal MenuRequested = new();
        public Signal<string> TerminalSaved = new();
        public Signal TerminalCloseRequested = new();
        public Signal<int> MenuChosen = new();
        public Signal MenuCloseRequested = new();
    }
}
