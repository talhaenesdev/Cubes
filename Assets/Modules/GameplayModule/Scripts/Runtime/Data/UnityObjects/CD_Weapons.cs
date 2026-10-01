using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // Every weapon in the game. Add an entry, give it a prefab, and name its Id on a character type or a level marker.
    [CreateAssetMenu(fileName = "CD_Weapons", menuName = "Night Shift/CD_Weapons")]
    public class CD_Weapons : ScriptableObject
    {
        [Tooltip("The weapon the player starts every run with.")]
        public string PlayerStartWeapon = "revolver";
        public List<WeaponCVO> Weapons = new();

        public WeaponCVO Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (WeaponCVO weapon in Weapons)
                if (weapon.Id == id) return weapon;
            return null;
        }

        // The shipped arsenal; also what a run uses when no CD_Weapons is filed on the Root.
        public void FillDefaults()
        {
            PlayerStartWeapon = "revolver";
            Weapons = new List<WeaponCVO>
            {
                new() {Id = "revolver", DisplayName = "REVOLVER", Magazine = 6, Reserve = 12, Reload = ReloadMode.PerRound, ReloadTime = 0.32f,
                    FireInterval = 0.2f, BulletSpeed = 42f, NoiseRadius = 11f},
                new() {Id = "pistol", DisplayName = "PISTOL", Magazine = 12, Reserve = 24, Reload = ReloadMode.Magazine, ReloadTime = 1.1f,
                    FireInterval = 0.15f, Spread = 2f, BulletSpeed = 40f, NoiseRadius = 9f},
                new() {Id = "silenced_pistol", DisplayName = "SILENCED PISTOL", Magazine = 8, Reserve = 16, Reload = ReloadMode.Magazine, ReloadTime = 1.3f,
                    FireInterval = 0.22f, Spread = 1f, BulletSpeed = 36f, NoiseRadius = 2.5f},
                new() {Id = "shotgun", DisplayName = "SHOTGUN", Magazine = 2, Reserve = 8, Reload = ReloadMode.PerRound, ReloadTime = 0.45f,
                    FireInterval = 0.5f, Pellets = 6, Spread = 14f, BulletSpeed = 34f, BulletLife = 0.4f, NoiseRadius = 14f},
                new() {Id = "smg", DisplayName = "SMG", Magazine = 24, Reserve = 48, Reload = ReloadMode.Magazine, ReloadTime = 1.6f,
                    Automatic = true, FireInterval = 0.08f, Spread = 5f, BulletSpeed = 44f, NoiseRadius = 12f},
                new() {Id = "rifle", DisplayName = "RIFLE", Magazine = 5, Reserve = 10, Reload = ReloadMode.Magazine, ReloadTime = 1.5f,
                    FireInterval = 0.6f, Damage = 3, BulletSpeed = 70f, BulletLife = 0.8f, NoiseRadius = 16f},
            };
        }
    }
}
