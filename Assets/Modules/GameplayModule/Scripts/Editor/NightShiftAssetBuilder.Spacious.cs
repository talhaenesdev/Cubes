#if UNITY_EDITOR
using System.Collections.Generic;
using Modules.GameplayModule.Entities;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // A one-off pass over every contract level: the building is stretched a quarter wider - rooms, doors, crates,
    // props, patrols and the art behind them - and a roomy room with no one in it gets one more guard, of a kind
    // the level already has. LayoutVersion marks a level done, so it runs once per level.
    public partial class NightShiftAssetBuilder
    {
        private const int SpaciousVersion = 1;
        // Vents were taken out of the game; this pass removes them from levels built before.
        private const int VentlessVersion = 2;
        // Vents came back as hiding places; this pass puts them back where the builder first placed them.
        private const int VentsBackVersion = 3;
        private const float Widen = 1.25f;
        // A room gets another guard when each guard in it would have more than this much floor to themselves.
        private const float FloorPerGuard = 5.5f;

        private bool LevelsNeedSpace()
        {
            foreach (string path in DocumentLevels())
            {
                var level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(path);
                if (level != null && level.LayoutVersion < VentsBackVersion) return true;
            }
            return false;
        }

        // Where each level's vents stood before they were taken out, already widened.
        private Vector2[] VentSpots(string path)
        {
            var spots = new List<Vector2>();
            if (path == DocksPrefab)
                foreach (Vector2 p in new[] {new Vector2(-18f, 0f), new Vector2(-4f, H), new Vector2(8f, 0f), new Vector2(21.5f, H), new Vector2(-28f, H), new Vector2(15.5f, 0f)})
                    spots.Add(p);
            else if (path == LevelPrefab)
                foreach (var vent in new Data.ValueObjects.ContractLevelVO().Vents) spots.Add(vent.Position);
            else
                foreach (ContractRecipe recipe in _contracts)
                    if (path == ContractPath(recipe.Id) && recipe.Floors > 1 && recipe.Half > 14f)
                    {
                        spots.Add(new Vector2(-recipe.Half + recipe.Half * 0.6f, 0f));
                        spots.Add(new Vector2(recipe.Half - recipe.Half * 0.5f, H));
                    }
            for (int i = 0; i < spots.Count; i++) spots[i] = new Vector2(spots[i].x * Widen, spots[i].y);
            return spots.ToArray();
        }

        private void PutVentsBack()
        {
            foreach (string path in DocumentLevels())
            {
                var asset = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(path);
                if (asset == null || asset.LayoutVersion >= VentsBackVersion) continue;
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform fixtures = contents.transform.Find("Fixtures") ?? contents.transform;
                    Vector2[] spots = contents.GetComponentsInChildren<VentView>(true).Length > 0 ? new Vector2[0] : VentSpots(path);
                    foreach (Vector2 spot in spots) Place(Vent, fixtures, spot);
                    contents.GetComponent<LevelAuthoring>().LayoutVersion = VentsBackVersion;
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    _report.Add("Put " + spots.Length + " vents back in " + System.IO.Path.GetFileNameWithoutExtension(path));
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }

        private void RemoveVents()
        {
            foreach (string path in DocumentLevels())
            {
                var asset = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(path);
                if (asset == null || asset.LayoutVersion >= VentlessVersion) continue;
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    VentView[] vents = contents.GetComponentsInChildren<VentView>(true);
                    foreach (VentView vent in vents) Object.DestroyImmediate(vent.gameObject);
                    contents.GetComponent<LevelAuthoring>().LayoutVersion = VentlessVersion;
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    _report.Add("Took " + vents.Length + " vents out of " + System.IO.Path.GetFileNameWithoutExtension(path));
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }

        private void MakeLevelsSpacious()
        {
            foreach (string path in DocumentLevels())
            {
                var asset = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(path);
                if (asset == null || asset.LayoutVersion >= SpaciousVersion) continue;
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var level = contents.GetComponent<LevelAuthoring>();
                    int added = Stretch(level, contents.transform);
                    level.LayoutVersion = SpaciousVersion;
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    _report.Add("Widened " + System.IO.Path.GetFileNameWithoutExtension(path) + " and posted " + added + " more guards");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }

        private int Stretch(LevelAuthoring level, Transform root)
        {
            level.HalfWidth *= Widen;
            Transform art = root.Find("Art");
            if (art != null) art.localScale = new Vector3(art.localScale.x * Widen, art.localScale.y, art.localScale.z);
            foreach (string group in new[] {"Fixtures", "Spawns"})
            {
                Transform parent = root.Find(group);
                if (parent == null) continue;
                foreach (Transform child in parent)
                    child.localPosition = new Vector3(child.localPosition.x * Widen, child.localPosition.y, child.localPosition.z);
            }
            foreach (LightRoomView room in root.GetComponentsInChildren<LightRoomView>(true))
            {
                room.Left *= Widen;
                room.Right *= Widen;
            }
            foreach (GuardSpawn guard in root.GetComponentsInChildren<GuardSpawn>(true))
            {
                guard.PatrolLeft *= Widen;
                guard.PatrolRight *= Widen;
            }
            Transform camera = root.Find("Camera");
            if (camera != null) camera.localPosition = new Vector3(camera.localPosition.x * Widen, camera.localPosition.y, camera.localPosition.z);
            Transform rain = root.Find("Rain");
            if (rain != null && rain.TryGetComponent(out ParticleSystem particles))
            {
                ParticleSystem.ShapeModule shape = particles.shape;
                shape.scale = new Vector3(shape.scale.x * Widen, shape.scale.y, shape.scale.z);
            }
            EditorUtility.SetDirty(level);
            return Reinforce(level, root);
        }

        private int Reinforce(LevelAuthoring level, Transform root)
        {
            Transform spawns = root.Find("Spawns");
            if (spawns == null) return 0;
            var guards = new List<GuardSpawn>(root.GetComponentsInChildren<GuardSpawn>(true));
            if (guards.Count == 0) return 0;
            PlayerSpawn player = root.GetComponentInChildren<PlayerSpawn>(true);
            Vector2 start = player != null ? (Vector2) player.transform.localPosition : new Vector2(-level.HalfWidth, 0f);
            var random = new System.Random(level.name.GetHashCode());
            int added = 0;
            foreach (LightRoomView room in root.GetComponentsInChildren<LightRoomView>(true))
            {
                float floor = Mathf.Round(room.transform.localPosition.y / level.FloorHeight) * level.FloorHeight;
                // The room the player starts in stays quiet.
                if (Mathf.Abs(start.y - floor) < 1f && start.x >= room.Left && start.x <= room.Right) continue;
                var inside = guards.FindAll(g => Mathf.Abs(g.transform.localPosition.y - floor) < 1f
                                                 && g.transform.localPosition.x >= room.Left && g.transform.localPosition.x <= room.Right);
                float width = room.Right - room.Left;
                if (width / (inside.Count + 1) < FloorPerGuard) continue;
                GuardSpawn model = inside.Count > 0 ? inside[random.Next(inside.Count)] : guards[random.Next(guards.Count)];
                // A clear spot: away from the others already here.
                float x = room.Left + width * 0.5f;
                for (int tries = 0; tries < 8; tries++)
                {
                    float candidate = room.Left + 1f + (float) random.NextDouble() * Mathf.Max(0.1f, width - 2f);
                    if (inside.TrueForAll(g => Mathf.Abs(g.transform.localPosition.x - candidate) > 1.8f)) { x = candidate; break; }
                }
                var spawn = Node("Spawn extra " + added, spawns, new Vector2(x, floor)).AddComponent<GuardSpawn>();
                spawn.CharacterType = model.CharacterType;
                spawn.Weapon = model.Weapon;
                spawn.Toughness = model.Toughness;
                spawn.Aggression = model.Aggression;
                spawn.Facing = random.Next(2) == 0 ? -1 : 1;
                spawn.PatrolLeft = Mathf.Max(room.Left + 0.6f, x - 2.5f);
                spawn.PatrolRight = Mathf.Min(room.Right - 0.6f, x + 2.5f);
                guards.Add(spawn);
                added++;
            }
            return added;
        }
    }
}
#endif
