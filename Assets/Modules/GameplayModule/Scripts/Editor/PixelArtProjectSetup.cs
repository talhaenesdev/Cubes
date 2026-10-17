#if UNITY_EDITOR
using System;
using Modules.GameplayModule.Data.UnityObjects;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    internal sealed class PixelArtTexturePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Modules/GameplayModule/Art/", StringComparison.Ordinal)) return;

            var importer = (TextureImporter) assetImporter;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }

    internal static class PixelArtProjectSetup
    {
        private const string GameplayConfigPath = "Assets/Modules/GameplayModule/Scriptables/CD_Gameplay.asset";
        private const string LevelFolder = "Assets/Modules/GameplayModule/Prefabs/Level";
        private const string PixelPerfectTypeName = "UnityEngine.U2D.PixelPerfectCamera, Unity.2D.PixelPerfect";

        [InitializeOnLoadMethod]
        private static void QueueSetup()
        {
            EditorApplication.delayCall += Apply;
        }

        [MenuItem("Tools/FlowIoC-Modules/Gameplay/Apply Pixel Art Settings")]
        private static void Apply()
        {
            Type pixelPerfectType = Type.GetType(PixelPerfectTypeName);
            if (pixelPerfectType == null) return;

            CD_Gameplay config = AssetDatabase.LoadAssetAtPath<CD_Gameplay>(GameplayConfigPath);
            if (config == null) return;

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] {LevelFolder});
            foreach (string prefabGuid in prefabGuids)
                ApplyToPrefab(AssetDatabase.GUIDToAssetPath(prefabGuid), pixelPerfectType, config);
        }

        private static void ApplyToPrefab(string path, Type pixelPerfectType, CD_Gameplay config)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
                bool changed = false;
                foreach (Camera camera in cameras)
                {
                    Component component = camera.GetComponent(pixelPerfectType);
                    if (!config.PixelArtCamera)
                    {
                        if (component == null) continue;
                        UnityEngine.Object.DestroyImmediate(component);
                        changed = true;
                        continue;
                    }

                    if (component == null)
                    {
                        component = camera.gameObject.AddComponent(pixelPerfectType);
                        changed = true;
                    }

                    var serialized = new SerializedObject(component);
                    changed |= Set(serialized, "m_AssetsPPU", Mathf.Max(1, config.PixelArtAssetsPPU));
                    changed |= Set(serialized, "m_RefResolutionX", Mathf.Max(1, config.PixelArtReferenceWidth));
                    changed |= Set(serialized, "m_RefResolutionY", Mathf.Max(1, config.PixelArtReferenceHeight));
                    changed |= Set(serialized, "m_UpscaleRT", config.PixelArtUpscaleRenderTexture);
                    changed |= Set(serialized, "m_PixelSnapping", !config.PixelArtUpscaleRenderTexture);
                    changed |= Set(serialized, "m_CropFrameX", false);
                    changed |= Set(serialized, "m_CropFrameY", false);
                    changed |= Set(serialized, "m_StretchFill", false);
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool Set(SerializedObject target, string propertyName, int value)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            if (property == null || property.intValue == value) return false;
            property.intValue = value;
            return true;
        }

        private static bool Set(SerializedObject target, string propertyName, bool value)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            if (property == null || property.boolValue == value) return false;
            property.boolValue = value;
            return true;
        }
    }
}
#endif
