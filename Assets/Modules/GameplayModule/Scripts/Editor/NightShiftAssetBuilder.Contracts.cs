#if UNITY_EDITOR
using System.Collections.Generic;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The campaign between the first job and the Docks: ten contracts, easiest first. Each is a recipe -
    // size, storeys, palette, who guards it and how sharp they are - and the builder lays the rooms,
    // doors, stairs, cover, props and patrols out from the recipe's seed, so the same recipe builds the
    // same level every time. Delete a level prefab and Build Missing writes it again.
    public partial class NightShiftAssetBuilder
    {
        private class ContractRecipe
        {
            public string Id, Title, Briefing, Sign, Lore;
            public int Seed, Floors, Reward, WorkPayment;
            public float Half;
            public LevelDifficulty Difficulty;
            public Color Wall, Trim, Sky, Glow;
            public bool Rain;
            // Character ids with weights; guards are drawn from these.
            public (string, float)[] Cast;
            public int GuardsPerRoom;
            public float Aggression;
            public int Toughness;
            // Props per room, on average.
            public float Barrels, Lamps, Clutter;
            public string[] Pickups;
        }

        private static string ContractPath(string id) => Prefabs + "/Level/PB_Level_" + id + ".prefab";

        private readonly ContractRecipe[] _contracts =
        {
            new() {Id = "Laundromat", Title = "SPIN CYCLE  /  LUCKY SUDS", Briefing = "Two lookouts wash the family's money at night. Quiet job - nobody upstairs.",
                   Sign = "LUCKY  SUDS", Lore = "Cash in drum 4. Rinse twice.", Seed = 101, Floors = 1, Half = 12f, Reward = 300, WorkPayment = 30, Difficulty = LevelDifficulty.Easy,
                   Wall = new Color(0.12f, 0.16f, 0.17f), Trim = new Color(0.4f, 0.75f, 0.75f), Sky = new Color(0.03f, 0.04f, 0.06f), Glow = new Color(0.3f, 0.8f, 0.8f), Rain = true,
                   Cast = new[] {("triangle", 1f)}, GuardsPerRoom = 1, Aggression = 0.7f, Barrels = 0f, Lamps = 0.5f, Clutter = 1f, Pickups = new[] {"silenced_pistol"}},
            new() {Id = "Diner", Title = "LAST ORDERS  /  ROSIE'S DINER", Briefing = "A back-room card game. The dealers are slow and the coffee is worse.",
                   Sign = "R O S I E ' S", Lore = "Table 3 owes 400. Don't serve the Square.", Seed = 202, Floors = 1, Half = 15f, Reward = 450, WorkPayment = 45, Difficulty = LevelDifficulty.Easy,
                   Wall = new Color(0.2f, 0.11f, 0.1f), Trim = new Color(0.85f, 0.35f, 0.4f), Sky = new Color(0.05f, 0.03f, 0.05f), Glow = new Color(0.95f, 0.35f, 0.45f), Rain = false,
                   Cast = new[] {("triangle", 3f), ("pentagon", 1f)}, GuardsPerRoom = 1, Aggression = 0.8f, Barrels = 0.2f, Lamps = 0.7f, Clutter = 1.2f, Pickups = new[] {"pistol"}},
            new() {Id = "PawnShop", Title = "HOT GOODS  /  GOLDMAN PAWN", Briefing = "The pawnbroker keeps two floors of stolen goods and a nervous guard on each.",
                   Sign = "GOLDMAN  PAWN", Lore = "Rings in the safe. Guns under the counter.", Seed = 303, Floors = 2, Half = 13f, Reward = 600, WorkPayment = 60, Difficulty = LevelDifficulty.Easy,
                   Wall = new Color(0.15f, 0.13f, 0.08f), Trim = new Color(0.8f, 0.65f, 0.25f), Sky = new Color(0.04f, 0.035f, 0.03f), Glow = new Color(0.95f, 0.75f, 0.3f), Rain = true,
                   Cast = new[] {("triangle", 2f), ("pentagon", 1f)}, GuardsPerRoom = 1, Aggression = 0.85f, Barrels = 0.2f, Lamps = 0.8f, Clutter = 1.5f, Pickups = new[] {"revolver"}},
            new() {Id = "Gym", Title = "GLASS JAW  /  IRON BOX GYM", Briefing = "Hexagons train here after hours. They hit hard and hear everything.",
                   Sign = "IRON  BOX", Lore = "Fight fixed for round 3.", Seed = 404, Floors = 2, Half = 15f, Reward = 800, WorkPayment = 80, Difficulty = LevelDifficulty.Normal,
                   Wall = new Color(0.1f, 0.1f, 0.12f), Trim = new Color(0.85f, 0.2f, 0.2f), Sky = new Color(0.03f, 0.03f, 0.04f), Glow = new Color(0.95f, 0.25f, 0.2f), Rain = false,
                   Cast = new[] {("hexagon", 2f), ("triangle", 2f)}, GuardsPerRoom = 1, Aggression = 1f, Barrels = 0.3f, Lamps = 0.8f, Clutter = 1f, Pickups = new[] {"shotgun"}},
            new() {Id = "Cinema", Title = "MATINEE  /  THE ORPHEUM", Briefing = "The projectionist runs the family's film reels. Dark halls, long sightlines.",
                   Sign = "O R P H E U M", Lore = "Reel 2 is the ledger. Burn reel 1.", Seed = 505, Floors = 2, Half = 18f, Reward = 1000, WorkPayment = 100, Difficulty = LevelDifficulty.Normal,
                   Wall = new Color(0.18f, 0.06f, 0.08f), Trim = new Color(0.9f, 0.7f, 0.3f), Sky = new Color(0.04f, 0.02f, 0.03f), Glow = new Color(0.95f, 0.8f, 0.4f), Rain = true,
                   Cast = new[] {("triangle", 2f), ("pentagon", 2f), ("diamond", 1f)}, GuardsPerRoom = 1, Aggression = 1f, Barrels = 0.2f, Lamps = 1f, Clutter = 0.8f, Pickups = new[] {"smg", "pistol"}},
            new() {Id = "Hotel", Title = "ROOM SERVICE  /  HOTEL MERIDIAN", Briefing = "A lieutenant hides on the top floor. Corridors full of doors, guards that check them.",
                   Sign = "HOTEL  MERIDIAN", Lore = "Suite 12. He orders at midnight.", Seed = 606, Floors = 2, Half = 20f, Reward = 1250, WorkPayment = 125, Difficulty = LevelDifficulty.Normal,
                   Wall = new Color(0.14f, 0.12f, 0.16f), Trim = new Color(0.65f, 0.5f, 0.85f), Sky = new Color(0.03f, 0.03f, 0.06f), Glow = new Color(0.7f, 0.5f, 0.95f), Rain = true,
                   Cast = new[] {("triangle", 2f), ("pentagon", 2f), ("hexagon", 1f)}, GuardsPerRoom = 2, Aggression = 1.05f, Barrels = 0.2f, Lamps = 1.2f, Clutter = 1.2f, Pickups = new[] {"silenced_pistol", "smg"}},
            new() {Id = "Distillery", Title = "PROOF  /  BLACKWATER DISTILLERY", Briefing = "The family's still. Barrels everywhere - and every barrel burns.",
                   Sign = "BLACKWATER", Lore = "Batch 9 goes out Friday. Mind the fumes.", Seed = 707, Floors = 2, Half = 21f, Reward = 1500, WorkPayment = 150, Difficulty = LevelDifficulty.Hard,
                   Wall = new Color(0.16f, 0.11f, 0.06f), Trim = new Color(0.9f, 0.5f, 0.15f), Sky = new Color(0.04f, 0.03f, 0.02f), Glow = new Color(1f, 0.55f, 0.15f), Rain = false,
                   Cast = new[] {("pentagon", 2f), ("hexagon", 2f), ("triangle", 1f)}, GuardsPerRoom = 2, Aggression = 1.15f, Toughness = 0, Barrels = 1.6f, Lamps = 0.8f, Clutter = 0.6f, Pickups = new[] {"shotgun", "smg"}},
            new() {Id = "Casino", Title = "HOUSE EDGE  /  THE GOLDEN CIRCLE", Briefing = "The family's casino. Diamonds on the floor, sharpshooters on the balcony.",
                   Sign = "GOLDEN  CIRCLE", Lore = "The house always wins. Count room upstairs.", Seed = 808, Floors = 2, Half = 23f, Reward = 1800, WorkPayment = 180, Difficulty = LevelDifficulty.Hard,
                   Wall = new Color(0.08f, 0.13f, 0.09f), Trim = new Color(0.95f, 0.8f, 0.3f), Sky = new Color(0.02f, 0.04f, 0.03f), Glow = new Color(0.4f, 0.95f, 0.5f), Rain = true,
                   Cast = new[] {("diamond", 2f), ("pentagon", 2f), ("triangle", 1f)}, GuardsPerRoom = 2, Aggression = 1.2f, Toughness = 1, Barrels = 0.4f, Lamps = 1.4f, Clutter = 1.2f, Pickups = new[] {"rifle", "smg"}},
            new() {Id = "Precinct", Title = "BOUGHT BADGES  /  PRECINCT 13", Briefing = "Crooked cops on the family's payroll. They react fast and never stop looking.",
                   Sign = "PRECINCT  13", Lore = "Evidence room key: under the mug.", Seed = 909, Floors = 2, Half = 25f, Reward = 2100, WorkPayment = 210, Difficulty = LevelDifficulty.Brutal,
                   Wall = new Color(0.1f, 0.12f, 0.16f), Trim = new Color(0.3f, 0.5f, 0.95f), Sky = new Color(0.02f, 0.03f, 0.06f), Glow = new Color(0.35f, 0.55f, 1f), Rain = true,
                   Cast = new[] {("triangle", 2f), ("diamond", 2f), ("hexagon", 2f), ("pentagon", 1f)}, GuardsPerRoom = 2, Aggression = 1.35f, Toughness = 1, Barrels = 0.3f, Lamps = 1f, Clutter = 1f, Pickups = new[] {"shotgun", "rifle"}},
            new() {Id = "Penthouse", Title = "TOP FLOOR  /  THE DON'S PENTHOUSE", Briefing = "The Don himself, two floors of his best. Nothing on this list is harder.",
                   Sign = "T H E   D O N", Lore = "After the docks, the Square comes for me. Let him.", Seed = 1010, Floors = 2, Half = 27f, Reward = 2400, WorkPayment = 240, Difficulty = LevelDifficulty.Brutal,
                   Wall = new Color(0.13f, 0.08f, 0.1f), Trim = new Color(0.9f, 0.15f, 0.25f), Sky = new Color(0.03f, 0.015f, 0.025f), Glow = new Color(1f, 0.2f, 0.3f), Rain = true,
                   Cast = new[] {("diamond", 2f), ("hexagon", 2f), ("pentagon", 2f), ("triangle", 1f)}, GuardsPerRoom = 3, Aggression = 1.5f, Toughness = 1, Barrels = 0.6f, Lamps = 1.2f, Clutter = 1f, Pickups = new[] {"rifle", "shotgun", "smg"}},
        };

        private bool ContractsMissing()
        {
            foreach (ContractRecipe recipe in _contracts)
                if (!Exists(ContractPath(recipe.Id))) return true;
            var missions = AssetDatabase.LoadAssetAtPath<CD_Missions>(MissionsAsset);
            if (missions != null)
                foreach (ContractRecipe recipe in _contracts)
                    if (missions.Find(recipe.Id.ToLowerInvariant()) == null) return true;
            return false;
        }

        private void BuildContracts()
        {
            foreach (ContractRecipe recipe in _contracts)
                if (ShouldWrite(ContractPath(recipe.Id))) BuildContract(recipe);
            RegisterContracts();
        }

        private void BuildContract(ContractRecipe r)
        {
            var random = new System.Random(r.Seed);
            float Range(float a, float b) => a + (float) random.NextDouble() * (b - a);
            GameObject root = Node("PB_Level_" + r.Id, null, Vector2.zero);
            var level = root.AddComponent<LevelAuthoring>();
            level.FloorHeight = H;
            level.HalfWidth = r.Half;
            level.Difficulty = r.Difficulty;
            level.FollowCamera = r.Half > 14f;

            Transform art = Node("Art", root.transform, Vector2.zero).transform;
            ContractBackdrop(Node("Backdrop", art, Vector2.zero).transform, r, random);
            ContractBuilding(Node("Building", art, Vector2.zero).transform, r, random);

            Transform fixtures = Node("Fixtures", root.transform, Vector2.zero).transform;
            Transform spawns = Node("Spawns", root.transform, Vector2.zero).transform;
            float left = -r.Half, right = r.Half;
            var rooms = new List<(float floor, float a, float b)>();
            for (int floor = 0; floor < r.Floors; floor++)
            {
                float y = floor * H;
                // Rooms of 7 to 11 units; a door stands on every wall between two.
                float x = left;
                while (x < right)
                {
                    float end = right - x < 13f ? right : x + Range(7f, 11f);
                    rooms.Add((y, x, end));
                    if (end < right) Place(Door, fixtures, new Vector2(end, y));
                    x = end;
                }
            }
            foreach ((float floor, float a, float b) in rooms) Room(fixtures, floor, a, b, a + Range(1.2f, 2.2f));

            // Stairs at both ends of the building; a long one gets a third in the middle, plus a vent.
            if (r.Floors > 1)
            {
                Pair(StairDoor, fixtures, new Vector2(left + 3f, 0f), new Vector2(left + 3f, H));
                Pair(StairDoor, fixtures, new Vector2(right - 2f, 0f), new Vector2(right - 2f, H));
                if (r.Half > 18f) Pair(StairDoor, fixtures, new Vector2(Range(-3f, 3f), 0f), new Vector2(Range(-3f, 3f), H));
                if (r.Half > 14f) Pair(Vent, fixtures, new Vector2(left + r.Half * 0.6f, 0f), new Vector2(right - r.Half * 0.5f, H));
            }

            var cast = new List<(string, float)>(r.Cast);
            float total = 0f;
            foreach ((string _, float weight) in cast) total += weight;
            string DrawCast()
            {
                float roll = Range(0f, total);
                foreach ((string id, float weight) in cast)
                    if ((roll -= weight) <= 0f) return id;
                return cast[0].Item1;
            }

            int guard = 0;
            int pickup = 0;
            for (int i = 0; i < rooms.Count; i++)
            {
                (float floor, float a, float b) = rooms[i];
                bool first = floor == 0f && i == 0;
                float width = b - a;
                // Cover breaks every room's line of sight; the player's room stays empty of guards.
                Place(Cover, fixtures, new Vector2(a + width * Range(0.35f, 0.65f), floor));
                if (width > 9f) Place(Cover, fixtures, new Vector2(a + width * Range(0.75f, 0.85f), floor));
                Scatter(PropPath(PropKind.Barrel), r.Barrels);
                Scatter(PropPath(PropKind.CeilingLamp), r.Lamps);
                Scatter(PropPath(random.Next(3) switch {0 => PropKind.Bottles, 1 => PropKind.Monitor, _ => PropKind.Plant}), r.Clutter);
                if (first) continue;

                int count = Mathf.Max(1, r.GuardsPerRoom - (random.NextDouble() < 0.35 ? 1 : 0));
                for (int g = 0; g < count; g++)
                {
                    float at = a + width * Range(0.2f, 0.8f);
                    var spawn = Node("Spawn " + guard++, spawns, new Vector2(at, floor)).AddComponent<GuardSpawn>();
                    spawn.CharacterType = DrawCast();
                    spawn.Facing = random.Next(2) == 0 ? -1 : 1;
                    spawn.PatrolLeft = Mathf.Max(a + 0.6f, at - Range(1.5f, 3.5f));
                    spawn.PatrolRight = Mathf.Min(b - 0.6f, at + Range(1.5f, 3.5f));
                    spawn.Toughness = r.Toughness + (random.NextDouble() < 0.15 ? 1 : 0);
                    spawn.Aggression = r.Aggression * Range(0.9f, 1.15f);
                }
                if (pickup < r.Pickups.Length && random.NextDouble() < 0.5)
                    Node("Weapon " + r.Pickups[pickup], spawns, new Vector2(a + width * 0.5f, floor)).AddComponent<WeaponSpawn>().Weapon = r.Pickups[pickup++];

                void Scatter(string prefab, float amount)
                {
                    int n = (int) amount + (random.NextDouble() < amount - (int) amount ? 1 : 0);
                    for (int p = 0; p < n; p++) Place(prefab, fixtures, new Vector2(a + width * Range(0.15f, 0.85f), floor));
                }
            }
            // Pickups the rooms did not place sit by the entrance.
            for (; pickup < r.Pickups.Length; pickup++)
                Node("Weapon " + r.Pickups[pickup], spawns, new Vector2(left + 2.4f + pickup * 0.8f, 0f)).AddComponent<WeaponSpawn>().Weapon = r.Pickups[pickup];

            var terminal = Place(Computer, fixtures, new Vector2(right - 4f, (r.Floors - 1) * H)).GetComponent<TerminalView>();
            terminal.Title = r.Sign + " / NOTES";
            terminal.Text = r.Lore;
            Place(Exit, fixtures, new Vector2(left + 0.8f, 0f));
            Node("Player Spawn", spawns, new Vector2(left + 1.8f, 0f)).AddComponent<PlayerSpawn>();

            GameObject cameraObject = Node("Camera", root.transform, Vector2.zero);
            cameraObject.transform.localPosition = new Vector3(level.FollowCamera ? left + 7f : 0f, r.Floors > 1 ? 4.1f : 2.2f, -20f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = r.Floors > 1 ? 6.6f : 4.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = r.Sky;
            camera.depth = 1;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            level.Camera = camera;
            if (r.Rain)
            {
                Rain(root.transform);
                ParticleSystem.ShapeModule shape = root.transform.Find("Rain").GetComponent<ParticleSystem>().shape;
                shape.scale = new Vector3(r.Half * 2f + 20f, 0.1f, 0.1f);
            }
            GameObject crosshair = Node("Crosshair", root.transform, Vector2.zero);
            Color sight = new(0.85f, 0.75f, 0.55f, 0.85f);
            Part("L", crosshair.transform, new Vector2(-0.16f, 0f), new Vector2(0.12f, 0.025f), sight, 50);
            Part("R", crosshair.transform, new Vector2(0.16f, 0f), new Vector2(0.12f, 0.025f), sight, 50);
            Part("D", crosshair.transform, new Vector2(0f, -0.16f), new Vector2(0.025f, 0.12f), sight, 50);
            Part("U", crosshair.transform, new Vector2(0f, 0.16f), new Vector2(0.025f, 0.12f), sight, 50);
            Part("Dot", crosshair.transform, Vector2.zero, new Vector2(0.025f, 0.025f), _crimson, 51);
            level.Crosshair = crosshair.transform;
            level.Dynamic = Node("Dynamic", root.transform, Vector2.zero).transform;
            SavePrefab<GameObject>(root, ContractPath(r.Id));
        }

        private void ContractBackdrop(Transform parent, ContractRecipe r, System.Random random)
        {
            float span = r.Half * 2f + 50f;
            Rect("Sky", parent, 0f, 8f, span, 26f, r.Sky, -110);
            for (int layer = 0; layer < 2; layer++)
                for (float x = -span * 0.5f; x < span * 0.5f; x += 3.4f)
                {
                    float height = 3f + (float) random.NextDouble() * 10f;
                    Color tower = Color.Lerp(r.Sky, _ink, 0.4f) * (1.1f + layer * 0.3f);
                    tower.a = 1f;
                    Rect("Tower", parent, x + layer * 1.4f, -2f + height * 0.5f, 3f, height, tower, -100 + layer * 10);
                    for (int w = 0; w < (int) height; w++)
                        if (random.NextDouble() > 0.75)
                            Rect("Window", parent, x + layer * 1.4f - 1f + (float) random.NextDouble() * 2f, -1.5f + w, 0.2f, 0.3f, new Color(r.Glow.r, r.Glow.g, r.Glow.b, 0.3f), -99 + layer * 10);
                }
            Rect("Street", parent, 0f, -1.6f, span, 2.4f, _ink, -65);
        }

        private void ContractBuilding(Transform parent, ContractRecipe r, System.Random random)
        {
            float w = r.Half * 2f;
            float top = r.Floors * H;
            Rect("Shell", parent, 0f, top * 0.5f, w + 0.6f, top + 0.6f, _ink, -50);
            Rect("Interior", parent, 0f, top * 0.5f, w, top, r.Wall * 0.7f, -49);
            for (int floor = 0; floor < r.Floors; floor++)
            {
                Transform storey = Node("Floor " + floor, parent, Vector2.zero).transform;
                float y = floor * H;
                Color wall = floor == 0 ? r.Wall : r.Wall * 1.15f;
                wall.a = 1f;
                for (float x = -r.Half + 2f; x < r.Half; x += 4f)
                {
                    Rect("Panel", storey, x, y + 1.9f, 3.9f, 3.7f, wall, -48);
                    Rect("Stripe", storey, x - 1.2f, y + 1.9f, 0.07f, 3.6f, r.Trim * 0.35f, -47);
                    Rect("Wainscot", storey, x, y + 0.5f, 3.9f, 0.9f, wall * 0.7f, -46);
                    int pick = random.Next(4);
                    if (pick == 0) { Rect("Frame", storey, x, y + 2.4f, 1f, 0.8f, r.Trim * 0.8f, -44); Rect("Painting", storey, x, y + 2.4f, 0.86f, 0.66f, wall * 0.6f, -43); }
                    else if (pick == 1) { Rect("Window", storey, x + 0.6f, y + 2.3f, 1.2f, 1.4f, r.Sky, -44); Rect("Window glow", storey, x + 0.6f, y + 2.3f, 1.2f, 0.06f, r.Glow * 0.6f, -43); }
                    else if (pick == 2) { Rect("Shelf", storey, x, y + 1.4f, 1.6f, 0.08f, r.Trim * 0.5f, -44); Rect("Stock", storey, x, y + 1.62f, 1.3f, 0.36f, wall * 1.4f, -43); }
                }
                Rect("Skirting", storey, 0f, y + 0.17f, w, 0.08f, r.Trim * 0.4f, -26);
                Rect("Cornice", storey, 0f, y + 3.8f, w, 0.08f, r.Trim * 0.4f, -31);
                Rect("Slab", parent, 0f, y + (floor == 0 ? -0.2f : -0.15f), w + 0.5f, floor == 0 ? 0.4f : 0.3f, _ink, -12);
            }
            Rect("Roof slab", parent, 0f, top + 0.1f, w + 0.5f, 0.32f, _ink, -12);
            Rect("Wall L", parent, -r.Half - 0.1f, top * 0.5f, 0.22f, top + 0.3f, _steel, -15);
            Rect("Wall R", parent, r.Half + 0.1f, top * 0.5f, 0.22f, top + 0.3f, _steel, -15);
            Rect("Billboard", parent, 0f, top + 1f, Mathf.Min(w - 2f, 12f), 1.2f, _ink, -10);
            Rect("Billboard neon", parent, 0f, top + 0.5f, Mathf.Min(w - 2f, 12f) - 0.6f, 0.05f, r.Glow, -9);
            Sign(r.Sign, parent, new Vector2(0f, top + 1.02f), 0.3f, r.Glow);
        }

        // The contracts go on the board in order, between the first job and the Docks.
        private void RegisterContracts()
        {
            var missions = AssetDatabase.LoadAssetAtPath<CD_Missions>(MissionsAsset);
            if (missions == null) return;
            int at = missions.Missions.FindIndex(m => m.Id == "docks_massacre");
            if (at < 0) at = missions.Missions.Count;
            foreach (ContractRecipe r in _contracts)
            {
                string id = r.Id.ToLowerInvariant();
                if (missions.Find(id) != null) { at = missions.Missions.FindIndex(m => m.Id == id) + 1; continue; }
                missions.Missions.Insert(at++, new MissionCVO
                {
                    Id = id, Title = r.Title, Briefing = r.Briefing, Reward = r.Reward, WorkPayment = r.WorkPayment,
                    LocationDifficulty = r.Difficulty >= LevelDifficulty.Hard ? LocationDifficulty.Hard
                        : r.Difficulty == LevelDifficulty.Normal ? LocationDifficulty.Medium : LocationDifficulty.Easy,
                    AttackChance = r.Difficulty >= LevelDifficulty.Hard ? 30f : r.Difficulty == LevelDifficulty.Normal ? 20f : 10f,
                    OwnerFamily = id switch
                    {
                        "laundromat" or "diner" => "circle",
                        "pawnshop" => "pentagon",
                        "gym" => "hexagon",
                        "casino" => "diamond",
                        _ => "rival",
                    },
                    AttackingFamily = "rival",
                    Level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(ContractPath(r.Id)),
                });
                _report.Add("Added the " + r.Title + " contract to CD_Missions");
            }
            EditorUtility.SetDirty(missions);
        }
    }
}
#endif
