using System;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One upgrade on the hideout's bench. Each level costs Cost times the level being bought.
    [Serializable]
    public class UpgradeCVO
    {
        public string Id = "new_upgrade";
        public string Title = "NEW UPGRADE";
        public string Description = "";
        public UpgradeKind Kind = UpgradeKind.Health;
        [Tooltip("What one level adds: hits for Health, percent for ReloadSpeed and MoveSpeed, rounds for Ammo.")]
        public float Amount = 1f;
        [Min(1)] public int MaxLevel = 3;
        [Min(0)] public int Cost = 200;
    }
}
