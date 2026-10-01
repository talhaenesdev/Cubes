#if UNITY_EDITOR
using System.Collections.Generic;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.GameplayModule.Editor
{
    // The noir grade: one URP volume profile, and a global volume on every level that turns its camera's
    // post-processing on. Added to a level in place, so nothing else in the prefab is touched.
    public partial class NightShiftAssetBuilder
    {
        public const string MoodProfile = Art + "/Materials/VP_Noir.asset";
        private const string MoodChild = "Noir Mood";

        private void BuildMood()
        {
            if (!Exists(MoodProfile) || _overwrite)
            {
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                var grade = profile.Add<ColorAdjustments>(true);
                grade.saturation.Override(-45f);
                grade.contrast.Override(22f);
                grade.postExposure.Override(-0.25f);
                grade.colorFilter.Override(new Color(1f, 0.92f, 0.82f));
                var vignette = profile.Add<Vignette>(true);
                vignette.intensity.Override(0.45f);
                vignette.smoothness.Override(0.45f);
                vignette.color.Override(Color.black);
                var grain = profile.Add<FilmGrain>(true);
                grain.type.Override(FilmGrainLookup.Medium1);
                grain.intensity.Override(0.35f);
                var bloom = profile.Add<Bloom>(true);
                bloom.intensity.Override(0.6f);
                bloom.threshold.Override(0.9f);
                Save(profile, MoodProfile);
                foreach (VolumeComponent component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
                AssetDatabase.SaveAssets();
            }
            foreach (string level in MoodLevels()) Mood(level);
        }

        // The levels to grade: the two shipped ones and every level CD_Missions names.
        private List<string> MoodLevels()
        {
            var paths = new List<string> {LevelPrefab, HideoutPrefab};
            var missions = AssetDatabase.LoadAssetAtPath<CD_Missions>(MissionsAsset);
            if (missions != null)
                foreach (MissionCVO mission in missions.Missions)
                    if (mission.Level != null && !paths.Contains(AssetDatabase.GetAssetPath(mission.Level)))
                        paths.Add(AssetDatabase.GetAssetPath(mission.Level));
            paths.RemoveAll(path => !Exists(path));
            return paths;
        }

        public bool NeedsMood(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab != null && prefab.transform.Find(MoodChild) == null;
        }

        private void Mood(string path)
        {
            if (!NeedsMood(path)) return;
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var volumeObject = new GameObject(MoodChild);
                volumeObject.transform.SetParent(contents.transform, false);
                var volume = volumeObject.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 10f;
                volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(MoodProfile);
                var level = contents.GetComponent<LevelAuthoring>();
                if (level != null && level.Camera != null)
                {
                    var data = level.Camera.GetComponent<UniversalAdditionalCameraData>();
                    if (data == null) data = level.Camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                    data.renderPostProcessing = true;
                }
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                _report.Add("Added the noir grade to " + path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        public bool HasMoodWork()
        {
            if (!Exists(MoodProfile)) return true;
            foreach (string level in MoodLevels())
                if (NeedsMood(level)) return true;
            return false;
        }
    }
}
#endif
