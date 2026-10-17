using System;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    // UTC keeps the same clock while the game is closed: the shops' payments come in, and the game is marked as open.
    internal class PayWorkingLocationsCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }

        public override void Execute()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _model.Territory.Heartbeat(now);
            _model.Territory.CollectPayments(now);
        }
    }
}
