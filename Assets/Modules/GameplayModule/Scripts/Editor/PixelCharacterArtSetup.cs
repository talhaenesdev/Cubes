#if UNITY_EDITOR
using System;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Entities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.GameplayModule.Editor
{
    // Puts the AI-drawn character sprites on the character prefabs. Run it from the menu when the sprites change: it
    // rewrites the prefabs, so it never runs by itself and edits made to them in between are kept until then.
    internal static class PixelCharacterArtSetup
    {
        // Kept visible over the drawing: the gun, the phone, the shadow and the marks over the head.
        private static readonly string[] KeepVisible =
        {
            "WeaponSocket", "Phone", "Shadow", "Mark", "Reload", "Headphones", "Blood"
        };

        private const string SpriteFolder = "Assets/Modules/GameplayModule/Art/Sprites/Characters";
        private const string PrefabFolder = "Assets/Modules/GameplayModule/Prefabs/Characters";
        private const string CharactersConfig = "Assets/Modules/GameplayModule/Scriptables/CD_Characters.asset";
        private const float PixelsPerUnit = 64f;

        private static readonly string[] Shapes =
        {
            "Rectangle", "Square", "Triangle", "Circle", "Pentagon", "Hexagon", "Diamond"
        };

        [MenuItem("Tools/FlowIoC-Modules/Gameplay/Apply Character Pixel Art")]
        public static void Apply()
        {
            foreach (string name in Shapes) ImportSprite(name);
            ImportSprite("Civilian");
            ImportSprite("ShopOwner");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (string name in Shapes)
                ApplySprite(PrefabPath(name), name);

            CreateRolePrefab("Civilian", "PBV_Character_Civilian");
            CreateRolePrefab("ShopOwner", "PBV_Character_ShopOwner");
            AssignRolePrefabs();
            AssetDatabase.SaveAssets();
        }

        private static void ImportSprite(string name)
        {
            string path = SpritePath(name);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            bool changed = importer.textureType != TextureImporterType.Sprite
                || Math.Abs(importer.spritePixelsPerUnit - PixelsPerUnit) > 0.01f
                || importer.filterMode != FilterMode.Point || importer.mipmapEnabled
                || importer.textureCompression != TextureImporterCompression.Uncompressed;
            if (!changed) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void CreateRolePrefab(string spriteName, string prefabName)
        {
            string source = PrefabPath("Circle");
            string destination = PrefabFolder + "/" + prefabName + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(source);
            try
            {
                root.name = prefabName;
                SetPixelSprite(root, spriteName);
                PrefabUtility.SaveAsPrefabAsset(root, destination);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ApplySprite(string prefabPath, string name)
        {
            if (!System.IO.File.Exists(SpritePath(name)) || AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                SetPixelSprite(root, name);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // The drawing hangs under Pose with its feet on the floor, and every shape of the old body, legs and arms is
        // switched off - except the parts the drawing does not show, which stay. Animation strips found beside the
        // still drawing (SPR_Character_<Name>_Walk.png and so on) are cut into frames for its PixelSpriteAnimator.
        private static void SetPixelSprite(GameObject root, string name)
        {
            Transform pose = root.transform.Find("Pose");
            bool keepGreen = name == "Square";
            Sprite sprite = PixelCharacterFrameCutter.Still(SpritePath(name), "SPR_Character_" + name + "_Still", keepGreen);
            if (pose == null || sprite == null) return;
            Transform old = pose.Find("Body/Pixel Art");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform art = pose.Find("Pixel Art");
            if (art == null)
            {
                art = new GameObject("Pixel Art").transform;
                art.SetParent(pose, false);
            }
            art.localPosition = Vector3.zero;
            art.localRotation = Quaternion.identity;
            art.localScale = Vector3.one;
            SpriteRenderer pixelRenderer = art.GetComponent<SpriteRenderer>();
            if (pixelRenderer == null) pixelRenderer = art.gameObject.AddComponent<SpriteRenderer>();
            pixelRenderer.sprite = sprite;
            pixelRenderer.color = Color.white;
            // Under the held gun (11-12), with the drawn arm between.
            pixelRenderer.sortingOrder = 9;

            PixelSpriteAnimator animator = art.GetComponent<PixelSpriteAnimator>();
            if (animator == null) animator = art.gameObject.AddComponent<PixelSpriteAnimator>();
            animator.Renderer = pixelRenderer;
            animator.Still = sprite;
            animator.Idle = Strip(name, "Idle", keepGreen, true);
            animator.Walk = Strip(name, "Walk", keepGreen, true);
            animator.Run = Strip(name, "Run", keepGreen, false);
            animator.Crouch = Strip(name, "Crouch", keepGreen, false);
            animator.CrouchWalk = Strip(name, "CrouchWalk", keepGreen, false);
            animator.CrouchWalkArmed = Strip(name, "CrouchWalkArmed", keepGreen, false);
            animator.Death = Strip(name, "Death", keepGreen, true);
            animator.IdleArmed = Strip(name, "IdleArmed", keepGreen, true);
            animator.WalkArmed = Strip(name, "WalkArmed", keepGreen, true);
            animator.RunArmed = Strip(name, "RunArmed", keepGreen, false);
            animator.CrouchArmed = Strip(name, "CrouchArmed", keepGreen, false);
            animator.WalkBackward = Strip(name, "WalkBackward", keepGreen, true);
            animator.Jump = Strip(name, "Jump", keepGreen, false);
            animator.Melee = Strip(name, "Melee", keepGreen, true);
            animator.Kick = Strip(name, "Kick", keepGreen, true);
            animator.Hurt = Strip(name, "Hurt", keepGreen, true);

            foreach (SpriteRenderer renderer in pose.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer != pixelRenderer) renderer.enabled = Kept(renderer.transform, pose);
            animator.Arm = SetPixelArm(pose, name, keepGreen, sprite);
        }

        // The drawn arm hangs from the rig's ArmPivot at the drawing's shoulder, at the same pixel size as the body
        // frames drawn with it, and the WeaponSocket moves out to its hand, so the gun sits in the drawn hand at every
        // angle. The arm is scaled like the armed strip drawn beside it, which the AI makes at the same size.
        private const float ShoulderHeight = 0.6f;
        private const float ArmLength = 0.4f;
        private const float ShoulderForward = 0f;

        private static SpriteRenderer SetPixelArm(Transform pose, string name, bool keepGreen, Sprite still)
        {
            Transform pivot = Find(pose, "ArmPivot");
            Transform socket = pivot != null ? Find(pivot, "WeaponSocket") : null;
            Transform old = pivot != null ? pivot.Find("Pixel Arm") : null;
            Sprite sprite = PixelCharacterFrameCutter.Single(SpriteFolder + "/SPR_Character_" + name + "_Arm.png",
                "SPR_Character_" + name + "_Arm", keepGreen, new Vector2(0f, 0.5f));
            if (pivot == null || socket == null || sprite == null)
            {
                if (old != null) Object.DestroyImmediate(old.gameObject);
                return null;
            }
            Transform arm = old != null ? old : new GameObject("Pixel Arm").transform;
            arm.SetParent(pivot, false);
            // The AI draws the arm far larger than the body it goes with, so it is sized against the body instead:
            // shoulder to fist is ArmLength of the character's height, which keeps the sleeve as thick as the coat's.
            float scale = still.bounds.size.y * ArmLength / Mathf.Max(0.001f, sprite.bounds.size.x);
            float length = sprite.bounds.size.x * scale;
            // The shoulder sits inside the coat, over the middle of the body, so the sleeve starts under the coat.
            pivot.localPosition = new Vector3(ShoulderForward, still.bounds.size.y * ShoulderHeight, pivot.localPosition.z);
            socket.localPosition = new Vector3(length * 0.92f, socket.localPosition.y, socket.localPosition.z);
            arm.localPosition = Vector3.zero;
            arm.localRotation = Quaternion.identity;
            arm.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer renderer = arm.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = arm.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            // Over the body, under the gun.
            renderer.sortingOrder = 10;
            renderer.enabled = false;
            return renderer;
        }

        private static Transform Find(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }

        private static Sprite[] Strip(string name, string move, bool keepGreen, bool upright) =>
            PixelCharacterFrameCutter.Frames(SpritePath(name), SpriteFolder + "/SPR_Character_" + name + "_" + move + ".png",
                "SPR_Character_" + name + "_" + move, keepGreen, upright).ToArray();

        private static bool Kept(Transform part, Transform pose)
        {
            for (Transform at = part; at != null && at != pose; at = at.parent)
                foreach (string name in KeepVisible)
                    if (at.name.StartsWith(name)) return true;
            return false;
        }

        private static void AssignRolePrefabs()
        {
            CD_Characters config = AssetDatabase.LoadAssetAtPath<CD_Characters>(CharactersConfig);
            if (config == null) return;
            config.CivilianPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/PBV_Character_Civilian.prefab");
            config.BossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/PBV_Character_ShopOwner.prefab");
            EditorUtility.SetDirty(config);
        }

        private static string SpritePath(string name) => SpriteFolder + "/SPR_Character_" + name + ".png";
        private static string PrefabPath(string name) => PrefabFolder + "/PB_Character_" + name + ".prefab";
    }
}
#endif
