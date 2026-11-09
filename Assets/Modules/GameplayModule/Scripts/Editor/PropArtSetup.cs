#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // Puts the AI-drawn object sprites on the fixture prefabs and the hideout's furniture. The flat shapes they replace
    // are switched off, not deleted; each drawing sits where the shapes it replaces stood, feet on the same floor line.
    // Running it again rebuilds the drawings rather than stacking a second set.
    internal static class PropArtSetup
    {
        private const string ArtFolder = "Assets/Modules/GameplayModule/Art/Incoming/Props";
        private const string FixtureFolder = "Assets/Modules/GameplayModule/Prefabs/Fixtures";
        private const string HideoutLevel = "Assets/Modules/GameplayModule/Prefabs/Level/PB_Level_Hideout.prefab";
        private const string Drawn = "Drawn";
        private const float PixelsPerUnit = 64f;

        // Shown over the drawing: what the player reads to play - signs, prompts, the shape grid on a door.
        private static readonly HashSet<string> Keep = new()
        {
            "Icon", "Guide", "Sign EXIT", "Sign STAIRS", "Shape Grid", "Playing light", "Darkness", "Toggle On", "Toggle Off",
            "Shadow", Drawn
        };

        private enum Anchor { Floor = 0, Ceiling = 1 }

        [MenuItem("Tools/FlowIoC-Modules/Gameplay/Apply Prop Art")]
        public static void Apply() => Debug.Log(ApplyAll());

        public static string ApplyAll()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] {ArtFolder})) Import(AssetDatabase.GUIDToAssetPath(guid));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var report = new StringBuilder("Prop art:\n");
            Fixture(report, "PB_Door", root =>
            {
                Bounds frame = Hide(root, root.Find("Frame"));
                Swap(root, root.Find("Closed"), "Door_Closed", frame, Anchor.Floor);
                Swap(root, root.Find("Open"), "Door_Open", frame, Anchor.Floor);
            });
            Whole(report, "PB_StairDoor", "StairDoor");
            Whole(report, "PB_Exit", "Exit");
            Whole(report, "PB_Crate", "Crate");
            Whole(report, "PB_Vent", "Vent");
            Whole(report, "PB_Wardrobe", "Wardrobe");
            Whole(report, "PB_WeaponLocker", "WeaponLocker");
            Whole(report, "PB_UpgradeBench", "UpgradeBench");
            Whole(report, "PB_MissionBoard", "MissionBoard");
            Whole(report, "PB_FamilyBoard", "FamilyBoard");
            Whole(report, "PB_Cat", "Cat");
            Whole(report, "PB_Document", "Document");
            OnTable(report, "PB_Computer", "Desk", "Computer", null);
            OnTable(report, "PB_CassettePlayer", "CassetteTable", "CassettePlayer", null);
            Breakable(report, "PB_Prop_Barrel", "Barrel", null, Anchor.Floor);
            Breakable(report, "PB_Prop_Plant", "Plant", null, Anchor.Floor);
            Breakable(report, "PB_Prop_CeilingLamp", "CeilingLamp", null, Anchor.Ceiling);
            Breakable(report, "PB_Prop_Monitor", "Monitor", "Desk", Anchor.Floor);
            Breakable(report, "PB_Prop_Bottles", "Bottles", "Table_Round", Anchor.Floor);
            report.AppendLine("  PB_Level_Hideout: " + Hideout());
            AssetDatabase.SaveAssets();
            return report.ToString();
        }

        private static void Import(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int) SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/PROP_{name}.png");

        // ------------------------------------------------------------------ prefab shapes

        private static void Whole(StringBuilder report, string prefab, string art) =>
            Fixture(report, prefab, root => Swap(root, root, art, null, Anchor.Floor));

        private static void OnTable(StringBuilder report, string prefab, string table, string top, string _) =>
            Fixture(report, prefab, root =>
            {
                Bounds all = Hide(root, root);
                Vector2 foot = Foot(root, all, Anchor.Floor);
                Sprite under = Art(table);
                Put(root, under, foot, Order(root, -21));
                Put(root, Art(top), foot + new Vector2(0f, under != null ? under.bounds.size.y - 0.06f : 0f), Order(root, -20));
            });

        // A breakable prop keeps its two states under Body; whatever stands beside them (a desk, a table) is drawn once.
        private static void Breakable(StringBuilder report, string prefab, string art, string table, Anchor anchor) =>
            Fixture(report, prefab, root =>
            {
                Transform body = root.Find("Body");
                Transform intact = body != null ? body.Find("Intact") : null;
                Transform broken = body != null ? body.Find("Broken") : null;
                Bounds all = Bounds(root, root);
                float lift = 0f;
                if (table != null)
                {
                    foreach (Transform t in root)
                        if (t != body && !Keep.Contains(t.name)) t.gameObject.SetActive(false);
                    Sprite under = Art(table);
                    Put(root, under, Foot(root, all, Anchor.Floor), -21);
                    lift = under != null ? under.bounds.size.y - 0.06f : 0f;
                }
                if (body != null)
                    foreach (Transform t in body)
                        if (t != intact && t != broken) t.gameObject.SetActive(false);
                Vector2 foot = Foot(root, all, anchor) + new Vector2(0f, lift);
                Swap(root, intact, art, null, anchor, foot);
                Swap(root, broken, Art(art + "_Broken") != null ? art + "_Broken" : art + "_Fallen", null, anchor, foot);
            });

        // ------------------------------------------------------------------ hideout furniture

        private static readonly (string art, string[] parts)[] HideoutFurniture =
        {
            ("Bed", new[] {"Bed frame", "Mattress", "Blanket", "Pillow"}),
            ("Couch", new[] {"Couch", "Couch back"}),
            ("FloorLamp", new[] {"Lamp pole", "Lamp shade"}),
            ("Rug", new[] {"Rug"}),
            ("Plant", new[] {"Plant pot", "Palm leaf"}),
            ("Whiskey", new[] {"Bottle", "Glass"}),
        };

        private static string Hideout()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(HideoutLevel) == null) return "no prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(HideoutLevel);
            int placed = 0;
            try
            {
                Transform art = root.transform.Find("Art");
                Transform home = root.transform.Find("Home");
                Transform old = art.Find(Drawn);
                if (old != null) Object.DestroyImmediate(old.gameObject);
                var parts = new List<Transform>();
                foreach (Transform t in art) parts.Add(t);
                if (home != null) foreach (Transform t in home) parts.Add(t);

                foreach ((string name, string[] names) in HideoutFurniture)
                {
                    Sprite sprite = Art(name);
                    if (sprite == null) continue;
                    // Each run of matching shapes is one piece of furniture; a name can repeat across the room.
                    var group = new HashSet<string>(names);
                    var found = new List<Transform>();
                    foreach (Transform t in parts)
                        if (group.Contains(t.name)) found.Add(t);
                    if (found.Count == 0) continue;
                    foreach (List<Transform> piece in Pieces(found))
                    {
                        Bounds b = default;
                        bool any = false;
                        int order = 0;
                        foreach (Transform t in piece)
                        {
                            var r = t.GetComponent<SpriteRenderer>();
                            if (r != null) { if (!any) { b = r.bounds; order = r.sortingOrder; any = true; } else { b.Encapsulate(r.bounds); order = Mathf.Min(order, r.sortingOrder); } }
                            t.gameObject.SetActive(false);
                        }
                        if (!any) continue;
                        // Furniture loose in the room can stand on something else - the whiskey on a table.
                        Put(art, sprite, (Vector2) art.InverseTransformPoint(new Vector3(b.center.x, Mathf.Max(0f, b.min.y))), order, Drawn);
                        placed++;
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(root, HideoutLevel);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return placed + " pieces of furniture drawn";
        }

        // Shapes of one kind more than 1.5 units apart are separate pieces.
        private static IEnumerable<List<Transform>> Pieces(List<Transform> found)
        {
            found.Sort((a, b) => a.position.x.CompareTo(b.position.x));
            var piece = new List<Transform>();
            foreach (Transform t in found)
            {
                if (piece.Count > 0 && t.position.x - piece[piece.Count - 1].position.x > 1.5f) { yield return piece; piece = new List<Transform>(); }
                piece.Add(t);
            }
            if (piece.Count > 0) yield return piece;
        }

        // ------------------------------------------------------------------ helpers

        private static void Fixture(StringBuilder report, string prefab, System.Action<Transform> skin)
        {
            string path = $"{FixtureFolder}/{prefab}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) { report.AppendLine($"  {prefab}: no prefab"); return; }
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    if (t != null && t.name == Drawn) Object.DestroyImmediate(t.gameObject);
                skin(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                report.AppendLine($"  {prefab}: drawn");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // Switches off the shapes under group and stands the drawing where they stood, inside the same group so the
        // view that shows and hides the group shows and hides the drawing with it.
        private static void Swap(Transform root, Transform group, string art, Bounds? place, Anchor anchor, Vector2? at = null)
        {
            if (group == null) return;
            Sprite sprite = Art(art);
            if (sprite == null) return;
            int order = Order(group, -20);
            Bounds hidden = Hide(root, group);
            Vector2 foot = at ?? Foot(root, place ?? hidden, anchor);
            // A hanging thing is anchored by its top; the sprite's pivot is its foot.
            if (anchor == Anchor.Ceiling) foot.y -= sprite.bounds.size.y;
            Put(group, sprite, (Vector2) group.InverseTransformPoint(root.TransformPoint(foot)), order);
        }

        // Switches off every shape directly under group except the ones the player reads, and returns where they stood
        // in the root's space.
        private static Bounds Hide(Transform root, Transform group)
        {
            if (group == null) return default;
            Bounds b = Bounds(root, group);
            if (group != root && group.GetComponent<SpriteRenderer>() != null && group.parent == root)
            {
                group.gameObject.SetActive(false);
                return b;
            }
            foreach (Transform t in group)
                if (!Keep.Contains(t.name)) t.gameObject.SetActive(false);
            return b;
        }

        private static Bounds Bounds(Transform root, Transform group)
        {
            Bounds b = default;
            bool any = false;
            foreach (SpriteRenderer r in group.GetComponentsInChildren<SpriteRenderer>(false))
            {
                if (Keep.Contains(r.name) || IsKept(r.transform, group)) continue;
                Bounds local = new(root.InverseTransformPoint(r.bounds.center), r.bounds.size);
                if (!any) { b = local; any = true; } else b.Encapsulate(local);
            }
            return b;
        }

        private static bool IsKept(Transform t, Transform stop)
        {
            for (; t != null && t != stop; t = t.parent)
                if (Keep.Contains(t.name)) return true;
            return false;
        }

        private static Vector2 Foot(Transform root, Bounds b, Anchor anchor) =>
            // Every fixture's origin is on its floor, so a standing thing stands at 0 whatever its shapes reached down to.
            anchor == Anchor.Ceiling ? new Vector2(b.center.x, b.max.y) : new Vector2(b.center.x, 0f);

        private static int Order(Transform group, int fallback)
        {
            int order = int.MaxValue;
            foreach (SpriteRenderer r in group.GetComponentsInChildren<SpriteRenderer>(true))
                if (!Keep.Contains(r.name)) order = Mathf.Min(order, r.sortingOrder);
            return order == int.MaxValue ? fallback : order;
        }

        private static void Put(Transform parent, Sprite sprite, Vector2 position, int order, string name = Drawn)
        {
            if (sprite == null) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
        }
    }
}
#endif
