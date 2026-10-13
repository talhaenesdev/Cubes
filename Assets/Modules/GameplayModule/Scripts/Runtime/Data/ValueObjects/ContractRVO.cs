using System;
using System.Collections.Generic;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    [Serializable]
    public class ContractRVO
    {
        public float FloorHeight = 4.2f;
        public float HalfWidth = 14f;
        public float ExitX = -13.3f;
        public Vector2 Player = new(-13f, 0f);
        public Vector2 Aim = new(-8f, 1f);
        public float VerticalSpeed;
        public int Floor;
        public bool Crouching;
        public bool Hidden;
        public bool InCover;
        public bool InVent;
        public int Vent = -1;
        public bool Alive = true;
        public bool Won;
        // Walked back out of the door they came in by, without the job done: home, nothing paid.
        public bool Abandoned;
        public bool Paused;
        public bool Active;
        // The player's character type and weapon, by Id in CD_Characters and CD_Weapons.
        public string PlayerType = "square";
        public string Outfit;
        public string Weapon = "revolver";
        public int Ammo = 6;
        public int Reserve = 12;
        public int Health = 1;
        // The second weapon, carried in the pocket; Q swaps it with the one in hand. Null when the pocket is empty.
        public string Holster;
        public int HolsterAmmo;
        public int HolsterReserve;
        public float ReloadRemaining;
        public float ShotCooldown;
        public float MeleeCooldown;
        // The last swing hit someone: the view shows the blow landing rather than a whiff.
        public bool MeleeLanded;
        public float NoiseRemaining;
        public float NoiseRadius;
        public Vector2 NoisePosition;
        public float Elapsed;
        public int Shots;
        public int SilentKills;
        public float FlashRemaining;
        public Vector2 ShotFrom;
        public Vector2 ShotTo;
        public string Hint = "";
        public GuardRVO[] Guards = Array.Empty<GuardRVO>();
        public DoorRVO[] Doors = Array.Empty<DoorRVO>();
        public LightRVO[] Lights = Array.Empty<LightRVO>();
        public VentRVO[] Vents = Array.Empty<VentRVO>();
        public List<BulletRVO> Bullets = new();
        public List<ImpactRVO> Impacts = new();
        public int ImpactCount;
        public int GuardShots;
        public Vector2 GuardShotFrom;
        public Vector2 GuardShotDirection;
        // Waist-high crates: solid up to CoverHeight, so a crouched body behind one is shielded from the far side only.
        public const float CoverHeight = 0.95f;
        public Vector2[] Covers = {new(-10f, 0f), new(0f, 0f), new(10f, 0f), new(-6f, 4.2f), new(6f, 4.2f)};
        public StairRVO[] Stairs = Array.Empty<StairRVO>();
        // Weapons lying on the floor: placed by the level, or dropped by whoever carried them.
        public List<PickupRVO> Pickups = new();
        // Cash the dead dropped, lying on the floor; and what the player has picked up this contract. It is only
        // banked when they get out - dying or retrying loses it.
        public List<LootRVO> Loot = new();
        public int LootCount;
        public int CashCarried;
        public int PickupCount;
        public TerminalRVO[] Terminals = Array.Empty<TerminalRVO>();
        // The terminal the player is using, or -1. While one is open the run stands still.
        public int Terminal = -1;
        // The start of a contract: the player takes out a phone and taps it, the track's card slides in, the music
        // starts a beat later, the phone goes away. The run is paused meanwhile.
        // The intro: headphones on, then the phone out, a tap, and the music.
        public const float IntroLength = 3.6f;
        public const float IntroHeadset = 1.0f;
        public const float IntroTap = 1.9f;
        public const float IntroMusic = 2.5f;
        public bool IntroPlaying;
        // The hideout's tape deck is running; the view spins its reels.
        public bool TapePlaying;
        // The tape deck's panel is open on the HUD; the run stands still.
        public bool DeckOpen;
        public float IntroRemaining;
        public bool IntroTapped;
        // Changing the song on the job: the weapon goes away, a hand to the headphones, a tap, the weapon back.
        // Seconds left of it, 0 when not; whether the tap has happened.
        public const float TrackSwitchLength = 1.1f;
        public const float TrackSwitchTap = 0.55f;
        public float TrackSwitch;
        public bool TrackSwitchTapped;
        public PropRVO[] Props = Array.Empty<PropRVO>();
        // Spots a bullet just hit, which nearby guards come to look at.
        public List<DisturbanceRVO> Disturbances = new();
        // The hideout: no enemies, no exit, a mission board and an upgrade bench.
        public bool InHideout;
        public StationRVO[] Stations = Array.Empty<StationRVO>();
        // The station whose list is open on the HUD, or -1. While one is open the run stands still.
        public int Menu = -1;
        // What bought upgrades change, applied when the run starts.
        public float ReloadScale = 1f;
        public float MoveScale = 1f;
        // CD_Gameplay.CameraZoom, carried to the view with the rest of the run.
        public float CameraZoom = 1f;
        public bool HasDocument;
        public Vector2 Document;
        public bool DocumentTaken;
        public bool FirstMissionBossContract;
        public int BossDialogue;
        public bool BossDialogueOpen;
        public bool BossRecruited;
        // The phone call at home: the line being shown, -1 when no call; the time spent on it.
        public int Call = -1;
        // The phone rings until the player answers it.
        public bool CallRinging;
        public float CallTimer;
        public int Cash;
    }

    [Serializable]
    public class GuardRVO
    {
        public Vector2 Position;
        public float Left;
        public float Right;
        public int Facing = 1;
        public bool Alive = true;
        public float Suspicion;
        public float FireCooldown = 0.5f;
        public float StunRemaining;
        public bool Alerted;
        // Its kind by Id in CD_Characters, and the numbers copied from it when the run began.
        public string Type;
        public Faction Faction;
        public string Weapon;
        public int Health = 1;
        // The second weapon, carried in the pocket; Q swaps it with the one in hand. Null when the pocket is empty.
        public string Holster;
        public int HolsterAmmo;
        public int HolsterReserve;
        public float WalkSpeed = 1.05f;
        public float AlertSpeed = 1.9f;
        public float SightRange = 6.5f;
        public float ReactionTime = 0.45f;
        public bool Dropped;
        // Difficulty, applied when the run begins: shorter waits between shots, faster bullets.
        public float FireRateScale = 1f;
        public float BulletSpeedScale = 1f;
        // Senses copied from the character type.
        public float SuspicionRate = 1.6f;
        public float SuspicionDecay = 0.6f;
        public float CrouchSight = 0.6f;
        public float DarkSight = 2.2f;
        public float CloseSense = 1.3f;
        public float Hearing = 1f;
        public float ChaseMemory = 3f;
        public float InvestigateTime = 1.5f;
        public float Curiosity = 6f;
        public float FirePace = 3f;
        public float DropChance = 1f;
        public bool Investigating;
        public Vector2 Target;
        public float Chase;
        public float Wait;
        // Tactics, copied from the type; and where the guard is in them.
        public bool UsesCover = true;
        public float CoverSearch = 7f;
        public float PeekTime = 1.1f;
        public float HideTime = 1.3f;
        public bool RaisesAlarm;
        public float AlarmRadius = 3.5f;
        // Crouched behind its crate: a smaller target that cannot shoot.
        public bool Crouching;
        public bool HasCover;
        // Where it stands behind its crate.
        public float CoverSpot;
        public float PeekTimer;
        // Where it last saw the player; what it tells the others.
        public Vector2 LastSeen;
        // Running to warn this guard, by index; -1 when not. Done once nobody is left to warn.
        public int Warning = -1;
        public bool AlarmDone;
        public Vector2Int CashDrop;
        // Non-combatants use the same lightweight character runtime as guards, but never count as enemies.
        public bool Civilian;
        public bool Boss;
        public bool Fleeing;
        public bool Escaped;
        public float WorkTimer;
    }

    [Serializable]
    public class DoorRVO
    {
        public Vector2 Position;
        public bool Open;
    }

    // One room on one floor, lit or dark; its switch is where a guard walks to put the light back on.
    [Serializable]
    public class LightRVO
    {
        public float Floor;
        public float Left;
        public float Right;
        public float Switch;
        public bool On = true;
    }

    // A computer the player can read and write on.
    [Serializable]
    public class TerminalRVO
    {
        public Vector2 Position;
        public string Title;
        public string Text;
    }

    [Serializable]
    public class StationRVO
    {
        public Vector2 Position;
        public StationKind Kind;
    }

    // Where a bullet just hit, and for how much longer guards in range take an interest.
    [Serializable]
    public class DisturbanceRVO
    {
        public Vector2 Position;
        public float Life;
    }

    // A breakable thing. Hit is its box's centre above Position, Size its box; Lift moves a falling lamp down.
    [Serializable]
    public class PropRVO
    {
        public Vector2 Position;
        public PropKind Kind;
        public Vector2 Hit;
        public Vector2 Size;
        public bool Broken;
        public bool Resolved;
        public bool Falling;
        public float Lift;
        public float FallSpeed;
    }

    // A stairwell door. Going through it comes out of the door it links to, on the other floor.
    [Serializable]
    public class StairRVO
    {
        public Vector2 Position;
        public int Link;
    }

    // A vent hides the player completely and connects to one other vent.
    [Serializable]
    public class VentRVO
    {
        public Vector2 Position;
        public int Link;
    }

    // A bullet in flight; it is stepped every frame and stops at the first wall, door or body in its path.
    [Serializable]
    public class BulletRVO
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public bool FromPlayer;
        public int Damage = 1;
        public float Life = 1f;
    }

    // A weapon on the floor with the rounds left in it. Walking over it with E takes it.
    [Serializable]
    public class PickupRVO
    {
        public int Id;
        public Vector2 Position;
        public string Weapon;
        public int Ammo;
        public int Reserve;
    }

    // Where a bullet stopped; kept as a bullet hole or a blood mark for the rest of the run.
    [Serializable]
    public class ImpactRVO
    {
        public int Id;
        public Vector2 Position;
        public Vector2 Normal;
        public bool Blood;
        public ImpactKind Kind;
    }

    [Serializable]
    public class LootRVO
    {
        public int Id;
        public Vector2 Position;
        public int Amount;
    }
}
