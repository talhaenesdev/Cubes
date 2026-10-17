#if UNITY_EDITOR
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The player's room - mission board, upgrade bench - and the two configs behind them.
    public partial class NightShiftAssetBuilder
    {
        public const string HideoutPrefab = Prefabs + "/Level/PB_Level_Hideout.prefab";
        public const string MissionsAsset = ModuleRoot + "/Scriptables/CD_Missions.asset";
        public const string UpgradesAsset = ModuleRoot + "/Scriptables/CD_Upgrades.asset";
        private const string MissionBoard = Prefabs + "/Fixtures/PB_MissionBoard.prefab";
        private const string FamilyBoard = Prefabs + "/Fixtures/PB_FamilyBoard.prefab";
        private const string UpgradeBench = Prefabs + "/Fixtures/PB_UpgradeBench.prefab";
        private const string CassettePlayer = Prefabs + "/Fixtures/PB_CassettePlayer.prefab";
        private readonly Color _cork = new(0.55f, 0.4f, 0.25f);
        private readonly Color _paper = new(0.9f, 0.88f, 0.8f);

        private void BuildStations()
        {
            if (ShouldWrite(MissionBoard))
            {
                GameObject board = Node("PB_MissionBoard", null, Vector2.zero);
                board.AddComponent<StationView>().Kind = StationKind.MissionBoard;
                Part("Frame", board.transform, new Vector2(0f, 1.75f), new Vector2(1.9f, 1.3f), _wood, -20);
                Part("Cork", board.transform, new Vector2(0f, 1.75f), new Vector2(1.72f, 1.12f), _cork, -19);
                var random = new System.Random(11);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 at = new(-0.6f + i * 0.3f, 1.62f + (float) random.NextDouble() * 0.35f);
                    Part("Note " + i, board.transform, at, new Vector2(0.26f, 0.32f), _paper, -18, (float) random.NextDouble() * 16f - 8f);
                    Part("Pin " + i, board.transform, at + new Vector2(0f, 0.13f), new Vector2(0.05f, 0.05f), _red, -17);
                }
                Part("Photo", board.transform, new Vector2(0.55f, 1.95f), new Vector2(0.34f, 0.28f), new Color(0.3f, 0.4f, 0.45f), -18, 6f);
                Line("String", board.transform, new Vector2(-0.6f, 1.9f), new Vector2(0.55f, 2f), 0.02f, _red, -17);
                SavePrefab<GameObject>(board, MissionBoard);
            }
            if (ShouldWrite(FamilyBoard))
            {
                GameObject board = Node("PB_FamilyBoard", null, Vector2.zero);
                board.AddComponent<StationView>().Kind = StationKind.FamilyBoard;
                Part("Frame", board.transform, new Vector2(0f, 1.72f), new Vector2(1.65f, 1.35f), _wood, -20);
                Part("Felt", board.transform, new Vector2(0f, 1.72f), new Vector2(1.48f, 1.18f), new Color(0.12f, 0.16f, 0.18f), -19);
                CharacterShape[] shapes = {CharacterShape.Square, CharacterShape.Circle, CharacterShape.Triangle, CharacterShape.Pentagon, CharacterShape.Hexagon, CharacterShape.Diamond};
                for (int i = 0; i < shapes.Length; i++)
                {
                    float x = -0.48f + i % 3 * 0.48f;
                    float y = 1.95f - i / 3 * 0.48f;
                    SpriteRenderer portrait = Part(shapes[i].ToString(), board.transform, new Vector2(x, y), new Vector2(0.25f, 0.25f), i < 2 ? _teal : _red, -18);
                    portrait.sprite = _shapes[shapes[i]];
                    Part("Pin " + i, board.transform, new Vector2(x, y + 0.17f), new Vector2(0.04f, 0.04f), _paper, -17).sprite = _shapes[CharacterShape.Circle];
                }
                SavePrefab<GameObject>(board, FamilyBoard);
            }
            if (ShouldWrite(UpgradeBench))
            {
                GameObject bench = Node("PB_UpgradeBench", null, Vector2.zero);
                bench.AddComponent<StationView>().Kind = StationKind.UpgradeBench;
                Part("Top", bench.transform, new Vector2(0f, 0.95f), new Vector2(2.1f, 0.14f), _wood, -20);
                Part("Leg L", bench.transform, new Vector2(-0.9f, 0.45f), new Vector2(0.12f, 0.9f), _wood, -20);
                Part("Leg R", bench.transform, new Vector2(0.9f, 0.45f), new Vector2(0.12f, 0.9f), _wood, -20);
                Part("Shelf", bench.transform, new Vector2(0f, 0.35f), new Vector2(1.8f, 0.08f), _wood, -20);
                Part("Pegboard", bench.transform, new Vector2(0f, 2f), new Vector2(2f, 1.2f), new Color(0.3f, 0.25f, 0.2f), -21);
                Part("Wrench", bench.transform, new Vector2(-0.6f, 2.05f), new Vector2(0.08f, 0.6f), _steel, -19, 20f);
                Part("Saw", bench.transform, new Vector2(0.1f, 2.15f), new Vector2(0.7f, 0.18f), _steel, -19);
                Part("Hammer", bench.transform, new Vector2(0.65f, 1.95f), new Vector2(0.06f, 0.5f), _wood, -19, -15f);
                Part("Vice", bench.transform, new Vector2(0.7f, 1.1f), new Vector2(0.3f, 0.2f), _ink, -19);
                Part("Parts", bench.transform, new Vector2(-0.4f, 1.08f), new Vector2(0.5f, 0.12f), _amber, -19);
                Part("Lamp", bench.transform, new Vector2(-0.85f, 1.3f), new Vector2(0.18f, 0.12f), _amber, -18);
                SavePrefab<GameObject>(bench, UpgradeBench);
            }
        }

        // A boombox on a side table: two speakers, a tape window whose reels turn, a red light while it plays.
        private void BuildCassettePlayer()
        {
            if (!ShouldWrite(CassettePlayer)) return;
            GameObject deck = Node("PB_CassettePlayer", null, Vector2.zero);
            deck.AddComponent<StationView>().Kind = StationKind.CassettePlayer;
            var view = deck.AddComponent<TapeDeckView>();
            Sprite round = _shapes[CharacterShape.Circle];
            Part("Table", deck.transform, new Vector2(0f, 0.72f), new Vector2(1.1f, 0.08f), _wood, -20);
            Part("Leg L", deck.transform, new Vector2(-0.45f, 0.36f), new Vector2(0.06f, 0.72f), _wood, -20);
            Part("Leg R", deck.transform, new Vector2(0.45f, 0.36f), new Vector2(0.06f, 0.72f), _wood, -20);
            Part("Body", deck.transform, new Vector2(0f, 1.02f), new Vector2(1f, 0.52f), new Color(0.16f, 0.15f, 0.15f), -19);
            Part("Handle", deck.transform, new Vector2(0f, 1.34f), new Vector2(0.7f, 0.05f), _steel, -19);
            Part("Handle L", deck.transform, new Vector2(-0.33f, 1.3f), new Vector2(0.04f, 0.1f), _steel, -19);
            Part("Handle R", deck.transform, new Vector2(0.33f, 1.3f), new Vector2(0.04f, 0.1f), _steel, -19);
            foreach (float x in new[] {-0.33f, 0.33f})
            {
                Part("Speaker", deck.transform, new Vector2(x, 0.99f), new Vector2(0.3f, 0.3f), _ink, -18).sprite = round;
                Part("Cone", deck.transform, new Vector2(x, 0.99f), new Vector2(0.14f, 0.14f), new Color(0.25f, 0.24f, 0.23f), -17).sprite = round;
            }
            Part("Tape window", deck.transform, new Vector2(0f, 1.04f), new Vector2(0.3f, 0.2f), new Color(0.05f, 0.05f, 0.06f), -18);
            Part("Tape", deck.transform, new Vector2(0f, 1.04f), new Vector2(0.26f, 0.15f), _hatBrass, -17);
            var reels = new System.Collections.Generic.List<Object>();
            foreach (float x in new[] {-0.07f, 0.07f})
            {
                Transform reel = Node("Reel", deck.transform, new Vector2(x, 1.04f)).transform;
                Part("Hub", reel, Vector2.zero, new Vector2(0.07f, 0.07f), _ink, -16).sprite = round;
                Part("Spoke", reel, Vector2.zero, new Vector2(0.07f, 0.015f), _hatBrass, -15);
                reels.Add(reel);
            }
            Part("Buttons", deck.transform, new Vector2(0f, 0.81f), new Vector2(0.4f, 0.05f), _steel, -18);
            GameObject light = Node("Playing light", deck.transform, new Vector2(0.2f, 1.22f));
            Part("Light", light.transform, Vector2.zero, new Vector2(0.05f, 0.05f), _crimson, -16).sprite = round;
            Part("Glow", light.transform, Vector2.zero, new Vector2(0.3f, 0.3f), new Color(1f, 0.2f, 0.2f, 0.3f), -17).sprite = _soft;
            light.SetActive(false);
            AssignArray(view, "_reels", reels);
            Assign(view, "_playingLight", light);
            SavePrefab<GameObject>(deck, CassettePlayer);
        }

        public bool HideoutNeedsDeck()
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(HideoutPrefab);
            return level != null && level.GetComponentsInChildren<TapeDeckView>(true).Length == 0;
        }

        public bool HideoutNeedsFamilyBoard()
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(HideoutPrefab);
            if (level == null) return false;
            foreach (StationView station in level.GetComponentsInChildren<StationView>(true))
                if (station.Kind == StationKind.FamilyBoard) return false;
            return true;
        }

        private void FamilyBoardForHideout()
        {
            if (!HideoutNeedsFamilyBoard() || !Exists(FamilyBoard)) return;
            GameObject contents = PrefabUtility.LoadPrefabContents(HideoutPrefab);
            try
            {
                Transform fixtures = contents.transform.Find("Fixtures") ?? contents.transform;
                Place(FamilyBoard, fixtures, new Vector2(-4.55f, 0f));
                PrefabUtility.SaveAsPrefabAsset(contents, HideoutPrefab);
                _report.Add("Put the family board in the hideout");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        // The hideout gets its tape deck added in place; nothing else in the prefab is touched.
        private void DeckForHideout()
        {
            if (!HideoutNeedsDeck() || !Exists(CassettePlayer)) return;
            GameObject contents = PrefabUtility.LoadPrefabContents(HideoutPrefab);
            try
            {
                Transform fixtures = contents.transform.Find("Fixtures") ?? contents.transform;
                Place(CassettePlayer, fixtures, new Vector2(1.2f, 0f));
                PrefabUtility.SaveAsPrefabAsset(contents, HideoutPrefab);
                _report.Add("Put a cassette player in the hideout");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private void BuildHideout()
        {
            if (!ShouldWrite(HideoutPrefab)) return;
            GameObject root = Node("PB_Level_Hideout", null, Vector2.zero);
            var level = root.AddComponent<LevelAuthoring>();
            level.FloorHeight = 4.2f;
            level.HalfWidth = 8f;

            Transform art = Node("Art", root.transform, Vector2.zero).transform;
            Rect("Outside", art, 0f, 3f, 30f, 12f, new Color(0.02f, 0.03f, 0.06f), -100);
            Rect("Shell", art, 0f, 2.1f, 16.6f, 4.8f, _ink, -50);
            Rect("Wallpaper", art, 0f, 2.15f, 16f, 4.1f, new Color(0.16f, 0.13f, 0.12f), -49);
            for (int i = 0; i < 16; i++) Rect("Stripe", art, -7.5f + i, 2.15f, 0.06f, 4.1f, new Color(0.2f, 0.16f, 0.14f), -48);
            Rect("Floor", art, 0f, -0.2f, 16.6f, 0.4f, new Color(0.22f, 0.15f, 0.1f), -12);
            for (int i = 0; i < 20; i++) Rect("Board", art, -7.8f + i * 0.82f, -0.05f, 0.03f, 0.1f, new Color(0.15f, 0.1f, 0.07f), -11);
            Rect("Ceiling", art, 0f, 4.1f, 16.6f, 0.3f, _ink, -12);
            Rect("Skirting", art, 0f, 0.12f, 16f, 0.08f, new Color(0.3f, 0.22f, 0.15f), -26);
            // window with the city in the rain
            Rect("Window frame", art, 1.5f, 2.4f, 2.4f, 1.7f, _ink, -44);
            Rect("Night", art, 1.5f, 2.4f, 2.2f, 1.5f, new Color(0.05f, 0.1f, 0.18f), -43);
            for (int i = 0; i < 6; i++) Rect("Tower", art, 0.7f + i * 0.32f, 1.9f + (i % 3) * 0.12f, 0.24f, 0.6f + (i % 3) * 0.25f, new Color(0.08f, 0.14f, 0.2f), -42);
            Rect("Neon", art, 1.1f, 2.9f, 0.5f, 0.06f, _teal, -41);
            Rect("Mullion", art, 1.5f, 2.4f, 0.06f, 1.5f, _ink, -40);
            // bed, couch, lamp, posters
            Rect("Bed frame", art, -5.8f, 0.35f, 2.6f, 0.5f, _wood, -20);
            Rect("Mattress", art, -5.8f, 0.68f, 2.4f, 0.22f, new Color(0.75f, 0.73f, 0.68f), -19);
            Rect("Blanket", art, -5.4f, 0.72f, 1.6f, 0.2f, new Color(0.25f, 0.4f, 0.45f), -18);
            Rect("Pillow", art, -6.75f, 0.84f, 0.5f, 0.18f, _paper, -18);
            Rect("Couch", art, 5.8f, 0.45f, 2.2f, 0.6f, new Color(0.45f, 0.18f, 0.2f), -20);
            Rect("Couch back", art, 5.8f, 0.95f, 2.2f, 0.45f, new Color(0.38f, 0.15f, 0.17f), -21);
            Rect("Lamp pole", art, 7.2f, 1f, 0.06f, 2f, _steel, -22);
            Rect("Lamp shade", art, 7.2f, 2.05f, 0.5f, 0.35f, _amber, -21);
            Rect("Poster", art, -5.8f, 2.6f, 1f, 1.3f, new Color(0.6f, 0.2f, 0.25f), -40);
            Rect("Poster shape", art, -5.8f, 2.6f, 0.45f, 0.45f, _paper, -39);
            Rect("Rug", art, 0f, 0.03f, 3.5f, 0.06f, new Color(0.45f, 0.25f, 0.2f), -10);

            Transform fixtures = Node("Fixtures", root.transform, Vector2.zero).transform;
            Place(MissionBoard, fixtures, new Vector2(-2.3f, 0f));
            Place(FamilyBoard, fixtures, new Vector2(-4.55f, 0f));
            Place(UpgradeBench, fixtures, new Vector2(4.2f, 0f));
            Place(CassettePlayer, fixtures, new Vector2(1.2f, 0f));
            Place(Cover, fixtures, new Vector2(-1f, 0f)).name = "Practice crate";

            Transform spawns = Node("Spawns", root.transform, Vector2.zero).transform;
            Node("Player Spawn", spawns, new Vector2(-4f, 0f)).AddComponent<PlayerSpawn>();

            GameObject cameraObject = Node("Camera", root.transform, Vector2.zero);
            cameraObject.transform.localPosition = new Vector3(0f, 2.1f, -20f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.03f, 0.06f);
            camera.depth = 1;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            level.Camera = camera;
            GameObject crosshair = Node("Crosshair", root.transform, Vector2.zero);
            Color sight = new(0.80f, 0.96f, 0.92f, 0.82f);
            Part("L", crosshair.transform, new Vector2(-0.16f, 0f), new Vector2(0.12f, 0.025f), sight, 50);
            Part("R", crosshair.transform, new Vector2(0.16f, 0f), new Vector2(0.12f, 0.025f), sight, 50);
            Part("D", crosshair.transform, new Vector2(0f, -0.16f), new Vector2(0.025f, 0.12f), sight, 50);
            Part("U", crosshair.transform, new Vector2(0f, 0.16f), new Vector2(0.025f, 0.12f), sight, 50);
            level.Crosshair = crosshair.transform;
            level.Dynamic = Node("Dynamic", root.transform, Vector2.zero).transform;
            SavePrefab<GameObject>(root, HideoutPrefab);
        }

        private void BuildCampaign()
        {
            var missions = AssetDatabase.LoadAssetAtPath<CD_Missions>(MissionsAsset);
            if (missions == null)
            {
                missions = ScriptableObject.CreateInstance<CD_Missions>();
                missions.Missions.Add(new MissionCVO
                {
                    Id = "night_shift", Title = "NIGHT SHIFT  /  COLD STORAGE 07", Reward = 500, WorkPayment = 50,
                    OwnerFamily = "rival", LocationDifficulty = LocationDifficulty.Easy, AttackChance = 10f, AttackingFamily = "rival",
                    Briefing = "Six shapes guard a cold storage unit. Clear the building, leave by the green exit.",
                });
                AssetDatabase.CreateAsset(missions, MissionsAsset);
                _report.Add("Wrote " + MissionsAsset);
            }
            if (missions.Hideout == null) missions.Hideout = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(HideoutPrefab);
            foreach (MissionCVO mission in missions.Missions)
                if (mission.Level == null && mission.Id == "night_shift") mission.Level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(LevelPrefab);
            EditorUtility.SetDirty(missions);

            if (AssetDatabase.LoadAssetAtPath<CD_Upgrades>(UpgradesAsset) == null)
            {
                var upgrades = ScriptableObject.CreateInstance<CD_Upgrades>();
                upgrades.FillDefaults();
                AssetDatabase.CreateAsset(upgrades, UpgradesAsset);
                _report.Add("Wrote " + UpgradesAsset);
            }
        }
    }
}
#endif
