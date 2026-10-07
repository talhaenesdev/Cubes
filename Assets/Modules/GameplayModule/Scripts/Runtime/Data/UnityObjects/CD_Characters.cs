using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // Every kind of character. Set each one's Faction to decide who is a friend and who is an enemy.
    [CreateAssetMenu(fileName = "CD_Characters", menuName = "Night Shift/CD_Characters")]
    public class CD_Characters : ScriptableObject
    {
        [Tooltip("The character type the player plays as.")]
        public string PlayerType = "rectangle";
        public List<CharacterTypeCVO> Types = new();

        public CharacterTypeCVO Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (CharacterTypeCVO type in Types)
                if (type.Id == id) return type;
            return null;
        }

        // The shipped cast; also what a run uses when no CD_Characters is filed on the Root.
        public void FillDefaults()
        {
            PlayerType = "rectangle";
            Types = new List<CharacterTypeCVO>
            {
                new() {Id = "rectangle", DisplayName = "RECTANGLE", Shape = CharacterShape.Rectangle, Faction = Faction.Friendly, Weapon = "revolver",
                    Health = 1, WalkSpeed = 1.05f, AlertSpeed = 1.9f, SightRange = 6.5f, ReactionTime = 0.45f},
                new() {Id = "square", DisplayName = "SQUARE", Shape = CharacterShape.Square, Faction = Faction.Friendly, Weapon = "revolver",
                    Health = 1, WalkSpeed = 1.05f, AlertSpeed = 1.9f, SightRange = 6.5f, ReactionTime = 0.45f},
                new() {Id = "triangle", DisplayName = "TRIANGLE", RaisesAlarm = true, Shape = CharacterShape.Triangle, Faction = Faction.Enemy, Weapon = "pistol",
                    Health = 1, WalkSpeed = 1.3f, AlertSpeed = 2.4f, SightRange = 6f, ReactionTime = 0.4f},
                new() {Id = "pentagon", DisplayName = "PENTAGON", Shape = CharacterShape.Pentagon, Faction = Faction.Enemy, Weapon = "smg",
                    Health = 2, WalkSpeed = 1f, AlertSpeed = 1.8f, SightRange = 6.5f, ReactionTime = 0.5f},
                new() {Id = "hexagon", DisplayName = "HEXAGON", UsesCover = false, Shape = CharacterShape.Hexagon, Faction = Faction.Enemy, Weapon = "shotgun",
                    Health = 3, WalkSpeed = 0.8f, AlertSpeed = 1.4f, SightRange = 5.5f, ReactionTime = 0.6f},
                new() {Id = "diamond", DisplayName = "DIAMOND", Shape = CharacterShape.Diamond, Faction = Faction.Enemy, Weapon = "rifle",
                    Health = 1, WalkSpeed = 0.9f, AlertSpeed = 1.6f, SightRange = 9f, ReactionTime = 0.8f},
                new() {Id = "circle", DisplayName = "CIRCLE", Shape = CharacterShape.Circle, Faction = Faction.Friendly, Weapon = "",
                    Health = 1, WalkSpeed = 0.9f, AlertSpeed = 1.6f, SightRange = 6f, ReactionTime = 1f},
            };
        }
    }
}
