#if UNITY_EDITOR
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    public partial class NightShiftAssetBuilder
    {
        private const string Door = Prefabs + "/Fixtures/PB_Door.prefab";
        private const string Cover = Prefabs + "/Fixtures/PB_Crate.prefab";
        private const string Vent = Prefabs + "/Fixtures/PB_Vent.prefab";
        private const string LightSwitch = Prefabs + "/Fixtures/PB_LightSwitch.prefab";
        private const string StairDoor = Prefabs + "/Fixtures/PB_StairDoor.prefab";
        private const string Computer = Prefabs + "/Fixtures/PB_Computer.prefab";
        // Written by earlier builds, before stairwell doors replaced ladders.
        private const string RetiredLadder = Prefabs + "/Fixtures/PB_Ladder.prefab";
        private const string Exit = Prefabs + "/Fixtures/PB_Exit.prefab";

        // ---------------------------------------------------------------- fixtures

        private void BuildFixtures()
        {
            if (ShouldWrite(Door) || DoorNeedsShapeGrid())
            {
                GameObject door = Node("PB_Door", null, Vector2.zero);
                var view = door.AddComponent<DoorView>();
                Part("Frame", door.transform, new Vector2(0f, 2.57f), new Vector2(0.6f, 0.14f), _steel, 20);
                GameObject closed = Node("Closed", door.transform, Vector2.zero);
                Part("Slab", closed.transform, new Vector2(0f, 1.25f), new Vector2(0.4f, 2.5f), _steel, 21);
                Part("Trim", closed.transform, new Vector2(-0.12f, 1.25f), new Vector2(0.045f, 2.35f), _amber, 22);
                DoorShapeGrid(closed.transform, CharacterShape.Square);
                Part("Knob", closed.transform, new Vector2(0.08f, 1.15f), new Vector2(0.12f, 0.08f), _ink, 23);
                GameObject open = Node("Open", door.transform, Vector2.zero);
                Part("Leaf", open.transform, new Vector2(0f, 1.25f), new Vector2(0.085f, 2.5f), _teal, 21);
                Part("Doorway", open.transform, new Vector2(0.38f, 1.25f), new Vector2(0.67f, 2.5f), new Color(0.12f, 0.20f, 0.22f, 0.42f), -7);
                open.SetActive(false);
                Assign(view, "_closed", closed);
                Assign(view, "_open", open);
                SavePrefab<GameObject>(door, Door);
            }
            if (ShouldWrite(Cover))
            {
                GameObject crate = Node("PB_Crate", null, Vector2.zero);
                crate.AddComponent<CoverView>();
                float h = ContractRVO.CoverHeight;
                Part("Outline", crate.transform, new Vector2(0f, h * 0.5f), new Vector2(1.7f, h), _ink, 15);
                Part("Body", crate.transform, new Vector2(0f, h * 0.5f + 0.005f), new Vector2(1.55f, h - 0.11f), new Color(0.18f, 0.24f, 0.25f), 16);
                Part("Lid", crate.transform, new Vector2(0f, h - 0.04f), new Vector2(1.73f, 0.09f), _steel, 17);
                for (int i = 0; i < 4; i++)
                    Part("Slat " + i, crate.transform, new Vector2(-0.55f + i * 0.37f, h * 0.48f), new Vector2(0.045f, h * 0.74f), _ink, 17);
                Part("Label", crate.transform, new Vector2(0.38f, h * 0.72f), new Vector2(0.3f, 0.16f), _amber * new Color(0.7f, 0.7f, 0.7f, 1f), 18);
                SavePrefab<GameObject>(crate, Cover);
            }
            if (ShouldWrite(Vent))
            {
                GameObject vent = Node("PB_Vent", null, Vector2.zero);
                vent.AddComponent<VentView>();
                Part("Frame", vent.transform, new Vector2(0f, 0.32f), new Vector2(0.95f, 0.6f), _ink, -6);
                Part("Duct", vent.transform, new Vector2(0f, 0.32f), new Vector2(0.82f, 0.48f), new Color(0.13f, 0.18f, 0.2f), -5);
                for (int i = 0; i < 4; i++) Part("Louvre " + i, vent.transform, new Vector2(0f, 0.14f + i * 0.12f), new Vector2(0.74f, 0.035f), _ink, -4);
                SavePrefab<GameObject>(vent, Vent);
            }
            if (ShouldWrite(LightSwitch))
            {
                GameObject room = Node("PB_LightSwitch", null, Vector2.zero);
                var view = room.AddComponent<LightRoomView>();
                Part("Plate", room.transform, new Vector2(0f, 1.35f), new Vector2(0.2f, 0.32f), _ink, -6);
                SpriteRenderer on = Part("Toggle On", room.transform, new Vector2(0f, 1.42f), new Vector2(0.08f, 0.12f), _amber, -5);
                SpriteRenderer off = Part("Toggle Off", room.transform, new Vector2(0f, 1.28f), new Vector2(0.08f, 0.12f), _steel, -5);
                off.gameObject.SetActive(false);
                SpriteRenderer dark = Part("Darkness", room.transform, new Vector2(0f, 1.95f), new Vector2(10f, 3.8f), new Color(0f, 0.005f, 0.02f, 0.72f), 13);
                dark.gameObject.SetActive(false);
                Assign(view, "_switchOn", on.gameObject);
                Assign(view, "_switchOff", off.gameObject);
                Assign(view, "_darkness", dark);
                SavePrefab<GameObject>(room, LightSwitch);
            }
            if (ShouldWrite(StairDoor))
            {
                GameObject stair = Node("PB_StairDoor", null, Vector2.zero);
                stair.AddComponent<StairDoorView>();
                Color frame = new(0.35f, 0.3f, 0.24f);
                Part("Frame", stair.transform, new Vector2(0f, 1.3f), new Vector2(1.1f, 2.6f), _ink, -22);
                Part("Door", stair.transform, new Vector2(0f, 1.25f), new Vector2(0.9f, 2.45f), new Color(0.16f, 0.14f, 0.12f), -21);
                Part("Panel", stair.transform, new Vector2(0f, 1.6f), new Vector2(0.6f, 0.8f), frame, -20);
                Part("Handle", stair.transform, new Vector2(0.3f, 1.15f), new Vector2(0.1f, 0.06f), _amber, -19);
                Part("Lintel", stair.transform, new Vector2(0f, 2.62f), new Vector2(1.2f, 0.1f), _steel, -19);
                Sign("STAIRS", stair.transform, new Vector2(0f, 2.9f), 0.15f, _amber);
                SavePrefab<GameObject>(stair, StairDoor);
            }
            if (ShouldWrite(Computer))
            {
                GameObject computer = Node("PB_Computer", null, Vector2.zero);
                computer.AddComponent<TerminalView>();
                Part("Top", computer.transform, new Vector2(0f, 0.9f), new Vector2(1.85f, 0.16f), _steel, -20);
                Part("Leg L", computer.transform, new Vector2(-0.75f, 0.42f), new Vector2(0.1f, 0.84f), _steel, -20);
                Part("Leg R", computer.transform, new Vector2(0.75f, 0.42f), new Vector2(0.1f, 0.84f), _steel, -20);
                Part("Monitor", computer.transform, new Vector2(-0.2f, 1.28f), new Vector2(0.65f, 0.56f), _ink, -19);
                Part("Screen", computer.transform, new Vector2(-0.2f, 1.3f), new Vector2(0.49f, 0.35f), new Color(0.16f, 0.5f, 0.42f), -18);
                for (int i = 0; i < 3; i++)
                    Part("Line " + i, computer.transform, new Vector2(-0.28f + i * 0.04f, 1.39f - i * 0.08f), new Vector2(0.28f - i * 0.06f, 0.025f), new Color(0.6f, 1f, 0.8f, 0.8f), -17);
                Part("Keyboard", computer.transform, new Vector2(0.15f, 1.0f), new Vector2(0.5f, 0.05f), _ink, -18);
                Part("Mug", computer.transform, new Vector2(0.58f, 1.06f), new Vector2(0.16f, 0.22f), _amber, -18);
                SavePrefab<GameObject>(computer, Computer);
            }
            if (ShouldWrite(Exit))
            {
                GameObject exit = Node("PB_Exit", null, Vector2.zero);
                exit.AddComponent<ExitView>();
                Color green = new(0.40f, 1f, 0.64f);
                Part("Frame", exit.transform, new Vector2(0f, 1.23f), new Vector2(1f, 2.46f), _ink, -22);
                Part("Door", exit.transform, new Vector2(0f, 1.23f), new Vector2(0.79f, 2.3f), new Color(0.08f, 0.24f, 0.19f), -21);
                Part("Glow", exit.transform, new Vector2(-0.42f, 1.23f), new Vector2(0.055f, 2.46f), green, -20);
                Sign("EXIT", exit.transform, new Vector2(0f, 2.75f), 0.22f, green);
                SavePrefab<GameObject>(exit, Exit);
            }
        }

        private bool DoorNeedsShapeGrid()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Door);
            return prefab != null && prefab.transform.Find("Closed/Shape Grid") == null;
        }

        private void DoorShapeGrid(Transform parent, CharacterShape shape)
        {
            const int gridSize = 5;
            const float cellSize = 0.035f;
            const float cellStep = 0.046f;
            Vector2 centre = new(0f, 1.72f);
            Transform grid = Node("Shape Grid", parent, centre).transform;
            Part("Screen", grid, Vector2.zero, new Vector2(0.29f, 0.29f), _ink, 23);

            float half = (gridSize - 1) * 0.5f;
            for (int row = 0; row < gridSize; row++)
            {
                for (int column = 0; column < gridSize; column++)
                {
                    Vector2 cell = new((column - half) * cellStep, (row - half) * cellStep);
                    Part($"Grid {column} {row}", grid, cell, new Vector2(cellSize, cellSize), new Color(0.16f, 0.22f, 0.22f), 24);
                    float x = (column - half) / 1.7f;
                    float y = (row - half) / 1.7f;
                    if (Inside(shape, x, y))
                        Part($"Shape {column} {row}", grid, cell, new Vector2(cellSize, cellSize), _teal, 25);
                }
            }
        }

        // ---------------------------------------------------------------- level

        private void BuildLevel()
        {
            if (!ShouldWrite(LevelPrefab) && !NeedsUpgrade(true)) return;
            var layout = new ContractLevelVO();
            GameObject root = Node("PB_Level_NightShift", null, Vector2.zero);
            var level = root.AddComponent<LevelAuthoring>();
            level.FloorHeight = layout.FloorHeight;
            level.HalfWidth = layout.HalfWidth;

            Transform art = Node("Art", root.transform, Vector2.zero).transform;
            Backdrop(Node("Backdrop", art, Vector2.zero).transform);
            Building(Node("Building", art, Vector2.zero).transform);

            Transform fixtures = Node("Fixtures", root.transform, Vector2.zero).transform;
            foreach (Vector2 p in layout.Doors) Place(Door, fixtures, p);
            foreach (Vector2 p in layout.Covers) Place(Cover, fixtures, p);
            var stairs = new StairDoorView[layout.Stairs.Length];
            for (int i = 0; i < stairs.Length; i++) stairs[i] = Place(StairDoor, fixtures, layout.Stairs[i].Position).GetComponent<StairDoorView>();
            for (int i = 0; i < stairs.Length; i++) stairs[i].Link = stairs[layout.Stairs[i].Link];
            var vents = new VentView[layout.Vents.Length];
            for (int i = 0; i < vents.Length; i++) vents[i] = Place(Vent, fixtures, layout.Vents[i].Position).GetComponent<VentView>();
            for (int i = 0; i < vents.Length; i++) vents[i].Link = vents[layout.Vents[i].Link];
            foreach (LightRVO light in layout.Lights)
            {
                var room = Place(LightSwitch, fixtures, new Vector2(light.Switch, light.Floor)).GetComponent<LightRoomView>();
                room.Left = light.Left;
                room.Right = light.Right;
            }
            foreach (TerminalRVO terminal in layout.Terminals)
            {
                var view = Place(Computer, fixtures, terminal.Position).GetComponent<TerminalView>();
                view.Title = terminal.Title;
                view.Text = terminal.Text;
            }
            Place(Exit, fixtures, new Vector2(layout.ExitX, 0f));

            Transform spawns = Node("Spawns", root.transform, Vector2.zero).transform;
            Node("Player Spawn", spawns, layout.PlayerSpawn).AddComponent<PlayerSpawn>();
            for (int i = 0; i < layout.Guards.Length; i++)
            {
                GuardSpawnVO g = layout.Guards[i];
                var spawn = Node("Guard Spawn " + i, spawns, g.Position).AddComponent<GuardSpawn>();
                spawn.Facing = g.Facing;
                spawn.PatrolLeft = g.Left;
                spawn.PatrolRight = g.Right;
                spawn.CharacterType = g.Type;
                spawn.Weapon = g.Weapon;
                spawn.name = "Spawn " + i + " " + g.Type;
            }
            foreach (WeaponSpawnVO w in layout.Weapons)
                Node("Weapon " + w.Weapon, spawns, w.Position).AddComponent<WeaponSpawn>().Weapon = w.Weapon;

            GameObject cameraObject = Node("Camera", root.transform, Vector2.zero);
            cameraObject.transform.localPosition = new Vector3(0f, 3.6f, -20f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.045f, 0.08f);
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.depth = 1;
            level.Camera = camera;

            Rain(root.transform);
            GameObject crosshair = Node("Crosshair", root.transform, Vector2.zero);
            Color sight = new(0.80f, 0.96f, 0.92f, 0.82f);
            Part("L", crosshair.transform, new Vector2(-0.16f, 0f), new Vector2(0.12f, 0.025f), sight, 50);
            Part("R", crosshair.transform, new Vector2(0.16f, 0f), new Vector2(0.12f, 0.025f), sight, 50);
            Part("D", crosshair.transform, new Vector2(0f, -0.16f), new Vector2(0.025f, 0.12f), sight, 50);
            Part("U", crosshair.transform, new Vector2(0f, 0.16f), new Vector2(0.025f, 0.12f), sight, 50);
            Part("Dot", crosshair.transform, Vector2.zero, new Vector2(0.025f, 0.025f), _red, 51);
            level.Crosshair = crosshair.transform;
            level.Dynamic = Node("Dynamic", root.transform, Vector2.zero).transform;
            SavePrefab<GameObject>(root, LevelPrefab);
        }

        // A level written by an earlier build - ladders instead of stairwell doors, desks instead of computers -
        // is rebuilt, and the ladder prefab it was the only user of is deleted after it.
        public bool NeedsUpgrade(bool report)
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(LevelPrefab);
            if (level == null) return false;
            bool stale = level.GetComponentsInChildren<StairDoorView>(true).Length == 0
                         || level.GetComponentsInChildren<TerminalView>(true).Length == 0
                         || level.GetComponentsInChildren<WeaponSpawn>(true).Length == 0;
            if (stale && report) _report.Add("PB_Level_NightShift was from an earlier build (no stairwell doors, computers or weapon markers) - rebuilt");
            return stale;
        }

        private void RetireLadder()
        {
            if (!Exists(RetiredLadder)) return;
            AssetDatabase.DeleteAsset(RetiredLadder);
            _report.Add("Deleted " + RetiredLadder + " - stairwell doors replace ladders");
        }

        private GameObject Place(string prefabPath, Transform parent, Vector2 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var instance = (GameObject) PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = position;
            return instance;
        }

        private void Rain(Transform parent)
        {
            GameObject go = Node("Rain", parent, new Vector2(0f, 15f));
            var rain = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = rain.main;
            main.loop = true;
            main.playOnAwake = true;
            main.prewarm = true;
            main.duration = 5f;
            main.startLifetime = 3f;
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = 0.016f;
            main.startSizeY = 0.4f;
            main.startColor = new Color(0.43f, 0.65f, 0.76f, 0.23f);
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = rain.emission;
            emission.rateOverTime = 110f;
            ParticleSystem.ShapeModule shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(44f, 0.1f, 0.1f);
            ParticleSystem.VelocityOverLifetimeModule velocity = rain.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            // All three axes share one mode, or Unity rejects the module.
            velocity.x = new ParticleSystem.MinMaxCurve(-1.6f, -1.4f);
            velocity.y = new ParticleSystem.MinMaxCurve(-10f, -7f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = _particle;
            renderer.sortingOrder = -60;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Rect("Puddle line", parent, 0f, -0.47f, 28f, 0.025f, new Color(0.3f, 0.67f, 0.72f, 0.12f), 25);
        }

        // ---------------------------------------------------------------- static art

        private void Rect(string name, Transform parent, float x, float y, float width, float height, Color color, int order) =>
            Part(name, parent, new Vector2(x, y), new Vector2(width, height), color, order);

        private void Backdrop(Transform parent)
        {
            var random = new System.Random(3187);
            for (int layer = 0; layer < 3; layer++)
            {
                Transform row = Node("Skyline " + layer, parent, Vector2.zero).transform;
                for (int i = 0; i < 19; i++)
                {
                    float x = -30f + i * 3.4f + layer * 0.8f;
                    float height = 5f + (float) random.NextDouble() * 10f;
                    float ground = -3f - layer;
                    Color wall = new(0.045f + layer * 0.015f, 0.075f + layer * 0.018f, 0.12f + layer * 0.018f);
                    Transform tower = Node("Tower " + i, row, Vector2.zero).transform;
                    Rect("Wall", tower, x, ground + height * 0.5f, 3f, height, wall, -100 + layer * 10);
                    Rect("Antenna", tower, x + 0.8f, ground + height + 0.35f, 0.05f, 0.7f, wall, -100 + layer * 10);
                    for (int r = 0; r < (int) height; r++)
                        for (int c = 0; c < 3; c++)
                            if (random.NextDouble() > 0.48)
                                Rect("Window", tower, x - 0.9f + c * 0.8f, ground + r + 0.5f, 0.22f, 0.37f,
                                    new Color(0.17f, 0.35f, 0.42f, 0.45f), -99 + layer * 10);
                }
            }
            Rect("Street", parent, 0f, -1.9f, 70f, 2.9f, new Color(0.035f, 0.07f, 0.10f), -65);
            for (int i = 0; i < 55; i++)
                Rect("Wet shine", parent, -18f + i * 0.69f, -0.75f - (i % 7) * 0.25f, 0.3f + i % 4 * 0.18f, 0.035f,
                    new Color(0.15f, 0.42f, 0.46f, 0.3f), -64);
        }

        private void Building(Transform parent)
        {
            Rect("Shell", parent, 0, 4.05f, 28.6f, 8.9f, _ink, -50);
            Rect("Interior", parent, 0, 4.1f, 28f, 8.3f, new Color(0.075f, 0.105f, 0.13f), -49);
            for (int floor = 0; floor < 2; floor++)
            {
                Transform storey = Node("Floor " + floor, parent, Vector2.zero).transform;
                float y = floor * 4.2f;
                for (int i = 0; i < 7; i++)
                {
                    Transform bay = Node("Bay " + i, storey, Vector2.zero).transform;
                    float x = -12f + i * 4f;
                    Color light = (i + floor) % 3 == 0 ? _amber : _teal;
                    Rect("Wash", bay, x, y + 1.9f, 3.85f, 3.75f, new Color(light.r * 0.095f, light.g * 0.095f, light.b * 0.095f), -48);
                    for (int r = 0; r < 8; r++)
                        for (int c = 0; c < 4; c++)
                            Rect("Tile", bay, x - 1.4f + c * 0.93f + r % 2 * 0.15f, y + 0.2f + r * 0.43f, 0.82f, 0.025f,
                                new Color(0.22f, 0.28f, 0.30f, 0.20f), -46);
                    Window(bay, x, y + 2.2f);
                    Rect("Lamp cord", bay, x, y + 3.68f, 0.045f, 0.6f, _steel, -30);
                    Rect("Lamp", bay, x, y + 3.36f, 0.65f, 0.12f, _ink, -29);
                    Rect("Bulb", bay, x, y + 3.28f, 0.5f, 0.055f, light, -28);
                    for (int band = 0; band < 7; band++)
                        Rect("Light cone", bay, x, y + 3.03f - band * 0.4f, 0.9f + band * 0.43f, 0.41f,
                            new Color(light.r, light.g, light.b, 0.022f + (6 - band) * 0.003f), -27);
                }
                Rect("Skirting", storey, 0f, y + 0.17f, 28f, 0.08f, new Color(0.22f, 0.28f, 0.29f), -26);
                Rect("Cornice", storey, 0f, y + 3.8f, 27.8f, 0.08f, _steel, -31);
                for (int i = 0; i < 14; i++) Rect("Bracket", storey, -13f + i * 2f, y + 3.8f, 0.065f, 0.21f, _ink, -30);
            }
            Slab(parent, -0.2f, 0.4f);
            Slab(parent, 4.05f, 0.3f);
            Slab(parent, 8.3f, 0.32f);
            Rect("Wall L", parent, -14.1f, 4f, 0.22f, 8.5f, _steel, -15);
            Rect("Wall R", parent, 14.1f, 4f, 0.22f, 8.5f, _steel, -15);
            Rect("Drainpipe", parent, -14.55f, 3.9f, 0.12f, 8.7f, _steel, -16);
            for (int i = 0; i < 10; i++) Rect("Pipe clamp", parent, -14.55f, i * 0.85f, 0.23f, 0.08f, _ink, -15);
            Rect("Roof unit", parent, -4.4f, 8.75f, 3.8f, 0.65f, _ink, -20);
            for (int i = 0; i < 12; i++) Rect("Fin", parent, -6f + i * 0.29f, 8.75f, 0.08f, 0.45f, _steel, -19);
            Rect("Antenna", parent, 7f, 8.95f, 0.1f, 1.2f, _steel, -20);
            Rect("Antenna bar", parent, 7f, 9.4f, 2.2f, 0.045f, _steel, -20);
            Rect("Antenna bar", parent, 7.3f, 9.7f, 1.6f, 0.045f, _steel, -20);
            Rect("Billboard", parent, -9.9f, 9.1f, 6.7f, 1.02f, _ink, -10);
            Rect("Billboard neon", parent, -9.9f, 8.65f, 6.2f, 0.035f, _teal, -9);
            Sign("N I G H T   S H I F T", parent, new Vector2(-9.9f, 9.12f), 0.33f, _teal);
            Sign("COLD STORAGE / 07", parent, new Vector2(8f, 7.65f), 0.19f, new Color(0.48f, 0.62f, 0.63f));
            Sign("DELIVERIES", parent, new Vector2(-10.9f, 3.2f), 0.17f, _amber);
            Sign("STAFF ONLY", parent, new Vector2(0f, 7.4f), 0.17f, _amber);
        }

        private void Window(Transform parent, float x, float y)
        {
            Transform window = Node("Window", parent, Vector2.zero).transform;
            Rect("Frame", window, x, y, 2.15f, 1.6f, _ink, -44);
            Rect("Glass", window, x, y, 1.98f, 1.44f, new Color(0.07f, 0.18f, 0.23f), -43);
            Rect("Glint", window, x - 0.7f, y, 0.18f, 1.4f, new Color(0.11f, 0.25f, 0.29f), -42);
            Rect("Mullion", window, x, y, 0.07f, 1.5f, _steel, -41);
            Rect("Transom", window, x, y, 2f, 0.07f, _steel, -41);
            for (int i = 0; i < 4; i++) Rect("Blind", window, x, y + 0.64f - i * 0.11f, 2f, 0.045f, _ink, -40);
            Rect("Sill", window, x, y - 0.82f, 2.28f, 0.1f, _steel, -39);
        }

        private void Slab(Transform parent, float y, float height)
        {
            Transform slab = Node("Slab", parent, Vector2.zero).transform;
            Rect("Concrete", slab, 0, y, 28.5f, height, _ink, -12);
            Rect("Edge", slab, 0, y + height * 0.5f - 0.025f, 28.5f, 0.05f, _steel, -11);
            for (int i = 0; i < 35; i++) Rect("Joint", slab, -13.6f + i * 0.8f, y - 0.04f, 0.04f, height * 0.6f, _steel, -11);
        }
    }
}
#endif
