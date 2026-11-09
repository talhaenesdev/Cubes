#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Modules.GameplayModule.Entities;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // Dresses every contract level with furniture that suits the place - stools in the diner, slot machines in the
    // casino. It is scenery only: no collider, no view, drawn behind the fixtures, and kept clear of every door, crate,
    // stair and prop so nothing the player uses is hidden. The same place gets the same room every run.
    internal static class LevelDressingSetup
    {
        private const string ArtFolder = "Assets/Modules/GameplayModule/Art/Incoming/Props";
        private const string LevelFolder = "Assets/Modules/GameplayModule/Prefabs/Level";
        private const string Node = "Dressing";
        private const int Order = -24;
        private const float Gap = 0.15f;
        private const float FixtureHalf = 0.5f;
        // A door, a stairwell or the exit keeps a clear stretch of wall either side, so nothing is drawn behind it.
        private const float DoorClear = 1.6f;

        private static readonly Dictionary<string, string[]> Themes = new()
        {
            ["NightShift"] = new[] {"FilingCabinet", "Chair", "Coatstand", "Shelf", "Safe"},
            ["Diner"] = new[] {"Counter", "Stool", "Stool", "Table_Round", "Chair", "Coatstand"},
            ["Laundromat"] = new[] {"WashingMachine", "WashingMachine", "Chair", "Shelf"},
            ["PawnShop"] = new[] {"Shelf", "Safe", "Clock_Grandfather", "Piano", "FloorLamp"},
            ["Gym"] = new[] {"PunchingBag", "PunchingBag", "Shelf", "Chair"},
            ["Cinema"] = new[] {"Sofa", "FloorLamp", "Coatstand", "Counter"},
            ["Distillery"] = new[] {"Still", "Still", "Shelf"},
            ["Docks"] = new[] {"Shelf", "Safe", "FilingCabinet"},
            ["Hotel"] = new[] {"Sofa", "Table_Round", "Chair", "FloorLamp", "Piano", "Clock_Grandfather"},
            ["Casino"] = new[] {"SlotMachine", "SlotMachine", "RouletteTable", "Table_Round", "Chair"},
            ["Precinct"] = new[] {"FilingCabinet", "FilingCabinet", "Chair", "Safe", "Coatstand"},
            ["Penthouse"] = new[] {"Sofa", "Piano", "FloorLamp", "Clock_Grandfather", "Table_Round", "Safe"},
        };

        // Things hung from the ceiling rather than stood on the floor.
        private static readonly HashSet<string> Hanging = new() {"PunchingBag"};

        [MenuItem("Tools/FlowIoC-Modules/Gameplay/Dress Levels With Furniture")]
        public static void Apply() => Debug.Log(ApplyAll());

        public static string ApplyAll()
        {
            var report = new StringBuilder("Level dressing:\n");
            foreach (KeyValuePair<string, string[]> theme in Themes)
                report.AppendLine($"  {theme.Key}: {Dress(theme.Key, theme.Value)}");
            AssetDatabase.SaveAssets();
            return report.ToString();
        }

        private static string Dress(string place, string[] theme)
        {
            string path = $"{LevelFolder}/PB_Level_{place}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return "no prefab";
            var sprites = new List<(string name, Sprite sprite)>();
            foreach (string name in theme)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/PROP_{name}.png");
                if (sprite != null) sprites.Add((name, sprite));
            }
            if (sprites.Count == 0) return "no art";

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            int placed = 0;
            try
            {
                var level = root.GetComponent<LevelAuthoring>();
                float half = level != null ? level.HalfWidth : 14f;
                float storey = level != null ? level.FloorHeight : 4.2f;
                Transform art = root.transform.Find("Art") ?? root.transform;
                Transform old = art.Find(Node);
                if (old != null) Object.DestroyImmediate(old.gameObject);
                Transform node = new GameObject(Node).transform;
                node.SetParent(art, false);

                // What stands on each floor: every fixture, as a band the furniture keeps out of.
                var taken = new Dictionary<int, List<(float a, float b)>>();
                int floors = 1;
                Transform fixtures = root.transform.Find("Fixtures");
                if (fixtures != null)
                    foreach (Transform t in fixtures)
                    {
                        // A lamp hangs and a switch sits on the wall: neither takes floor space.
                        if (t.name.Contains("CeilingLamp") || t.name.Contains("LightSwitch")) continue;
                        int floor = Mathf.RoundToInt(t.localPosition.y / storey);
                        floors = Mathf.Max(floors, floor + 1);
                        if (!taken.TryGetValue(floor, out var list)) taken[floor] = list = new List<(float, float)>();
                        bool door = t.name.Contains("Door") || t.name.Contains("Exit");
                        float clear = door ? DoorClear : FixtureHalf;
                        list.Add((t.localPosition.x - clear, t.localPosition.x + clear));
                    }

                var random = new System.Random(Seed(place));
                for (int floor = 0; floor < floors; floor++)
                {
                    taken.TryGetValue(floor, out var list);
                    list ??= new List<(float, float)>();
                    float x = -half + 1.2f;
                    int pick = random.Next(sprites.Count);
                    while (x < half - 1.2f)
                    {
                        (string name, Sprite sprite) = sprites[pick % sprites.Count];
                        float width = sprite.bounds.size.x;
                        float a = x, b = x + width;
                        if (b > half - 0.8f) break;
                        if (Free(list, a - Gap, b + Gap) && random.NextDouble() < 0.85)
                        {
                            Put(node, name, sprite, new Vector2(x + width * 0.5f, floor * storey), storey);
                            list.Add((a, b));
                            placed++;
                            pick++;
                            x = b + Gap + (float) random.NextDouble() * 1.5f;
                        }
                        else x += 0.5f;
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return placed + " pieces";
        }

        private static int Seed(string text)
        {
            int seed = 17;
            foreach (char c in text) seed = seed * 31 + c;
            return seed & 0x7fffffff;
        }

        private static bool Free(List<(float a, float b)> taken, float a, float b)
        {
            foreach ((float ta, float tb) in taken)
                if (a < tb && b > ta) return false;
            return true;
        }

        private static void Put(Transform parent, string name, Sprite sprite, Vector2 foot, float storey)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            // A hanging thing's top touches the ceiling, under the cornice.
            if (Hanging.Contains(name)) foot.y += storey - 0.35f - sprite.bounds.size.y;
            go.transform.localPosition = foot;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = Order;
        }
    }
}
#endif
