using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    internal class SaveTerminalCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [SignalParam] private string _text { get; set; }
        public override void Execute() => _model.WriteTerminal(_model.Run.Terminal, _text);
    }
}
