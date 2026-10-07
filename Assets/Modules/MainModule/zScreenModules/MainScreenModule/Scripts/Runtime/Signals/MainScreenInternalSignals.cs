using FlowIoC.BaseModule.Signals;

namespace Modules.MainModule.MainScreenModule.Signals
{
    // What the main screen says to its own commands.
    internal class MainScreenInternalSignals : ISignalHolder
    {
        public Signal NextLanguage = new();
        public Signal OpenSettings = new();
        // Settings sliders, 0 to 1.
        public Signal<float> MasterVolume = new();
        public Signal<float> MusicVolume = new();
    }
}
