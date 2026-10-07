#if UNITY_EDITOR
using System.IO;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The music folder and CD_Music, and the phone every character carries for the contract intro.
    public partial class NightShiftAssetBuilder
    {
        public const string MusicFolder = Art + "/Audio/Music";
        public const string MusicAsset = ModuleRoot + "/Scriptables/CD_Music.asset";
        public const string HideoutMusicFolder = MusicFolder + "/Hideout";
        public const string ContractMusicFolder = MusicFolder + "/Contracts";

        private void BuildMusic()
        {
            Folder(HideoutMusicFolder);
            Folder(ContractMusicFolder);
            string readme = MusicFolder + "/LICENSES.txt";
            if (!File.Exists(readme))
            {
                File.WriteAllText(readme,
                    "Drop music into Hideout/ (plays at home) or Contracts/ (plays on jobs) - mp3, ogg or wav.\n" +
                    "Each file is added to Scriptables/CD_Music on import, filed under the folder it is in.\n" +
                    "Name files SND_MUS_<Title>_01, e.g. SND_MUS_NightOnTheDocks_01.mp3.\n\n" +
                    "Keep every track's licence below, and copy its attribution into the track's Credit field.\n" +
                    "Kevin MacLeod (incompetech.com) tracks are CC BY 4.0 and must be credited, for example:\n" +
                    "\"Night on the Docks - Sax\" Kevin MacLeod (incompetech.com)\n" +
                    "Licensed under Creative Commons: By Attribution 4.0 License\n" +
                    "http://creativecommons.org/licenses/by/4.0/\n\n" +
                    "--- Tracks in this folder ---\n");
                AssetDatabase.ImportAsset(readme);
                _report.Add("Wrote " + readme);
            }
            var music = AssetDatabase.LoadAssetAtPath<CD_Music>(MusicAsset);
            if (music == null)
            {
                music = ScriptableObject.CreateInstance<CD_Music>();
                AssetDatabase.CreateAsset(music, MusicAsset);
                _report.Add("Wrote " + MusicAsset);
            }
            ListMusic(music);
        }

        // Every clip in the music folders that CD_Music does not list yet is added, titled from its file name and
        // filed as a Hideout or Contract track by its folder.
        public void ListMusic(CD_Music music)
        {
            bool changed = false;
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] {MusicFolder}))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;
                // A file moved between the two folders changes where it plays.
                MusicTrackCVO listed = music.Tracks.Find(track => track.Clip == clip);
                if (listed != null)
                {
                    MusicCategory folder = path.StartsWith(HideoutMusicFolder + "/") ? MusicCategory.Hideout : MusicCategory.Contract;
                    if (listed.Category != folder) { listed.Category = folder; changed = true; }
                    continue;
                }
                // A stray space in a downloaded file name does not end up in the Id.
                string name = Path.GetFileNameWithoutExtension(path).Trim();
                music.Tracks.Add(new MusicTrackCVO
                {
                    Id = name.ToLowerInvariant(), Title = TitleFrom(name), Artist = "UNKNOWN ARTIST", Clip = clip,
                    Category = path.StartsWith(HideoutMusicFolder + "/") ? MusicCategory.Hideout : MusicCategory.Contract,
                });
                _report.Add("Listed " + name + " in CD_Music - set its artist and credit");
                changed = true;
            }
            if (changed) EditorUtility.SetDirty(music);
        }

        // SND_MUS_NightOnTheDocks_01 reads as NIGHT ON THE DOCKS.
        private string TitleFrom(string fileName)
        {
            string core = fileName;
            if (core.StartsWith("SND_MUS_")) core = core.Substring(8);
            int number = core.LastIndexOf('_');
            if (number > 0 && int.TryParse(core.Substring(number + 1), out _)) core = core.Substring(0, number);
            var title = new System.Text.StringBuilder();
            for (int i = 0; i < core.Length; i++)
            {
                char c = core[i];
                if (c == '_' || c == '-') { title.Append(' '); continue; }
                if (i > 0 && char.IsUpper(c) && char.IsLower(core[i - 1])) title.Append(' ');
                title.Append(c);
            }
            return title.ToString().Trim().ToUpperInvariant();
        }

        // ---------------------------------------------------------------- phone

        private void PhoneForCharacters()
        {
            foreach (CharacterShape shape in System.Enum.GetValues(typeof(CharacterShape)))
            {
                string path = CharacterPath(shape);
                if (!NeedsPhone(path)) continue;
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform arm = contents.GetComponentInChildren<CharacterRig>(true).transform.Find("Pose/Body/ArmPivot");
                    if (arm == null) continue;
                    GameObject phone = Node("Phone", arm, new Vector2(0.36f, 0.04f));
                    Part("Case", phone.transform, Vector2.zero, new Vector2(0.11f, 0.2f), _ink, 14, -90f);
                    SpriteRenderer screen = Part("Screen", phone.transform, Vector2.zero, new Vector2(0.08f, 0.15f), new Color(0.25f, 0.35f, 0.4f), 15, -90f);
                    Part("Screen glow", phone.transform, Vector2.zero, new Vector2(0.5f, 0.5f), new Color(0.6f, 0.85f, 1f, 0.18f), 13).sprite = _soft;
                    phone.SetActive(false);
                    var rig = contents.GetComponent<CharacterRig>();
                    Assign(rig, "_phone", phone);
                    Assign(rig, "_phoneScreen", screen);
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    _report.Add("Gave " + Path.GetFileNameWithoutExtension(path) + " a phone for the contract intro");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }

        public bool NeedsPhone(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab != null && prefab.transform.Find("Pose/Body/ArmPivot/Phone") == null;
        }
    }

    // A clip dropped into the music folder is listed in CD_Music straight away.
    public class NightShiftMusicImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            bool music = false;
            foreach (string path in imported)
                if (path.StartsWith(NightShiftAssetBuilder.MusicFolder + "/") && AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(AudioClip))
                    music = true;
            if (!music) return;
            var config = AssetDatabase.LoadAssetAtPath<CD_Music>(NightShiftAssetBuilder.MusicAsset);
            if (config == null) return;
            new NightShiftAssetBuilder().ListMusic(config);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
