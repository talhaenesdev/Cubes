using FlowIoC.BaseModule.Signals;

namespace Modules.MainModule.MainScreenModule.Signals
{
    public class MainScreenSignals : ISignalHolder
    {
        public MainScreenSignalsIncoming Incoming = new();
        public MainScreenSignalsOutgoing Outgoing = new();
    }

    public class MainScreenSignalsIncoming
    {
        public Signal OpenMainScreen = new();
        // Opened over a paused game: a panel with Continue rather than the title screen.
        public Signal OpenPauseMenu = new();
    }

    public class MainScreenSignalsOutgoing
    {
        public Signal PlayClicked = new();
    }
}