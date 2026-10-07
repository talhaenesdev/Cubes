using System;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One weapon a character type may carry, and how likely next to the others in its pool.
    [Serializable]
    public class WeaponChanceCVO
    {
        [Tooltip("Weapon Id in CD_Weapons.")]
        public string Weapon = "pistol";
        [Min(0f)] public float Weight = 1f;
    }
}
