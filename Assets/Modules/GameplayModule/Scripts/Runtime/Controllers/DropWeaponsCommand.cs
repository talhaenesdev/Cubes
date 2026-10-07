using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // Whoever went down this frame lets go of their weapon, loaded, where they fell - and of their cash.
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
                if (weapon != null && UnityEngine.Random.value <= guard.DropChance) _model.Drop(guard.Position, weapon.Id, weapon.Magazine, weapon.Magazine);
                if (!_model.Tuning.UseMoney || guard.CashDrop.y <= 0) continue;
                int amount = UnityEngine.Random.Range(Mathf.Max(0, guard.CashDrop.x), Mathf.Max(guard.CashDrop.x, guard.CashDrop.y) + 1);
                if (amount <= 0) continue;
                ContractRVO run = _model.Run;
                // A little way from the weapon, so the two do not sit on top of each other.
                run.Loot.Add(new LootRVO {Id = ++run.LootCount, Position = guard.Position + new Vector2(-guard.Facing * 0.45f, 0f), Amount = amount});
            }
        }
    }
}
