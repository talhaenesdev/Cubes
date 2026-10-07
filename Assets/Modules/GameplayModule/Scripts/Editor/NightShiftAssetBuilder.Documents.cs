#if UNITY_EDITOR
using System.Collections.Generic;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The file every contract is after, the arrow that points home once it is taken, and the headphones the
    // player puts on before the music. Levels that have no document get one where the last guard stands
    // furthest from the entrance; move it in the level prefab and that is where it is found.
    public partial class NightShiftAssetBuilder
    {
        public const string DocumentPrefab = Prefabs + "/Fixtures/PB_Document.prefab";

        private IEnumerable<string> DocumentLevels()
        {
            yield return LevelPrefab;
            yield return DocksPrefab;
            foreach (ContractRecipe recipe in _contracts) yield return ContractPath(recipe.Id);
        }

        private bool DocumentsMissing()
        {
            if (!Exists(DocumentPrefab)) return true;
            var exit = AssetDatabase.LoadAssetAtPath<GameObject>(Exit);
            if (exit != null && exit.transform.Find("Guide") == null) return true;
            var hero = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath(CharacterShape.Rectangle));
            if (hero != null && hero.GetComponentInChildren<CharacterRig>(true).transform.Find("Pose/Body/Torso/Headphones") == null) return true;
            foreach (string path in DocumentLevels())
            {
                var level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(path);
                if (level != null && level.GetComponentInChildren<DocumentView>(true) == null) return true;
            }
            return false;
        }

        private void BuildDocuments()
        {
            BuildDocumentPrefab();
            GuideForExit();
            HeadphonesForHero();
            foreach (string path in DocumentLevels()) PlaceDocument(path);
        }

        private void BuildDocumentPrefab()
        {
            if (!ShouldWrite(DocumentPrefab)) return;
            GameObject root = Node("PB_Document", null, Vector2.zero);
            var view = root.AddComponent<DocumentView>();
            GameObject visual = Node("Visual", root.transform, Vector2.zero);
            SpriteRenderer glow = Part("Glow", visual.transform, new Vector2(0f, 0.75f), new Vector2(1.8f, 1.8f), new Color(1f, 0.85f, 0.45f, 0.35f), 18);
            glow.sprite = _soft;
            Part("Stand", visual.transform, new Vector2(0f, 0.32f), new Vector2(0.7f, 0.64f), new Color(0.2f, 0.13f, 0.08f), 18);
            Part("Stand top", visual.transform, new Vector2(0f, 0.66f), new Vector2(0.8f, 0.06f), new Color(0.3f, 0.2f, 0.12f), 19);
            Transform folder = Node("Folder", visual.transform, new Vector2(0f, 0.8f)).transform;
            folder.localRotation = Quaternion.Euler(0f, 0f, -8f);
            Part("Back", folder, new Vector2(0f, 0f), new Vector2(0.62f, 0.4f), new Color(0.62f, 0.48f, 0.26f), 20);
            Part("Tab", folder, new Vector2(-0.18f, 0.22f), new Vector2(0.22f, 0.06f), new Color(0.62f, 0.48f, 0.26f), 20);
            Part("Paper", folder, new Vector2(0.02f, 0.03f), new Vector2(0.54f, 0.32f), new Color(0.9f, 0.87f, 0.78f), 21);
            Part("Stamp", folder, new Vector2(0.04f, 0.03f), new Vector2(0.4f, 0.08f), _crimson, 22, 12f);
            Part("Line", folder, new Vector2(0f, -0.1f), new Vector2(0.4f, 0.02f), new Color(0.3f, 0.3f, 0.3f), 22);
            Part("Line", folder, new Vector2(0f, 0.13f), new Vector2(0.4f, 0.02f), new Color(0.3f, 0.3f, 0.3f), 22);
            Assign(view, "_visual", visual);
            Assign(view, "_glow", glow);
            SavePrefab<GameObject>(root, DocumentPrefab);
        }

        private void GuideForExit()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Exit);
            if (prefab == null || prefab.transform.Find("Guide") != null) return;
            GameObject contents = PrefabUtility.LoadPrefabContents(Exit);
            try
            {
                Color green = new(0.4f, 1f, 0.64f);
                GameObject guide = Node("Guide", contents.transform, new Vector2(0f, 3.45f));
                Part("Glow", guide.transform, Vector2.zero, new Vector2(1.6f, 1.6f), new Color(0.4f, 1f, 0.64f, 0.3f), 30).sprite = _soft;
                Part("Shaft", guide.transform, new Vector2(0f, 0.18f), new Vector2(0.16f, 0.4f), green, 31);
                Part("Head L", guide.transform, new Vector2(-0.1f, -0.08f), new Vector2(0.38f, 0.12f), green, 31, -45f);
                Part("Head R", guide.transform, new Vector2(0.1f, -0.08f), new Vector2(0.38f, 0.12f), green, 31, 45f);
                guide.SetActive(false);
                Assign(contents.GetComponent<ExitView>(), "_guide", guide.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, Exit);
                _report.Add("Gave the exit an arrow that points home once the documents are taken");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        // Studio headphones: a band over the hat, one cup on the ear facing the camera, a coiled cord.
        private void HeadphonesForHero()
        {
            string path = CharacterPath(CharacterShape.Rectangle);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var rig = contents.GetComponentInChildren<CharacterRig>(true);
                Transform torso = rig.transform.Find("Pose/Body/Torso");
                if (torso == null || torso.Find("Headphones") != null) return;
                Sprite round = _shapes[CharacterShape.Circle];
                Color shell = new(0.08f, 0.07f, 0.07f);
                GameObject phones = Node("Headphones", torso, Vector2.zero);
                Part("Band", phones.transform, new Vector2(-0.04f, 0.9f), new Vector2(0.5f, 0.07f), shell, 16, -5f);
                Part("Band side", phones.transform, new Vector2(-0.27f, 0.62f), new Vector2(0.06f, 0.56f), shell, 16, 4f);
                Part("Cup", phones.transform, new Vector2(-0.24f, 0.32f), new Vector2(0.26f, 0.3f), shell, 17).sprite = round;
                Part("Cup ring", phones.transform, new Vector2(-0.24f, 0.32f), new Vector2(0.18f, 0.21f), _crimson, 18).sprite = round;
                Part("Cup cap", phones.transform, new Vector2(-0.24f, 0.32f), new Vector2(0.09f, 0.1f), _hatBrass, 19).sprite = round;
                Part("Cord", phones.transform, new Vector2(-0.24f, 0.0f), new Vector2(0.025f, 0.36f), shell, 15);
                phones.SetActive(false);
                Assign(rig, "_headphones", phones.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                _report.Add("Gave the Rectangle headphones for the contract intro");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private void PlaceDocument(string path)
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(path);
            if (level == null || level.GetComponentInChildren<DocumentView>(true) != null) return;
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var authoring = contents.GetComponent<LevelAuthoring>();
                PlayerSpawn spawn = contents.GetComponentInChildren<PlayerSpawn>(true);
                Vector2 start = spawn != null ? (Vector2) spawn.transform.localPosition : Vector2.zero;
                // The guard posted furthest from the entrance - upstairs counts extra - keeps the file.
                GuardSpawn keeper = null;
                float best = -1f;
                foreach (GuardSpawn guard in contents.GetComponentsInChildren<GuardSpawn>(true))
                {
                    Vector2 at = guard.transform.localPosition;
                    float score = Mathf.Abs(at.x - start.x) + at.y * 2f;
                    if (score <= best) continue;
                    best = score;
                    keeper = guard;
                }
                Vector2 spot = keeper != null ? (Vector2) keeper.transform.localPosition : new Vector2(authoring.HalfWidth - 2f, 0f);
                // Just past the guard, away from the entrance, and inside the walls.
                spot.x = Mathf.Clamp(spot.x + Mathf.Sign(spot.x - start.x) * 0.9f, -authoring.HalfWidth + 0.8f, authoring.HalfWidth - 0.8f);
                Transform fixtures = contents.transform.Find("Fixtures") ?? contents.transform;
                Place(DocumentPrefab, fixtures, spot).name = "Document";
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                _report.Add("Hid the documents in " + System.IO.Path.GetFileNameWithoutExtension(path));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
    }
}
#endif
