using System;
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
        [Tooltip("Weapon Id from CD_Weapons, used when the spawn marker names none. Empty for unarmed.")]
        public string Weapon = "pistol";

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
    }
}
