using System;
using System.Collections.Generic;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One kind of character in CD_Characters. Level spawn markers name it by Id.
    [Serializable]
    public class CharacterTypeCVO
    {
        [Tooltip("The key spawn markers name this character by. Keep it unique.")]
        public string Id = "new_character";
        public string DisplayName = "NEW CHARACTER";
        public CharacterShape Shape = CharacterShape.Square;
        [Tooltip("Enemies hunt and shoot the player and must all fall before the exit opens. Friendlies only patrol.")]
        public Faction Faction = Faction.Enemy;
        [Tooltip("The prefab drawn for this character; it needs a CharacterRig.")]
        public GameObject Prefab;
        [Tooltip("Weapon Id from CD_Weapons, used when the spawn marker names none and the pool below is empty. Empty for unarmed.")]
        public string Weapon = "pistol";
        [Tooltip("Each one of this type carries a weapon drawn from here by weight. Empty: always Weapon above.")]
        public List<WeaponChanceCVO> WeaponPool = new();
        [Tooltip("Chance its weapon is left on the floor when it goes down.")]
        [Range(0f, 1f)] public float DropChance = 1f;

        [Header("Body")]
        [Tooltip("Hits it takes to go down. Melee from behind always kills.")]
        [Min(1)] public int Health = 1;
        [Min(0.1f)] public float WalkSpeed = 1.05f;
        [Min(0.1f)] public float AlertSpeed = 1.9f;

        [Header("Senses")]
        [Tooltip("How far it sees a standing player in a lit room.")]
        [Min(1f)] public float SightRange = 6.5f;
        [Tooltip("Seconds between fully spotting the player and the first shot.")]
        [Min(0f)] public float ReactionTime = 0.45f;
        [Tooltip("How fast suspicion fills while it sees the player: 1 means a full meter in one second.")]
        [Min(0.05f)] public float SuspicionRate = 1.6f;
        [Tooltip("How fast suspicion drains once the player is out of sight.")]
        [Min(0f)] public float SuspicionDecay = 0.6f;
        [Tooltip("Sight range against a crouching player, as a share of SightRange.")]
        [Range(0.1f, 1f)] public float CrouchSight = 0.6f;
        [Tooltip("How far it sees into a dark room.")]
        [Min(0f)] public float DarkSight = 2.2f;
        [Tooltip("This close it notices the player even facing away.")]
        [Min(0f)] public float CloseSense = 1.3f;
        [Tooltip("Scales how far it hears gunshots, kicks and breaking things. 2 hears twice as far.")]
        [Min(0f)] public float Hearing = 1f;
        [Tooltip("Seconds it keeps hunting the player after losing sight of them.")]
        [Min(0f)] public float ChaseMemory = 3f;
        [Tooltip("Seconds it stands looking around a spot it came to investigate.")]
        [Min(0f)] public float InvestigateTime = 1.5f;
        [Tooltip("How far away a bullet hitting a wall or a crate makes it come and look.")]
        [Min(0f)] public float Curiosity = 6f;

        [Header("Shooting")]
        [Tooltip("Its bullets fly at this share of the weapon's BulletSpeed; below 1 lets the player dodge them.")]
        [Range(0.2f, 2f)] public float BulletSpeed = 0.6f;
        [Tooltip("Seconds between its shots, as a multiple of the weapon's FireInterval.")]
        [Min(0.2f)] public float FirePace = 3f;

        [Header("Loot")]
        [Tooltip("Cash it drops when it dies, picked between these two. 0 / 0 drops nothing.")]
        public Vector2Int CashDrop = new(10, 25);

        [Header("Tactics")]
        [Tooltip("Once it has seen the player it runs to the nearest crate between them, crouches behind it and pops up to shoot, instead of standing in the open.")]
        public bool UsesCover = true;
        [Tooltip("How far it will run to reach a crate.")]
        [Min(0f)] public float CoverSearch = 7f;
        [Tooltip("Seconds it stands up out of cover to shoot, and seconds it stays down between.")]
        [Min(0.2f)] public float PeekTime = 1.1f;
        [Min(0.2f)] public float HideTime = 1.3f;
        [Tooltip("Having seen the player, it first runs to the others who have not and tells them - they then know where the player is and come for them. It fights once nobody is left to warn.")]
        public bool RaisesAlarm;
        [Tooltip("Those within this distance of the one it warns are told too.")]
        [Min(0f)] public float AlarmRadius = 3.5f;
    }
}
