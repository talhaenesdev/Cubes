using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // Every gameplay number that is not about one weapon or one kind of character.
    [CreateAssetMenu(fileName = "CD_Gameplay", menuName = "Night Shift/CD_Gameplay")]
    public class CD_Gameplay : ScriptableObject
    {
        [Header("Player movement")]
        [Min(0.1f)] public float WalkSpeed = 4.7f;
        [Min(0.1f)] public float CrouchSpeed = 2.25f;
        [Min(0f)] public float JumpSpeed = 7f;
        [Min(1f)] public float Gravity = 22f;

        [Header("Melee")]
        [Min(0.1f)] public float MeleeRange = 1.6f;
        [Min(0.05f)] public float MeleeCooldown = 0.45f;
        [Tooltip("Seconds an aware guard is knocked back by a shove that does not kill.")]
        [Min(0f)] public float MeleeStun = 1.2f;
        [Tooltip("How far a landed blow shoves the guard.")]
        [Min(0f)] public float MeleeKnockback = 0.7f;

        [Header("Doors")]
        [Tooltip("How far a kicked door knocks down whoever is behind it.")]
        [Min(0f)] public float KickReach = 1.8f;
        [Min(0f)] public float KickStun = 2.5f;
        [Tooltip("How far guards hear a door being kicked.")]
        [Min(0f)] public float KickNoise = 4f;

        [Header("Gunfire")]
        [Tooltip("Seconds a gunshot keeps carrying, for guards within its noise radius.")]
        [Min(0.05f)] public float ShotNoiseTime = 0.65f;
        [Tooltip("Seconds the spot a bullet hit stays interesting to guards in their Curiosity range.")]
        [Min(0f)] public float ImpactCuriosityTime = 1.2f;
        [Tooltip("Suspicion a guard gains from a bullet hitting near it - never enough on its own to start shooting.")]
        [Range(0f, 0.95f)] public float ImpactSuspicion = 0.35f;

        [Header("Props")]
        [Min(0f)] public float BlastRadius = 2.4f;
        [Tooltip("A barrel this close to a blast goes off too.")]
        [Min(0f)] public float ChainRadius = 2.8f;
        [Tooltip("Half the width a falling lamp crushes.")]
        [Min(0f)] public float CrushHalfWidth = 0.7f;

        [Header("Money")]
        [Tooltip("Off: no cash anywhere - nothing drops, nothing is shown, and outfits and upgrades cost nothing.")]
        public bool UseMoney = true;
        [Tooltip("A finished contract also pays its CD_Missions Reward. Off: the only income is what the dead drop.")]
        public bool MissionsPay;
        [Tooltip("Outfits cost their CD_Outfits Price. Off: every outfit is free.")]
        public bool OutfitsCost;
        [Tooltip("How close the player has to walk to a dropped bundle to pick it up.")]
        [Min(0.1f)] public float LootReach = 0.8f;

        [Header("Damage")]
        [Tooltip("Hits a barrel's blast deals to everyone inside BlastRadius. Taken off Health like a bullet's.")]
        [Min(0)] public int BlastDamage = 3;
        [Tooltip("Hits a falling lamp deals to whoever stands under it.")]
        [Min(0)] public int CrushDamage = 2;
        [Tooltip("Hits a takedown deals - on an unaware, turned-away or knocked-down guard.")]
        [Min(0)] public int MeleeDamage = 10;

        [Header("Contract")]
        [Tooltip("How close to the exit the player has to be to leave.")]
        [Min(0.2f)] public float ExitReach = 1.3f;

        [Header("Camera")]
        [Tooltip("How close the camera sits. 1 is the level's authored framing; 2 shows everything twice as big. Above 1 the camera follows the player.")]
        [Range(0.5f, 3f)] public float CameraZoom = 1.6f;
        [Tooltip("The furthest the mouse wheel zooms out.")]
        [Range(0.5f, 3f)] public float ZoomMin = 0.8f;
        [Tooltip("The closest the mouse wheel zooms in.")]
        [Range(0.5f, 4f)] public float ZoomMax = 3f;
        [Tooltip("How much one notch of the mouse wheel zooms.")]
        [Range(0.02f, 1f)] public float ZoomStep = 0.15f;

        [Header("Interface and saving")]
        [Tooltip("Seconds between automatic contract saves.")]
        [Min(0.25f)] public float RunSaveInterval = 3f;
        [Tooltip("Seconds between live refreshes while the city map is open.")]
        [Min(0.1f)] public float MapRefreshInterval = 1f;
        [Tooltip("Prevents the same input that opened a call from immediately answering it.")]
        [Min(0f)] public float PhoneInputGrace = 0.4f;
    }
}
