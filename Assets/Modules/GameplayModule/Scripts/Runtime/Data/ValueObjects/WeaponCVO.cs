using System;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One weapon in CD_Weapons. Characters carry it, it drops when they die, and the player picks it up.
    [Serializable]
    public class WeaponCVO
    {
        [Tooltip("The key characters and level markers name this weapon by. Keep it unique.")]
        public string Id = "new_weapon";
        public string DisplayName = "NEW WEAPON";
        [Tooltip("The prefab held in the hand and lying on the floor when dropped. Needs a child named Muzzle.")]
        public GameObject Prefab;

        [Header("Ammunition")]
        [Min(1)] public int Magazine = 6;
        [Tooltip("Spare rounds the player starts with, and that a dropped copy carries.")]
        [Min(0)] public int Reserve = 12;
        public ReloadMode Reload = ReloadMode.PerRound;
        [Tooltip("Seconds per round (PerRound) or for the whole magazine (Magazine).")]
        [Min(0.05f)] public float ReloadTime = 0.32f;

        [Header("Firing")]
        [Tooltip("Hold the trigger to keep firing.")]
        public bool Automatic;
        [Min(0.02f)] public float FireInterval = 0.2f;
        [Tooltip("Bullets per shot; a shotgun fires several.")]
        [Min(1)] public int Pellets = 1;
        [Tooltip("Random spread per bullet, in degrees either side.")]
        [Range(0f, 45f)] public float Spread;
        [Tooltip("Hits a bullet deals; most characters take 1 to 3.")]
        [Min(1)] public int Damage = 1;
        [Min(1f)] public float BulletSpeed = 42f;
        [Tooltip("Seconds a bullet flies before it drops; short for a shotgun.")]
        [Min(0.05f)] public float BulletLife = 1f;
        [Tooltip("How far guards hear the shot. A suppressed weapon is quiet.")]
        [Min(0f)] public float NoiseRadius = 11f;
    }
}
