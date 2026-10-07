using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // Walking over dropped cash picks it up. It is carried, not banked, until the player gets out.
    internal class CollectLootCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (!run.Alive || run.Won || run.Paused || run.InVent) return;
            float reach = _model.Tuning.LootReach;
            for (int i = run.Loot.Count - 1; i >= 0; i--)
            {
                LootRVO loot = run.Loot[i];
                if (Mathf.Abs(loot.Position.y - run.Player.y) > 0.5f || Mathf.Abs(loot.Position.x - run.Player.x) > reach) continue;
                run.CashCarried += loot.Amount;
                run.Loot.RemoveAt(i);
            }
        }
    }
}
