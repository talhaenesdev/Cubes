using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // The root of a level prefab. Move, add or remove the markers under it in the prefab and the next run
    // is played on what you built: doors, stairwell doors, crates, vents, light rooms, guards and the spawn.
    public class LevelAuthoring : MonoBehaviour
    {
        [Header("Building")]
        [Tooltip("Height of one floor; the second floor stands at this Y.")]
        public float FloorHeight = 4.2f;
        [Tooltip("Half the building's width; the outer walls stand at ±this X.")]
        public float HalfWidth = 14f;
        [Tooltip("How hard every enemy in this level is, on top of its character type and its own spawn settings.")]
        public Enums.LevelDifficulty Difficulty = Enums.LevelDifficulty.Normal;
        [Tooltip("For a level wider than the screen: the camera follows the player instead of framing it all.")]
        public bool FollowCamera;

        [Header("Scene")]
        public Camera Camera;
        [Tooltip("Pooled characters, bullets, effects and marks are parented here.")]
        public Transform Dynamic;
        public Transform Crosshair;

        public DoorView[] Doors { get; private set; } = new DoorView[0];
        public LightRoomView[] Rooms { get; private set; } = new LightRoomView[0];
        public PropView[] Props { get; private set; } = new PropView[0];

        public ContractLevelVO Collect()
        {
            var level = new ContractLevelVO {FloorHeight = FloorHeight, HalfWidth = HalfWidth, Difficulty = Difficulty};
            Props = GetComponentsInChildren<PropView>(true);
            level.Props = new PropRVO[Props.Length];
            for (int i = 0; i < Props.Length; i++)
                level.Props[i] = new PropRVO {Position = Snap(Props[i].transform.position), Kind = Props[i].Kind, Hit = Props[i].HitCenter, Size = Props[i].HitSize};
            Doors = GetComponentsInChildren<DoorView>(true);
            Rooms = GetComponentsInChildren<LightRoomView>(true);
            level.Doors = Positions(Doors);
            level.Covers = Positions(GetComponentsInChildren<CoverView>(true));
            StairDoorView[] stairs = GetComponentsInChildren<StairDoorView>(true);
            var stairIndex = new List<StairDoorView>(stairs);
            level.Stairs = new StairRVO[stairs.Length];
            for (int i = 0; i < stairs.Length; i++)
            {
                int link = stairs[i].Link != null ? stairIndex.IndexOf(stairs[i].Link) : i;
                level.Stairs[i] = new StairRVO {Position = Snap(stairs[i].transform.position), Link = link < 0 ? i : link};
            }
            level.Lights = new LightRVO[Rooms.Length];
            for (int i = 0; i < Rooms.Length; i++)
            {
                Vector3 p = Rooms[i].transform.position;
                level.Lights[i] = new LightRVO {Floor = FloorOf(p.y), Left = Rooms[i].Left, Right = Rooms[i].Right, Switch = p.x};
            }
            VentView[] vents = GetComponentsInChildren<VentView>(true);
            var ventIndex = new List<VentView>(vents);
            level.Vents = new VentRVO[vents.Length];
            for (int i = 0; i < vents.Length; i++)
            {
                int link = vents[i].Link != null ? ventIndex.IndexOf(vents[i].Link) : i;
                level.Vents[i] = new VentRVO {Position = Snap(vents[i].transform.position), Link = link < 0 ? i : link};
            }
            TerminalView[] terminals = GetComponentsInChildren<TerminalView>(true);
            level.Terminals = new TerminalRVO[terminals.Length];
            for (int i = 0; i < terminals.Length; i++)
                level.Terminals[i] = new TerminalRVO {Position = Snap(terminals[i].transform.position), Title = terminals[i].Title, Text = terminals[i].Text};
            StationView[] stations = GetComponentsInChildren<StationView>(true);
            level.Stations = new StationRVO[stations.Length];
            for (int i = 0; i < stations.Length; i++)
                level.Stations[i] = new StationRVO {Position = Snap(stations[i].transform.position), Kind = stations[i].Kind};
            GuardSpawn[] guards = GetComponentsInChildren<GuardSpawn>(true);
            level.Guards = new GuardSpawnVO[guards.Length];
            for (int i = 0; i < guards.Length; i++)
                level.Guards[i] = new GuardSpawnVO
                {
                    Position = Snap(guards[i].transform.position), Left = guards[i].PatrolLeft,
                    Right = guards[i].PatrolRight, Facing = guards[i].Facing >= 0 ? 1 : -1,
                    Type = guards[i].CharacterType, Weapon = guards[i].Weapon,
                    Toughness = guards[i].Toughness, Aggression = guards[i].Aggression,
                };
            WeaponSpawn[] weapons = GetComponentsInChildren<WeaponSpawn>(true);
            level.Weapons = new WeaponSpawnVO[weapons.Length];
            for (int i = 0; i < weapons.Length; i++)
                level.Weapons[i] = new WeaponSpawnVO {Position = Snap(weapons[i].transform.position), Weapon = weapons[i].Weapon};
            PlayerSpawn spawn = GetComponentInChildren<PlayerSpawn>(true);
            if (spawn != null) level.PlayerSpawn = Snap(spawn.transform.position);
            ExitView exit = GetComponentInChildren<ExitView>(true);
            if (exit != null) level.ExitX = exit.transform.position.x;
            return level;
        }

        private Vector2[] Positions(Component[] items)
        {
            var positions = new Vector2[items.Length];
            for (int i = 0; i < items.Length; i++) positions[i] = Snap(items[i].transform.position);
            return positions;
        }

        // Markers stand on a floor; whatever height they were dropped at, they are read as standing on the one below.
        private Vector2 Snap(Vector3 position) => new(position.x, FloorOf(position.y));

        private float FloorOf(float y) => Mathf.Clamp(Mathf.Floor((y + 0.5f) / FloorHeight), 0f, 1f) * FloorHeight;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.3f, 0.35f, 0.8f);
            for (int floor = 0; floor <= 2; floor++)
                Gizmos.DrawLine(transform.position + new Vector3(-HalfWidth, floor * FloorHeight),
                    transform.position + new Vector3(HalfWidth, floor * FloorHeight));
        }
    }
}
