using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    // The first time home in the game, the phone rings: the broker has a job to explain, once the player picks up.
    internal class RingPhoneCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (!run.InHideout || _model.CallHeard || _model.Missions == null || _model.Missions.PhoneCall.Count == 0) return;
            run.Call = 0;
            run.CallRinging = true;
            run.CallTimer = 0f;
            run.Paused = true;
        }
    }
}
