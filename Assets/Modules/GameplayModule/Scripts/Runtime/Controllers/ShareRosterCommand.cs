using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // The view draws each character and weapon with the prefab its config entry names.
    internal class ShareRosterCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public override void Execute()
        {
            _signals.RosterShared.Dispatch(_model.Characters, _model.Weapons);
            _signals.OutfitsShared.Dispatch(_model.Outfits);
        }
    }
}
