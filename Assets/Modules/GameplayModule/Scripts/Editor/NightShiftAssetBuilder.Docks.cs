#if UNITY_EDITOR
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // "Docks Massacre": a long two-storey warehouse on the waterfront, Brutal, built to be loud.
    // Ground floor is the loading dock, upstairs the family's offices; barrels and lamps are placed
    // over patrol routes so a good shot clears a room.
    public partial class NightShiftAssetBuilder
    {
        public const string DocksPrefab = Prefabs + "/Level/PB_Level_Docks.prefab";
        private const float DocksHalf = 30f;
        private const float H = 4.2f;

        private void BuildDocks()
        {
            if (!ShouldWrite(DocksPrefab)) return;
            GameObject root = Node("PB_Level_Docks", null, Vector2.zero);
            var level = root.AddComponent<LevelAuthoring>();
            level.FloorHeight = H;
            level.HalfWidth = DocksHalf;
            level.Difficulty = LevelDifficulty.Brutal;
            level.FollowCamera = true;

            Transform art = Node("Art", root.transform, Vector2.zero).transform;
            DocksBackdrop(Node("Backdrop", art, Vector2.zero).transform);
            DocksBuilding(Node("Building", art, Vector2.zero).transform);

            Transform fixtures = Node("Fixtures", root.transform, Vector2.zero).transform;
            // Walls between rooms are doors; every room has a switch.
            foreach (float x in new[] {-22f, -10f, 2f, 14f, 24f}) Place(Door, fixtures, new Vector2(x, 0f));
            foreach (float x in new[] {-16f, -2f, 10f, 20f}) Place(Door, fixtures, new Vector2(x, H));
            Room(fixtures, 0f, -30f, -22f, -26f);
            Room(fixtures, 0f, -22f, -10f, -16.8f);
            Room(fixtures, 0f, -10f, 2f, -5f);
            Room(fixtures, 0f, 2f, 14f, 9.2f);
            Room(fixtures, 0f, 14f, 24f, 18.2f);
            Room(fixtures, 0f, 24f, 30f, 26.6f);
            Room(fixtures, H, -30f, -16f, -22.8f);
            Room(fixtures, H, -16f, -2f, -9.8f);
            Room(fixtures, H, -2f, 10f, 1.6f);
            Room(fixtures, H, 10f, 20f, 13.2f);
            Room(fixtures, H, 20f, 30f, 24.6f);
            foreach (Vector2 p in new[]
                     {
                         new Vector2(-19f, 0f), new Vector2(-13.5f, 0f), new Vector2(-7f, 0f), new Vector2(-1f, 0f), new Vector2(5f, 0f),
                         new Vector2(11f, 0f), new Vector2(16.5f, 0f), new Vector2(21.5f, 0f), new Vector2(27.5f, 0f),
                         new Vector2(-26f, H), new Vector2(-11.5f, H), new Vector2(-5f, H), new Vector2(3.5f, H), new Vector2(14.5f, H), new Vector2(23f, H),
                     })
                Place(Cover, fixtures, p);
            Pair(StairDoor, fixtures, new Vector2(-24f, 0f), new Vector2(-24f, H));
            Pair(StairDoor, fixtures, new Vector2(6.5f, 0f), new Vector2(6.5f, H));
            Pair(StairDoor, fixtures, new Vector2(28.5f, 0f), new Vector2(28.5f, H));
            Pair(Vent, fixtures, new Vector2(-18f, 0f), new Vector2(-4f, H));
            Pair(Vent, fixtures, new Vector2(8f, 0f), new Vector2(21.5f, H));
            Pair(Vent, fixtures, new Vector2(-28f, H), new Vector2(15.5f, 0f));
            DocksComputer(fixtures, new Vector2(-8.5f, 0f), "DOCK OFFICE / MANIFEST", "Crate 44 - 'machine parts'. Do not open.\nThe Pentagons pick up at dawn.");
            DocksComputer(fixtures, new Vector2(17.5f, H), "THE FAMILY / LEDGER", "Every Triangle on the dock is on the payroll.\nThe Square is coming. Double the guard.");

            foreach (Vector2 p in new[] {new Vector2(-15f, 0f), new Vector2(-0.2f, 0f), new Vector2(12.2f, 0f), new Vector2(20.5f, 0f),
                                         new Vector2(-12.8f, H), new Vector2(7.8f, H), new Vector2(25.8f, H)})
                Place(PropPath(PropKind.Barrel), fixtures, p);
            foreach (Vector2 p in new[] {new Vector2(-16.8f, 0f), new Vector2(-4f, 0f), new Vector2(8.5f, 0f), new Vector2(19f, 0f),
                                         new Vector2(-21f, H), new Vector2(-8f, H), new Vector2(4.5f, H), new Vector2(16f, H), new Vector2(26.5f, H)})
                Place(PropPath(PropKind.CeilingLamp), fixtures, p);
            foreach (Vector2 p in new[] {new Vector2(-11.2f, 0f), new Vector2(3.4f, 0f), new Vector2(-2.8f, H), new Vector2(11.8f, H)})
                Place(PropPath(PropKind.Bottles), fixtures, p);
            foreach (Vector2 p in new[] {new Vector2(-6f, 0f), new Vector2(-14.2f, H), new Vector2(0.4f, H), new Vector2(19.2f, H)})
                Place(PropPath(PropKind.Monitor), fixtures, p);
            foreach (Vector2 p in new[] {new Vector2(-28.8f, 0f), new Vector2(-17.4f, H), new Vector2(9.2f, H), new Vector2(29f, H)})
                Place(PropPath(PropKind.Plant), fixtures, p);
            Place(Exit, fixtures, new Vector2(-29.2f, 0f));

            Transform spawns = Node("Spawns", root.transform, Vector2.zero).transform;
            Node("Player Spawn", spawns, new Vector2(-28.2f, 0f)).AddComponent<PlayerSpawn>();
            // x, floor, type, facing, patrol left, patrol right, extra toughness, aggression
            object[][] cast =
            {
                new object[] {-17.5f, 0f, "triangle", -1, -20.5f, -14.2f, 0, 1f},
                new object[] {-12.5f, 0f, "hexagon", 1, -14f, -11f, 0, 1f},
                new object[] {-7.8f, 0f, "pentagon", -1, -9.4f, -3.5f, 0, 1f},
                new object[] {-3f, 0f, "triangle", 1, -5f, 1.2f, 0, 1.2f},
                new object[] {3.6f, 0f, "diamond", 1, 3f, 5.5f, 0, 1f},
                new object[] {9.6f, 0f, "hexagon", -1, 7.5f, 13f, 1, 1f},
                new object[] {13f, 0f, "pentagon", 1, 11.8f, 13.5f, 0, 1.2f},
                new object[] {16f, 0f, "triangle", -1, 15f, 18f, 0, 1.3f},
                new object[] {21f, 0f, "hexagon", 1, 18.5f, 23.2f, 0, 1f},
                new object[] {26f, 0f, "diamond", -1, 24.8f, 27.8f, 0, 1f},
                new object[] {-27f, H, "triangle", 1, -29.2f, -24.8f, 0, 1f},
                new object[] {-20f, H, "pentagon", -1, -22.5f, -17f, 0, 1f},
                new object[] {-13.5f, H, "diamond", 1, -15.2f, -10.5f, 0, 1f},
                new object[] {-8.5f, H, "triangle", -1, -10f, -6f, 0, 1.3f},
                new object[] {-3.4f, H, "hexagon", 1, -6f, -2.6f, 1, 1f},
                new object[] {0.8f, H, "pentagon", 1, -1.2f, 4.8f, 0, 1f},
                new object[] {8.8f, H, "triangle", -1, 7f, 9.4f, 0, 1.2f},
                new object[] {12.2f, H, "hexagon", 1, 10.8f, 15.8f, 1, 1f},
                new object[] {17f, H, "diamond", -1, 16.2f, 19.4f, 0, 1f},
                new object[] {22.2f, H, "pentagon", 1, 20.6f, 25.2f, 0, 1.2f},
                new object[] {27.2f, H, "triangle", -1, 26f, 29.2f, 0, 1f},
                new object[] {24.2f, H, "hexagon", -1, 23.6f, 27f, 2, 1.4f},
            };
            for (int i = 0; i < cast.Length; i++)
            {
                object[] c = cast[i];
                var spawn = Node("Spawn " + i + " " + c[2], spawns, new Vector2((float) c[0], (float) c[1])).AddComponent<GuardSpawn>();
                spawn.CharacterType = (string) c[2];
                spawn.Facing = (int) c[3];
                spawn.PatrolLeft = (float) c[4];
                spawn.PatrolRight = (float) c[5];
                spawn.Toughness = (int) c[6];
                spawn.Aggression = (float) c[7];
            }
            foreach ((Vector2 p, string weapon) in new[]
                     {
                         (new Vector2(-26.5f, 0f), "shotgun"), (new Vector2(-20f, 0f), "silenced_pistol"),
                         (new Vector2(-21.5f, H), "smg"), (new Vector2(11f, H), "rifle"), (new Vector2(0.6f, 0f), "smg"),
                     })
                Node("Weapon " + weapon, spawns, p).AddComponent<WeaponSpawn>().Weapon = weapon;

            GameObject cameraObject = Node("Camera", root.transform, Vector2.zero);
            cameraObject.transform.localPosition = new Vector3(-22f, 4.1f, -20f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
            camera.depth = 1;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            level.Camera = camera;
            Rain(root.transform);
            ParticleSystem.ShapeModule rain = root.transform.Find("Rain").GetComponent<ParticleSystem>().shape;
            rain.scale = new Vector3(80f, 0.1f, 0.1f);
            ParticleSystem.EmissionModule wet = root.transform.Find("Rain").GetComponent<ParticleSystem>().emission;
            wet.rateOverTime = 260f;
            GameObject crosshair = Node("Crosshair", root.transform, Vector2.zero);
            Color sight = new(0.85f, 0.75f, 0.55f, 0.85f);
            Part("L", crosshair.transform, new Vector2(-0.16f, 0f), new Vector2(0.12f, 0.025f), sight, 50);
            Part("R", crosshair.transform, new Vector2(0.16f, 0f), new Vector2(0.12f, 0.025f), sight, 50);
            Part("D", crosshair.transform, new Vector2(0f, -0.16f), new Vector2(0.025f, 0.12f), sight, 50);
            Part("U", crosshair.transform, new Vector2(0f, 0.16f), new Vector2(0.025f, 0.12f), sight, 50);
            Part("Dot", crosshair.transform, Vector2.zero, new Vector2(0.025f, 0.025f), _crimson, 51);
            level.Crosshair = crosshair.transform;
            level.Dynamic = Node("Dynamic", root.transform, Vector2.zero).transform;
            SavePrefab<GameObject>(root, DocksPrefab);
        }

        private void Room(Transform parent, float floor, float left, float right, float button)
        {
            var room = Place(LightSwitch, parent, new Vector2(button, floor)).GetComponent<LightRoomView>();
            room.Left = left;
            room.Right = right;
        }

        private void Pair(string prefab, Transform parent, Vector2 a, Vector2 b)
        {
            GameObject first = Place(prefab, parent, a);
            GameObject second = Place(prefab, parent, b);
            if (first.TryGetComponent(out StairDoorView up) && second.TryGetComponent(out StairDoorView down)) { up.Link = down; down.Link = up; }
            if (first.TryGetComponent(out VentView one) && second.TryGetComponent(out VentView two)) { one.Link = two; two.Link = one; }
        }

        private void DocksComputer(Transform parent, Vector2 at, string title, string text)
        {
            var view = Place(Computer, parent, at).GetComponent<TerminalView>();
            view.Title = title;
            view.Text = text;
        }

        private void DocksBackdrop(Transform parent)
        {
            var random = new System.Random(4401);
            Rect("Sky", parent, 0f, 8f, 110f, 26f, new Color(0.03f, 0.03f, 0.05f), -110);
            Rect("Moon", parent, 18f, 13f, 2f, 2f, new Color(0.6f, 0.55f, 0.45f, 0.5f), -108);
            for (int layer = 0; layer < 2; layer++)
                for (int i = 0; i < 30; i++)
                {
                    float x = -52f + i * 3.6f + layer * 1.3f;
                    float height = 4f + (float) random.NextDouble() * 9f;
                    Rect("Tower", parent, x, -2f + height * 0.5f, 3.2f, height, new Color(0.04f + layer * 0.015f, 0.035f + layer * 0.012f, 0.04f + layer * 0.012f), -100 + layer * 10);
                    for (int w = 0; w < (int) height; w++)
                        if (random.NextDouble() > 0.72)
                            Rect("Window", parent, x - 1f + (float) random.NextDouble() * 2f, -1.5f + w, 0.2f, 0.3f, new Color(0.55f, 0.42f, 0.2f, 0.4f), -99 + layer * 10);
                }
            // cranes over the water
            for (int i = 0; i < 3; i++)
            {
                float x = -40f + i * 38f;
                Rect("Crane mast", parent, x, 6f, 0.4f, 14f, new Color(0.06f, 0.05f, 0.05f), -95);
                Rect("Crane jib", parent, x + 4f, 12.6f, 10f, 0.3f, new Color(0.06f, 0.05f, 0.05f), -95);
                Rect("Crane light", parent, x + 8.6f, 12.4f, 0.15f, 0.15f, _crimson, -94);
            }
            Rect("Water", parent, 0f, -2.2f, 110f, 3.4f, new Color(0.02f, 0.04f, 0.06f), -65);
            for (int i = 0; i < 90; i++)
                Rect("Ripple", parent, -54f + i * 1.2f, -1.2f - (i % 5) * 0.4f, 0.4f + i % 3 * 0.3f, 0.03f, new Color(0.6f, 0.45f, 0.25f, 0.18f), -64);
        }

        private void DocksBuilding(Transform parent)
        {
            float w = DocksHalf * 2f;
            Rect("Shell", parent, 0f, 4.05f, w + 0.6f, 8.9f, _ink, -50);
            Rect("Interior", parent, 0f, 4.1f, w, 8.3f, new Color(0.09f, 0.075f, 0.07f), -49);
            for (int floor = 0; floor < 2; floor++)
            {
                Transform storey = Node("Floor " + floor, parent, Vector2.zero).transform;
                float y = floor * H;
                Color wall = floor == 0 ? new Color(0.12f, 0.1f, 0.09f) : new Color(0.14f, 0.1f, 0.1f);
                for (int i = 0; i < 15; i++)
                {
                    float x = -28f + i * 4f;
                    Rect("Panel", storey, x, y + 1.9f, 3.9f, 3.7f, wall, -48);
                    if (floor == 0)
                    {
                        for (int s = 0; s < 6; s++) Rect("Corrugation", storey, x - 1.7f + s * 0.68f, y + 1.9f, 0.05f, 3.6f, new Color(0.07f, 0.06f, 0.055f), -47);
                        if (i % 3 == 1) { Rect("Crate stack", storey, x + 1.1f, y + 0.9f, 1.3f, 1.8f, new Color(0.25f, 0.18f, 0.1f), -30); Rect("Crate band", storey, x + 1.1f, y + 1.1f, 1.3f, 0.08f, _ink, -29); }
                        if (i % 4 == 2) Sign("DOCK " + (i + 1), storey, new Vector2(x, y + 3.1f), 0.14f, _hatBrass);
                    }
                    else
                    {
                        Rect("Wallpaper stripe", storey, x - 1f, y + 1.9f, 0.08f, 3.6f, new Color(0.2f, 0.12f, 0.12f), -47);
                        Rect("Wallpaper stripe", storey, x + 1f, y + 1.9f, 0.08f, 3.6f, new Color(0.2f, 0.12f, 0.12f), -47);
                        if (i % 2 == 0) { Rect("Frame", storey, x, y + 2.4f, 1f, 0.8f, _hatBrass, -44); Rect("Painting", storey, x, y + 2.4f, 0.86f, 0.66f, new Color(0.2f, 0.12f, 0.1f), -43); }
                        Rect("Wainscot", storey, x, y + 0.5f, 3.9f, 0.9f, new Color(0.18f, 0.11f, 0.07f), -46);
                    }
                }
                Rect("Skirting", storey, 0f, y + 0.17f, w, 0.08f, new Color(0.2f, 0.15f, 0.1f), -26);
                Rect("Cornice", storey, 0f, y + 3.8f, w, 0.08f, new Color(0.2f, 0.16f, 0.12f), -31);
            }
            Rect("Ground slab", parent, 0f, -0.2f, w + 0.5f, 0.4f, _ink, -12);
            Rect("Middle slab", parent, 0f, 4.05f, w + 0.5f, 0.3f, _ink, -12);
            Rect("Roof slab", parent, 0f, 8.3f, w + 0.5f, 0.32f, _ink, -12);
            Rect("Wall L", parent, -DocksHalf - 0.1f, 4f, 0.22f, 8.5f, _steel, -15);
            Rect("Wall R", parent, DocksHalf + 0.1f, 4f, 0.22f, 8.5f, _steel, -15);
            Rect("Billboard", parent, 0f, 9.3f, 12f, 1.2f, _ink, -10);
            Rect("Billboard neon", parent, 0f, 8.8f, 11.4f, 0.04f, _crimson, -9);
            Sign("S H A P E   &   S O N S   S H I P P I N G", parent, new Vector2(0f, 9.32f), 0.3f, _hatBrass);
        }

        private void RegisterDocks()
        {
            var missions = AssetDatabase.LoadAssetAtPath<CD_Missions>(MissionsAsset);
            if (missions == null || missions.Find("docks_massacre") != null) return;
            missions.Missions.Add(new MissionCVO
            {
                Id = "docks_massacre", Title = "DOCKS MASSACRE  /  SHAPE & SONS", Reward = 2500, WorkPayment = 250,
                OwnerFamily = "rival", LocationDifficulty = LocationDifficulty.Hard, AttackChance = 30f, AttackingFamily = "rival",
                Level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(DocksPrefab),
                Briefing = "Twenty-two of the family hold the waterfront warehouse. Brutal. Barrels are live - use them.",
            });
            EditorUtility.SetDirty(missions);
            _report.Add("Added the Docks Massacre contract to CD_Missions");
        }
    }
}
#endif
