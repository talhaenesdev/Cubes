using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    // Whoever went down this frame lets go of their weapon, loaded, where they fell.
    internal class DropWeaponsCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            foreach (GuardRVO guard in _model.Run.Guards)
            {
                if (guard.Alive || guard.Dropped) continue;
                guard.Dropped = true;
                WeaponCVO weapon = _model.Weapon(guard.Weapon);
                if (weapon != null) _model.Drop(guard.Position, weapon.Id, weapon.Magazine, weapon.Magazine);
            }
        }
    }
}
