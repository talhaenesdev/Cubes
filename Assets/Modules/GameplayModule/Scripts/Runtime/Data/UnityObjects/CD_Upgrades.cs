using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // Everything the hideout's upgrade bench sells.
    [CreateAssetMenu(fileName = "CD_Upgrades", menuName = "Night Shift/CD_Upgrades")]
    public class CD_Upgrades : ScriptableObject
    {
        public List<UpgradeCVO> Upgrades = new();

        public void FillDefaults()
        {
            Upgrades = new List<UpgradeCVO>
            {
                new() {Id = "armor", Title = "PADDED COAT", Description = "Survive one more hit.", Kind = UpgradeKind.Health, Amount = 1f, MaxLevel = 2, Cost = 400},
                new() {Id = "quick_hands", Title = "QUICK HANDS", Description = "Reload 15% faster.", Kind = UpgradeKind.ReloadSpeed, Amount = 15f, MaxLevel = 3, Cost = 200},
                new() {Id = "light_step", Title = "LIGHT STEP", Description = "Move 10% faster.", Kind = UpgradeKind.MoveSpeed, Amount = 10f, MaxLevel = 3, Cost = 250},
                new() {Id = "bandolier", Title = "BANDOLIER", Description = "Start with 6 more spare rounds.", Kind = UpgradeKind.Ammo, Amount = 6f, MaxLevel = 3, Cost = 150},
            };
        }
    }
}
