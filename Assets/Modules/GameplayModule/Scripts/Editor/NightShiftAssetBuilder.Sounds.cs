#if UNITY_EDITOR
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.ViewsMediators;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // CD_Sounds, filled with the clips in Art/Audio and handed to the NightShiftView on GameplaySystemRoot.
    public partial class NightShiftAssetBuilder
    {
        public const string SoundsAsset = ModuleRoot + "/Scriptables/CD_Sounds.asset";

        private bool SoundsMissing()
        {
            if (!Exists(SoundsAsset)) return true;
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(RootPrefab);
            var view = root != null ? root.GetComponentInChildren<NightShiftView>(true) : null;
            return view != null && new SerializedObject(view).FindProperty("_sounds").objectReferenceValue == null;
        }

        private void BuildSounds()
        {
            var sounds = AssetDatabase.LoadAssetAtPath<CD_Sounds>(SoundsAsset);
            if (sounds == null)
            {
                sounds = ScriptableObject.CreateInstance<CD_Sounds>();
                sounds.Footsteps = new[] {Clip("SND_SFX_Step_01"), Clip("SND_SFX_Step_02"), Clip("SND_SFX_Step_03"), Clip("SND_SFX_Step_04")};
                sounds.SwitchClick = Clip("SND_SFX_Switch_01");
                sounds.CatMeows = new[] {Clip("SND_SFX_Cat_Meow_01"), Clip("SND_SFX_Cat_Meow_02")};
                sounds.CatPurr = Clip("SND_SFX_Cat_Purr_Loop");
                sounds.HideoutAmbience = Clip("SND_AMB_Room_Loop");
                sounds.PhoneRing = Clip("SND_SFX_PhoneRing_Loop");
                sounds.CashPickup = Clip("SND_SFX_Cash_01");
                AssetDatabase.CreateAsset(sounds, SoundsAsset);
                _report.Add("Wrote " + SoundsAsset);
            }
            GameObject contents = PrefabUtility.LoadPrefabContents(RootPrefab);
            try
            {
                var view = contents.GetComponentInChildren<NightShiftView>(true);
                var serialized = new SerializedObject(view);
                SerializedProperty slot = serialized.FindProperty("_sounds");
                if (slot.objectReferenceValue != null) return;
                slot.objectReferenceValue = sounds;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(contents, RootPrefab);
                _report.Add("Assigned CD_Sounds to NightShiftView");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(Art + "/Audio/" + name + ".wav");
    }
}
#endif
