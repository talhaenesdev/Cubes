using System;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // The layout a run starts from. The level prefab's LevelAuthoring writes it from its markers;
    // the values below are the fallback when no level is in the scene, and match the shipped prefab.
    [Serializable]
    public class ContractLevelVO
    {
        public float FloorHeight = 4.2f;
        public float HalfWidth = 14f;
        public float ExitX = -13.3f;
        public Vector2 PlayerSpawn = new(-13f, 0f);
        // The file the contract is after; with one, the exit opens once it is taken rather than once the building is clear.
        public bool HasDocument;
        public Vector2 Document;
        public GuardSpawnVO[] Guards =
        {
            new() {Position = new Vector2(-5f, 0f), Left = -6.5f, Right = -4f, Facing = 1, Type = "triangle"},
            new() {Position = new Vector2(3f, 0f), Left = 1f, Right = 5f, Facing = 1, Type = "pentagon"},
            new() {Position = new Vector2(12f, 0f), Left = 9f, Right = 13f, Facing = -1, Type = "hexagon"},
            new() {Position = new Vector2(-11f, 4.2f), Left = -12.5f, Right = -9f, Facing = -1, Type = "triangle"},
            new() {Position = new Vector2(0f, 4.2f), Left = -2f, Right = 1.5f, Facing = 1, Type = "diamond"},
            new() {Position = new Vector2(8f, 4.2f), Left = 5f, Right = 10f, Facing = 1, Type = "pentagon"},
            new() {Position = new Vector2(-10.5f, 0f), Left = -12f, Right = -9f, Facing = 1, Type = "circle"},
        };
        public WeaponSpawnVO[] Weapons =
        {
            new() {Position = new Vector2(-11.5f, 0f), Weapon = "silenced_pistol"},
        };
        public Vector2[] Doors = {new(-3f, 0f), new(3f, 4.2f)};
        public Vector2[] Covers = {new(-10f, 0f), new(0f, 0f), new(10f, 0f), new(-6f, 4.2f), new(6f, 4.2f)};
        public StairRVO[] Stairs =
        {
            new() {Position = new Vector2(-8f, 0f), Link = 1}, new() {Position = new Vector2(-8f, 4.2f), Link = 0},
            new() {Position = new Vector2(11f, 0f), Link = 3}, new() {Position = new Vector2(11f, 4.2f), Link = 2},
        };
        public TerminalRVO[] Terminals =
        {
            new() {Position = new Vector2(-5.3f, 0f), Title = "DELIVERIES / LOG", Text = "Night courier due 02:40.\nDock door code changed - ask Marek."},
            new() {Position = new Vector2(7.3f, 0f), Title = "COLD STORAGE / 07", Text = "Unit 07 temperature alarm muted.\nDo not open before the buyer arrives."},
            new() {Position = new Vector2(-1.8f, 4.2f), Title = "STAFF ONLY / SECURITY", Text = "Patrol rota: two upstairs, three down.\nLights on the top floor keep tripping."},
        };
        public LightRVO[] Lights =
        {
            new() {Floor = 0f, Left = -14f, Right = -3f, Switch = -6.8f},
            new() {Floor = 0f, Left = -3f, Right = 14f, Switch = 5.5f},
            new() {Floor = 4.2f, Left = -14f, Right = 3f, Switch = -4f},
            new() {Floor = 4.2f, Left = 3f, Right = 14f, Switch = 9f},
        };
        public StationRVO[] Stations = Array.Empty<StationRVO>();
        public PropRVO[] Props = Array.Empty<PropRVO>();
        public LevelDifficulty Difficulty = LevelDifficulty.Normal;
        public VentRVO[] Vents =
        {
            new() {Position = new Vector2(-12f, 4.2f), Link = 1}, new() {Position = new Vector2(7.8f, 0f), Link = 0},
            new() {Position = new Vector2(-1.5f, 0f), Link = 3}, new() {Position = new Vector2(12.8f, 4.2f), Link = 2},
        };
    }

    [Serializable]
    public class GuardSpawnVO
    {
        public Vector2 Position;
        public float Left;
        public float Right;
        public int Facing = 1;
        [Tooltip("Character type Id in CD_Characters.")]
        public string Type = "triangle";
        [Tooltip("Weapon Id in CD_Weapons; empty uses the character type's own.")]
        public string Weapon = "";
        [Tooltip("Extra hits this one takes, on top of its type and the level's difficulty.")]
        public int Toughness;
        [Tooltip("1 is its type's pace. 2 reacts and fires twice as fast; 0.5 half as fast.")]
        public float Aggression = 1f;
        [Tooltip("A non-combatant: works until frightened, then hides or flees.")]
        public bool Civilian;
        [Tooltip("The civilian the player talks to after all enemies are gone.")]
        public bool Boss;
    }

    [Serializable]
    public class WeaponSpawnVO
    {
        public Vector2 Position;
        public string Weapon;
    }
}
