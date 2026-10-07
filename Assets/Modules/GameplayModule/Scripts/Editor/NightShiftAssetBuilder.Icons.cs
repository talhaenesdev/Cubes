#if UNITY_EDITOR
using System.Collections.Generic;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The hideout says what things are with shapes, not words: an icon floats over every station - a pin for the
    // contracts, a wrench for upgrades, a hanger for the wardrobe, a bullet for the locker, a note for the tape
    // deck - and the neon on the wall is a rectangle and a square with a heart between them. Prefabs written
    // earlier lose their labels and gain their icon in place.
    public partial class NightShiftAssetBuilder
    {
        private const string IconChild = "Icon";
        private const string NeonChild = "Neon Us";

        private readonly Dictionary<StationKind, (string path, float height)> _stationIcons = new()
        {
            {StationKind.MissionBoard, (Prefabs + "/Fixtures/PB_MissionBoard.prefab", 2.75f)},
            {StationKind.UpgradeBench, (Prefabs + "/Fixtures/PB_UpgradeBench.prefab", 2.95f)},
            {StationKind.CassettePlayer, (Prefabs + "/Fixtures/PB_CassettePlayer.prefab", 1.75f)},
            {StationKind.Wardrobe, (Prefabs + "/Fixtures/PB_Wardrobe.prefab", 3.15f)},
            {StationKind.WeaponLocker, (Prefabs + "/Fixtures/PB_WeaponLocker.prefab", 2.65f)},
        };

        public bool NeedsIcons()
        {
            foreach (var entry in _stationIcons.Values)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.path);
                if (prefab != null && prefab.transform.Find(IconChild) == null) return true;
            }
            var hideout = AssetDatabase.LoadAssetAtPath<GameObject>(HideoutPrefab);
            Transform home = hideout != null ? hideout.transform.Find(HomeChild) : null;
            return home != null && home.Find(NeonChild) == null;
        }

        private void IconsForHideout()
        {
            foreach (var pair in _stationIcons)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pair.Value.path);
                if (prefab == null || prefab.transform.Find(IconChild) != null) continue;
                GameObject contents = PrefabUtility.LoadPrefabContents(pair.Value.path);
                try
                {
                    RemoveLabels(contents.transform);
                    StationIcon(pair.Key, contents.transform, pair.Value.height);
                    PrefabUtility.SaveAsPrefabAsset(contents, pair.Value.path);
                    _report.Add("Gave " + prefab.name + " an icon instead of a label");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
            var hideout = AssetDatabase.LoadAssetAtPath<GameObject>(HideoutPrefab);
            Transform home = hideout != null ? hideout.transform.Find(HomeChild) : null;
            if (home == null || home.Find(NeonChild) != null) return;
            GameObject level = PrefabUtility.LoadPrefabContents(HideoutPrefab);
            try
            {
                RemoveLabels(level.transform);
                Transform glow = level.transform.Find(HomeChild + "/Neon glow");
                if (glow != null) Object.DestroyImmediate(glow.gameObject);
                NeonUs(Node(NeonChild, level.transform.Find(HomeChild), new Vector2(-1.1f, 3.0f)).transform);
                PrefabUtility.SaveAsPrefabAsset(level, HideoutPrefab);
                _report.Add("Swapped the hideout's words for shapes: a neon of the two of you");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(level);
            }
        }

        // The portrait and the photo once hung in the hideout come down wherever an earlier build put them.
        private readonly string[] Pictures = {"Portrait Engagement", "Portrait", "Photo Of Us"};

        public bool HideoutHasPictures()
        {
            var hideout = AssetDatabase.LoadAssetAtPath<GameObject>(HideoutPrefab);
            Transform home = hideout != null ? hideout.transform.Find(HomeChild) : null;
            if (home == null) return false;
            foreach (string picture in Pictures)
                if (home.Find(picture) != null) return true;
            return false;
        }

        private void TakeDownPictures()
        {
            if (!HideoutHasPictures()) return;
            GameObject level = PrefabUtility.LoadPrefabContents(HideoutPrefab);
            try
            {
                Transform home = level.transform.Find(HomeChild);
                foreach (string picture in Pictures)
                {
                    Transform found = home.Find(picture);
                    if (found != null) Object.DestroyImmediate(found.gameObject);
                }
                PrefabUtility.SaveAsPrefabAsset(level, HideoutPrefab);
                _report.Add("Took the pictures down in the hideout");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(level);
            }
        }

        // Every TextMesh the builder ever wrote under this object goes. One inside a nested prefab is left to that
        // prefab's own patch - it cannot be destroyed from here, and the station prefabs are patched first.
        private void RemoveLabels(Transform root)
        {
            foreach (TextMesh label in root.GetComponentsInChildren<TextMesh>(true))
                if (!PrefabUtility.IsPartOfPrefabInstance(label.gameObject))
                    Object.DestroyImmediate(label.gameObject);
        }

        private void StationIcon(StationKind kind, Transform station, float height)
        {
            Sprite round = _shapes[CharacterShape.Circle];
            Transform icon = Node(IconChild, station, new Vector2(0f, height)).transform;
            Color tint = kind switch
            {
                StationKind.MissionBoard => new Color(0.85f, 0.15f, 0.18f),
                StationKind.WeaponLocker => new Color(0.85f, 0.68f, 0.3f),
                StationKind.CassettePlayer => new Color(0.95f, 0.85f, 0.6f),
                _ => new Color(0.85f, 0.75f, 0.55f),
            };
            SpriteRenderer glow = Part("Glow", icon, Vector2.zero, new Vector2(0.75f, 0.75f), new Color(tint.r, tint.g, tint.b, 0.12f), -8);
            glow.sprite = _soft;
            Part("Disc", icon, Vector2.zero, new Vector2(0.42f, 0.42f), new Color(0.05f, 0.04f, 0.04f, 0.85f), -7).sprite = round;
            switch (kind)
            {
                case StationKind.MissionBoard: // a push pin
                    Part("Pin head", icon, new Vector2(0f, 0.06f), new Vector2(0.14f, 0.14f), tint, -6).sprite = round;
                    Part("Pin collar", icon, new Vector2(0f, -0.02f), new Vector2(0.1f, 0.04f), tint * 0.8f, -6);
                    Part("Pin needle", icon, new Vector2(0f, -0.09f), new Vector2(0.02f, 0.11f), new Color(0.8f, 0.8f, 0.8f), -6);
                    Part("Pin shine", icon, new Vector2(-0.03f, 0.09f), new Vector2(0.03f, 0.03f), Color.white, -5).sprite = round;
                    break;
                case StationKind.UpgradeBench: // a wrench
                    Part("Wrench handle", icon, new Vector2(0.03f, -0.03f), new Vector2(0.05f, 0.24f), tint, -6, -40f);
                    Part("Wrench head", icon, new Vector2(-0.06f, 0.08f), new Vector2(0.13f, 0.13f), tint, -6).sprite = round;
                    Part("Wrench jaw", icon, new Vector2(-0.09f, 0.11f), new Vector2(0.06f, 0.07f), new Color(0.05f, 0.04f, 0.04f), -5, -40f);
                    break;
                case StationKind.Wardrobe: // a coat hanger
                    Part("Hanger hook", icon, new Vector2(0.01f, 0.1f), new Vector2(0.06f, 0.06f), tint, -6).sprite = round;
                    Part("Hanger hook hole", icon, new Vector2(0.01f, 0.1f), new Vector2(0.03f, 0.03f), new Color(0.05f, 0.04f, 0.04f), -5).sprite = round;
                    Part("Hanger neck", icon, new Vector2(0f, 0.05f), new Vector2(0.02f, 0.06f), tint, -6);
                    Part("Hanger arm L", icon, new Vector2(-0.07f, -0.01f), new Vector2(0.17f, 0.025f), tint, -6, 32f);
                    Part("Hanger arm R", icon, new Vector2(0.07f, -0.01f), new Vector2(0.17f, 0.025f), tint, -6, -32f);
                    Part("Hanger bar", icon, new Vector2(0f, -0.06f), new Vector2(0.28f, 0.025f), tint, -6);
                    break;
                case StationKind.WeaponLocker: // a bullet
                    Part("Bullet case", icon, new Vector2(0f, -0.04f), new Vector2(0.08f, 0.16f), tint, -6);
                    Part("Bullet tip", icon, new Vector2(0f, 0.06f), new Vector2(0.08f, 0.12f), new Color(0.7f, 0.42f, 0.25f), -6).sprite = round;
                    Part("Bullet rim", icon, new Vector2(0f, -0.12f), new Vector2(0.1f, 0.025f), tint * 0.75f, -5);
                    break;
                case StationKind.CassettePlayer: // a music note
                    Part("Note head", icon, new Vector2(-0.04f, -0.07f), new Vector2(0.1f, 0.08f), tint, -6, 20f).sprite = round;
                    Part("Note stem", icon, new Vector2(0.01f, 0.02f), new Vector2(0.025f, 0.2f), tint, -6);
                    Part("Note flag", icon, new Vector2(0.05f, 0.09f), new Vector2(0.08f, 0.03f), tint, -6, -30f);
                    break;
            }
            var view = station.GetComponent<StationView>();
            if (view == null) return;
            Assign(view, "_icon", icon);
            Assign(view, "_iconGlow", glow);
        }

        // Neon tubes on the wall: an outline rectangle and an outline square, a heart between them, a soft glow.
        private void NeonUs(Transform root)
        {
            Color tube = new(1f, 0.32f, 0.4f);
            Part("Glow", root, Vector2.zero, new Vector2(2.6f, 1f), new Color(0.9f, 0.1f, 0.2f, 0.2f), -41).sprite = _soft;
            void Box(string name, Vector2 at, Vector2 size)
            {
                Part(name + " top", root, at + new Vector2(0f, size.y * 0.5f), new Vector2(size.x, 0.035f), tube, -39);
                Part(name + " bottom", root, at - new Vector2(0f, size.y * 0.5f), new Vector2(size.x, 0.035f), tube, -39);
                Part(name + " left", root, at - new Vector2(size.x * 0.5f, 0f), new Vector2(0.035f, size.y), tube, -39);
                Part(name + " right", root, at + new Vector2(size.x * 0.5f, 0f), new Vector2(0.035f, size.y), tube, -39);
            }
            Box("Rectangle", new Vector2(-0.6f, 0f), new Vector2(0.32f, 0.6f));
            Box("Square", new Vector2(0.6f, -0.08f), new Vector2(0.4f, 0.4f));
            Sprite round = _shapes[CharacterShape.Circle];
            Part("Heart L", root, new Vector2(-0.06f, 0.04f), new Vector2(0.14f, 0.14f), tube, -39).sprite = round;
            Part("Heart R", root, new Vector2(0.06f, 0.04f), new Vector2(0.14f, 0.14f), tube, -39).sprite = round;
            Part("Heart point", root, new Vector2(0f, -0.04f), new Vector2(0.15f, 0.15f), tube, -39, 45f);
            Part("Heart glow", root, new Vector2(0f, 0f), new Vector2(0.6f, 0.6f), new Color(1f, 0.3f, 0.4f, 0.3f), -40).sprite = _soft;
        }
    }
}
#endif
