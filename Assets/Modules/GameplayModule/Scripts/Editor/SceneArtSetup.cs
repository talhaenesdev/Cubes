#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Modules.GameplayModule.Entities;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // Lays the AI-drawn place art into every level prefab: the far view behind the building, the back wall on
    // every storey and the floor strip on every slab. The flat shapes they replace are switched off, not deleted,
    // so a level whose art is missing keeps its old look and running this again changes nothing twice.
    internal static class SceneArtSetup
    {
        private const string ArtFolder = "Assets/Modules/GameplayModule/Art/Incoming/Scenes";
        private const string LevelFolder = "Assets/Modules/GameplayModule/Prefabs/Level";
        private const string Node = "Scene Art";

        private const float StoreyHeight = 4.2f;
        private const float FarHeight = 26f;
        private const float StripHeight = 0.7f;

        // The drawings are busy, so they sit back: the far view nearly black, the wall dim, so characters, doors
        // and cover read in front of them.
        private static readonly Color FarTint = new(0.4f, 0.4f, 0.44f);
        private static readonly Color WallTint = new(0.72f, 0.72f, 0.74f);
        private static readonly Color FloorTint = new(0.75f, 0.75f, 0.77f);

        // The flat shapes the drawn wall stands in for.
        private static readonly HashSet<string> Replaced = new()
        {
            "Interior", "Panel", "Stripe", "Wainscot", "Frame", "Painting", "Window", "Window glow", "Shelf", "Stock",
            "Skirting", "Cornice", "Shell",
            // The hideout draws its room straight under Art.
            "Outside", "Wallpaper", "Window frame", "Night", "Tower", "Neon", "Mullion", "Board", "Ceiling"
        };

        private static readonly string[] Places =
        {
            "Hideout", "NightShift", "Diner", "Laundromat", "PawnShop", "Gym", "Cinema", "Distillery", "Docks", "Hotel",
            "Casino", "Precinct", "Penthouse"
        };

        [MenuItem("Tools/FlowIoC-Modules/Gameplay/Apply Scene Art")]
        public static void Apply() => Debug.Log(ApplyAll());

        public static string ApplyAll()
        {
            var report = new StringBuilder("Scene art:\n");
            foreach (string place in Places)
            {
                Import(Path(place, "Far"), 1080f / FarHeight);
                Import(Path(place, "Wall"), 1080f / StoreyHeight);
                Import(Path(place, "Floor"), 128f / StripHeight);
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (string place in Places) report.AppendLine("  " + place + ": " + ApplyPlace(place));
            AssetDatabase.SaveAssets();
            return report.ToString();
        }

        private static string Path(string place, string layer) => $"{ArtFolder}/BG_{place}_{layer}.png";

        private static void Import(string path, float pixelsPerUnit)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int) SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static string ApplyPlace(string place)
        {
            string prefabPath = $"{LevelFolder}/PB_Level_{place}.prefab";
            var far = AssetDatabase.LoadAssetAtPath<Sprite>(Path(place, "Far"));
            var wall = AssetDatabase.LoadAssetAtPath<Sprite>(Path(place, "Wall"));
            var floor = AssetDatabase.LoadAssetAtPath<Sprite>(Path(place, "Floor"));
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null) return "no prefab";
            if (far == null || wall == null || floor == null) return "art missing, left as it was";

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var level = root.GetComponent<LevelAuthoring>();
                Transform art = root.transform.Find("Art") ?? root.transform;
                float half = level != null ? level.HalfWidth : 14f;
                float storey = level != null ? level.FloorHeight : StoreyHeight;
                int floors = Floors(root.transform, storey);

                Transform old = art.Find(Node);
                if (old != null) Object.DestroyImmediate(old.gameObject);
                Transform node = new GameObject(Node).transform;
                node.SetParent(art, false);

                Transform backdrop = art.Find("Backdrop");
                if (backdrop != null) backdrop.gameObject.SetActive(false);
                int hidden = 0;
                Transform building = art.Find("Building");
                if (building != null)
                    foreach (Transform t in building.GetComponentsInChildren<Transform>(true))
                        if (Replaced.Contains(t.name) && t.gameObject.activeSelf) { t.gameObject.SetActive(false); hidden++; }
                foreach (Transform t in art)
                    if ((Replaced.Contains(t.name) || t.name == "Floor") && t.gameObject.activeSelf) { t.gameObject.SetActive(false); hidden++; }

                float top = floors * storey;
                float width = half * 2f;
                Layer("Far", node, far, new Vector2(0f, top * 0.5f + 1f), new Vector2(width + 70f, FarHeight), -120, FarTint);
                for (int f = 0; f < floors; f++)
                {
                    Layer("Wall " + f, node, wall, new Vector2(0f, f * storey + storey * 0.5f), new Vector2(width, storey), -48, WallTint);
                    Slab("Floor " + f, node, f * storey, width + 0.5f);
                }
                Slab("Roof", node, top + StripHeight, width + 0.5f);

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                return $"{floors} floor(s), {hidden} flat shapes switched off";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // Every marker stands on a floor, so the highest one says how many storeys the building has.
        private static int Floors(Transform root, float storey)
        {
            float highest = 0f;
            Transform fixtures = root.Find("Fixtures");
            if (fixtures != null)
                foreach (Transform t in fixtures)
                    highest = Mathf.Max(highest, t.localPosition.y);
            return Mathf.Max(1, Mathf.RoundToInt(highest / storey) + 1);
        }

        // The drawn floor strips came out as busy little scenes, so a slab is a plain dark band with one thin trim
        // line on its top edge, the line the characters stand on.
        private const string PixelSprite = "Assets/Modules/GameplayModule/Art/Textures/SPR_Pixel.png";
        private static readonly Color SlabColor = new(0.045f, 0.035f, 0.03f);
        private static readonly Color TrimColor = new(0.42f, 0.34f, 0.2f);

        private static void Slab(string name, Transform parent, float top, float width)
        {
            var pixel = AssetDatabase.LoadAssetAtPath<Sprite>(PixelSprite);
            var slab = new GameObject(name).transform;
            slab.SetParent(parent, false);
            Block("Band", slab, pixel, new Vector2(0f, top - StripHeight * 0.5f), new Vector2(width, StripHeight), -11, SlabColor);
            Block("Trim", slab, pixel, new Vector2(0f, top - 0.03f), new Vector2(width, 0.06f), -10, TrimColor);
        }

        private static void Block(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, int order, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            Vector2 native = sprite.bounds.size;
            go.transform.localScale = new Vector3(size.x / native.x, size.y / native.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
        }

        private static void Layer(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, int order, Color tint)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            // The far view keeps its proportions and repeats sideways; wall and floor fill their band exactly.
            renderer.size = size;
            renderer.sortingOrder = order;
            renderer.color = tint;
        }
    }
}
#endif
