using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LocalizationModule.Services;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // The call: the phone rings until the player answers with a click, E or Space; then the lines come one by one -
    // on the same keys, or on their own - and the run picks up when the last one is done.
    internal class PhoneCallCommand : Command
    {
        // A press this soon after the ring starts is the one that opened the door, not an answer.
        private const float Grace = 0.4f;
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (run.Call < 0 || _model.MenuOpen) return;
            CD_Missions missions = _model.Missions;
            bool pressed = _model.Input.Interact || _model.Input.Jump || _model.Input.Fire;
            if (run.CallRinging)
            {
                if (run.CallTimer == 0f) _signals.Outgoing.CallRinging.Dispatch(_loc.Get(missions.PhoneCaller));
                run.CallTimer += Mathf.Min(Time.deltaTime, 0.05f);
                if (!pressed || run.CallTimer < Grace) return;
                run.CallRinging = false;
                run.CallTimer = 0f;
                _model.CallHeard = true;
                _signals.Outgoing.CallShown.Dispatch(_loc.Get(missions.PhoneCaller), _loc.Get(missions.PhoneCall[run.Call]));
                return;
            }
            run.CallTimer += Mathf.Min(Time.deltaTime, 0.05f);
            bool next = pressed || run.CallTimer >= missions.PhoneLineTime;
            if (next) run.Call++;
            if (run.Call >= missions.PhoneCall.Count)
            {
                run.Call = -1;
                run.Paused = false;
                _signals.Outgoing.CallEnded.Dispatch();
                return;
            }
            if (next)
            {
                run.CallTimer = 0f;
                _signals.Outgoing.CallShown.Dispatch(_loc.Get(missions.PhoneCaller), _loc.Get(missions.PhoneCall[run.Call]));
            }
        }
    }
}
