#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using FlowIoC.BaseModule.Root;
using FlowIoC.PoolModule.Data.UnityObjects;
using FlowIoC.PoolModule.Data.ValueObjects;
using FlowIoC.PoolModule.RootsContexts;
using Modules.GameplayModule.Constants;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.ViewsMediators;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // Writes every asset Night Shift is drawn with: sprites, sounds, the particle material, the character,
    // weapon, fixture, bullet, effect and decal prefabs, the level prefab, Pool_Gameplay, and the two
    // references GameplaySystemRoot needs. An asset that already exists is left alone unless overwrite is
    // asked for, so what the developer changed in a prefab survives the next build.
    public partial class NightShiftAssetBuilder
    {
        public const string ModuleRoot = "Assets/Modules/GameplayModule";
        private const string Art = ModuleRoot + "/Art";
        private const string Prefabs = ModuleRoot + "/Prefabs";
        public const string RootPrefab = Prefabs + "/GameplaySystemRoot.prefab";
        public const string PoolAsset = ModuleRoot + "/Scriptables/Pool_Gameplay.asset";
        public const string LevelPrefab = Prefabs + "/Level/PB_Level_NightShift.prefab";

        private readonly Color _ink = new(0.025f, 0.035f, 0.055f);
        private readonly Color _steel = new(0.20f, 0.29f, 0.34f);
        private readonly Color _teal = new(0.30f, 0.90f, 0.83f);
        private readonly Color _amber = new(1f, 0.65f, 0.30f);
        private readonly Color _red = new(0.95f, 0.22f, 0.30f);
        private readonly Color _skin = new(0.79f, 0.84f, 0.79f);

        private bool _overwrite;
        private Sprite _pixel;
        private Sprite _soft;
        private Material _particle;
        private Font _font;
        private readonly List<string> _report = new();

        public IReadOnlyList<string> Report => _report;

        public void Build(bool overwrite)
        {
            _overwrite = overwrite;
            _report.Clear();
            Folders();
            BuildArt();
            BuildShapes();
            BuildWeapons();
            BuildShapeCharacters();
            PhoneForCharacters();
            BuildMusic();
            BuildFixtures();
            BuildBullet();
            BuildEffects();
            BuildDecals();
            BuildConfigs();
            BuildStations();
            BuildCassettePlayer();
            BuildProps();
            BuildLevel();
            BuildHideout();
            DeckForHideout();
            BuildHome();
            TakeDownPictures();
            IconsForHideout();
            BuildDocks();
            BuildCampaign();
            RegisterDocks();
            BuildContracts();
            BuildDocuments();
            MakeLevelsSpacious();
            RemoveVents();
            PutVentsBack();
            BuildSounds();
            BuildMood();
            RetireLadder();
            BuildCash();
            BuildPool();
            RetireHumanoids();
            WireRoot();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public bool Exists(string path) => AssetDatabase.LoadAssetAtPath<Object>(path) != null;

        // True when Build missing would write something: an asset is missing or the level is from an earlier build.
        public bool HasWork()
        {
            foreach (string path in new[]
                     {
                         LevelPrefab, PoolAsset, WeaponsAsset, CharactersAsset, Pickup, WeaponPath("revolver"), CharacterPath(Enums.CharacterShape.Square), Door, Cover, Vent, LightSwitch, StairDoor, Computer, Exit,
                         Prefabs + "/Projectiles/PB_Bullet.prefab", Prefabs + "/FX/PB_FX_Muzzle.prefab",
                         Prefabs + "/Decals/PB_Decal_Blood.prefab", Prefabs + "/Decals/PB_Decal_BulletHole.prefab",
                     })
                if (!Exists(path)) return true;
            foreach (string path in Retired)
                if (Exists(path)) return true;
            foreach (CharacterShape shape in System.Enum.GetValues(typeof(CharacterShape)))
                if (!Exists(CharacterPath(shape)) || NeedsRestyle(CharacterPath(shape)) || NeedsPhone(CharacterPath(shape))) return true;
            if (!Exists(CassettePlayer) || HideoutNeedsDeck()) return true;
            if (!Exists(GameplayAsset) || ContractsMissing() || DocumentsMissing() || SoundsMissing() || !Exists(CashPrefab) || LevelsNeedSpace()) return true;
            if (!Exists(OutfitsAsset) || !Exists(Wardrobe) || !Exists(WeaponLocker) || !Exists(Cat) || HideoutNeedsHome()) return true;
            if (NeedsRestyle(CharacterPath(CharacterShape.Rectangle)) || HideoutHasPictures() || NeedsIcons()) return true;
            var outfits = AssetDatabase.LoadAssetAtPath<Data.UnityObjects.CD_Outfits>(OutfitsAsset);
            if (outfits != null && outfits.Find("engagement") == null) return true;
            var cast = AssetDatabase.LoadAssetAtPath<Data.UnityObjects.CD_Characters>(CharactersAsset);
            if (cast != null && cast.Find("rectangle") == null) return true;
            if (!Exists(MusicAsset) || !AssetDatabase.IsValidFolder(HideoutMusicFolder) || !AssetDatabase.IsValidFolder(ContractMusicFolder)) return true;
            foreach (string path in new[] {HideoutPrefab, MissionsAsset, UpgradesAsset, MissionBoard, UpgradeBench, DocksPrefab,
                         Prefabs + "/FX/PB_FX_Explosion.prefab", Prefabs + "/Decals/PB_Decal_Scorch.prefab"})
                if (!Exists(path)) return true;
            foreach (PropKind kind in System.Enum.GetValues(typeof(PropKind)))
                if (!Exists(PropPath(kind))) return true;
            var missions = AssetDatabase.LoadAssetAtPath<Data.UnityObjects.CD_Missions>(MissionsAsset);
            if (missions != null && missions.Find("docks_massacre") == null) return true;
            var pool = AssetDatabase.LoadAssetAtPath<CD_PoolGroup>(PoolAsset);
            if (pool != null && pool.Items.Find(item => item.PoolKey == NightShiftPoolKeys.Explosion) == null) return true;
            return NeedsUpgrade(false) || Exists(RetiredLadder) || HasMoodWork();
        }

        private void Folders()
        {
            foreach (string folder in new[]
                     {
                         Art, Art + "/Textures", Art + "/Materials", Art + "/Audio", ModuleRoot + "/Scriptables",
                         Prefabs + "/Characters", Prefabs + "/Weapons", Prefabs + "/Fixtures", Prefabs + "/Level",
                         Prefabs + "/FX", Prefabs + "/Decals", Prefabs + "/Projectiles",
                     })
                Folder(folder);
        }

        private void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (parent != null && !AssetDatabase.IsValidFolder(parent)) Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // ---------------------------------------------------------------- art

        private void BuildArt()
        {
            _pixel = SpriteAsset(Art + "/Textures/SPR_Pixel.png", 4, FilterMode.Point, (x, y, size) => Color.white);
            _soft = SpriteAsset(Art + "/Textures/SPR_SoftDot.png", 32, FilterMode.Bilinear, (x, y, size) =>
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                return new Color(1f, 1f, 1f, a * a);
            });
            string materialPath = Art + "/Materials/MT_FX_Particle.mat";
            _particle = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (_particle == null || _overwrite)
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Particles/Unlit");
                var material = new Material(shader) {name = "MT_FX_Particle", mainTexture = _soft.texture};
                Save(material, materialPath);
                _particle = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Sound("SND_SFX_Shot_01", 0.19f, 95f, 30f, 0);
            Sound("SND_SFX_GuardShot_01", 0.21f, 80f, 26f, 0);
            Sound("SND_SFX_ShellLoad_01", 0.12f, 1400f, 40f, 1);
            Sound("SND_SFX_Melee_01", 0.17f, 150f, 22f, 2);
            Sound("SND_SFX_DoorKick_01", 0.28f, 70f, 14f, 2);
        }

        private Sprite SpriteAsset(string path, int size, FilterMode filter, System.Func<int, int, int, Color> paint)
        {
            if (!Exists(path) || _overwrite)
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        texture.SetPixel(x, y, paint(x, y, size));
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = (TextureImporter) AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = size;
                importer.filterMode = filter;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                _report.Add("Wrote " + path);
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private AudioClip Sound(string name, float duration, float frequency, float decay, int kind)
        {
            string path = Art + "/Audio/" + name + ".wav";
            if (!Exists(path) || _overwrite)
            {
                const int rate = 22050;
                var samples = new float[(int) (rate * duration)];
                var random = new System.Random(93 + kind + (int) frequency);
                for (int i = 0; i < samples.Length; i++)
                {
                    float t = i / (float) rate;
                    float noise = (float) random.NextDouble() * 2f - 1f;
                    float envelope = Mathf.Exp(-t * decay) * Mathf.Min(1f, t * 1500f);
                    if (kind == 1) envelope *= Mathf.Pow(Mathf.Max(0f, Mathf.Cos(t * 62f)), 8f);
                    float tone = Mathf.Sin(2f * Mathf.PI * frequency * t * (1f - t * 1.8f));
                    samples[i] = (noise * (kind == 2 ? 0.3f : 0.65f) + tone * 0.35f) * envelope * 0.8f;
                }
                WriteWav(path, samples, rate);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                _report.Add("Wrote " + path);
            }
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private void WriteWav(string path, float[] samples, int rate)
        {
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            int bytes = samples.Length * 2;
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + bytes);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);
            writer.Write((short) 1);
            writer.Write((short) 1);
            writer.Write(rate);
            writer.Write(rate * 2);
            writer.Write((short) 2);
            writer.Write((short) 16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(bytes);
            foreach (float sample in samples) writer.Write((short) (Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
        }

        // ---------------------------------------------------------------- pool and root

        private void BuildPool()
        {
            var group = AssetDatabase.LoadAssetAtPath<CD_PoolGroup>(PoolAsset);
            if (group == null)
            {
                group = ScriptableObject.CreateInstance<CD_PoolGroup>();
                AssetDatabase.CreateAsset(group, PoolAsset);
                _report.Add("Wrote " + PoolAsset);
            }
            PoolItem(group, NightShiftPoolKeys.Pickup, Pickup, 12);
            PoolItem(group, NightShiftPoolKeys.Cash, CashPrefab, 12);
            PoolItem(group, NightShiftPoolKeys.Bullet, Prefabs + "/Projectiles/PB_Bullet.prefab", 16);
            PoolItem(group, NightShiftPoolKeys.Muzzle, Prefabs + "/FX/PB_FX_Muzzle.prefab", 4);
            PoolItem(group, NightShiftPoolKeys.GuardMuzzle, Prefabs + "/FX/PB_FX_GuardMuzzle.prefab", 4);
            PoolItem(group, NightShiftPoolKeys.ImpactWall, Prefabs + "/FX/PB_FX_ImpactWall.prefab", 8);
            PoolItem(group, NightShiftPoolKeys.ImpactBlood, Prefabs + "/FX/PB_FX_ImpactBlood.prefab", 6);
            PoolItem(group, NightShiftPoolKeys.ShellLoad, Prefabs + "/FX/PB_FX_ShellLoad.prefab", 3);
            PoolItem(group, NightShiftPoolKeys.Melee, Prefabs + "/FX/PB_FX_Melee.prefab", 2);
            PoolItem(group, NightShiftPoolKeys.DoorKick, Prefabs + "/FX/PB_FX_DoorKick.prefab", 2);
            PoolItem(group, NightShiftPoolKeys.BulletHole, Prefabs + "/Decals/PB_Decal_BulletHole.prefab", 40);
            PoolItem(group, NightShiftPoolKeys.BloodMark, Prefabs + "/Decals/PB_Decal_Blood.prefab", 30);
            PoolItem(group, NightShiftPoolKeys.Scorch, Prefabs + "/Decals/PB_Decal_Scorch.prefab", 6);
            PoolItem(group, NightShiftPoolKeys.CeilingDebris, Prefabs + "/FX/PB_FX_CeilingDebris.prefab", 4);
            PoolItem(group, NightShiftPoolKeys.FloorDust, Prefabs + "/FX/PB_FX_FloorDust.prefab", 4);
            PoolItem(group, NightShiftPoolKeys.Splinters, Prefabs + "/FX/PB_FX_Splinters.prefab", 4);
            PoolItem(group, NightShiftPoolKeys.Glass, Prefabs + "/FX/PB_FX_Glass.prefab", 4);
            PoolItem(group, NightShiftPoolKeys.MetalSparks, Prefabs + "/FX/PB_FX_MetalSparks.prefab", 4);
            PoolItem(group, NightShiftPoolKeys.Dirt, Prefabs + "/FX/PB_FX_Dirt.prefab", 2);
            PoolItem(group, NightShiftPoolKeys.Explosion, Prefabs + "/FX/PB_FX_Explosion.prefab", 4);
            EditorUtility.SetDirty(group);
        }

        // An entry the developer already tuned keeps its counts; only a missing prefab reference is filled.
        private void PoolItem(CD_PoolGroup group, string key, string prefabPath, int count)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            PoolItemCVO item = group.Items.Find(entry => entry.PoolKey == key);
            if (item == null)
            {
                item = new PoolItemCVO {PoolKey = key, InitialCreateCount = count, IsExtendable = true};
                group.Items.Add(item);
            }
            if (item.Prefab == null || _overwrite) item.Prefab = prefab;
        }

        private void WireRoot()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(RootPrefab);
            try
            {
                var root = contents.GetComponent<RootBase>();
                root.SubContextTypes ??= new List<SubContextData>();
                string poolContext = typeof(PoolSubContext).FullName;
                int index = root.SubContextTypes.FindIndex(entry => entry.ContextFullName == poolContext);
                var group = AssetDatabase.LoadAssetAtPath<CD_PoolGroup>(PoolAsset);
                if (index < 0)
                {
                    var settings = new PoolSubContextSettingsCVO {UnregisterWhenRootDestroyed = true};
                    settings.Groups.Add(NightShiftPoolKeys.Group, new PoolGroupCVO {Group = group, AutoInitialize = true});
                    root.SubContextTypes.Add(new SubContextData
                    {
                        ContextScript = Script(nameof(PoolSubContext)),
                        ContextFullName = poolContext,
                        ContextName = nameof(PoolSubContext),
                        AutoSetup = true,
                        Settings = settings,
                    });
                    _report.Add("Listed PoolSubContext (gameplay → Pool_Gameplay) on GameplaySystemRoot");
                }
                else if (root.SubContextTypes[index].Settings is PoolSubContextSettingsCVO settings
                         && !settings.Groups.ContainsKey(NightShiftPoolKeys.Group))
                {
                    settings.Groups.Add(NightShiftPoolKeys.Group, new PoolGroupCVO {Group = group, AutoInitialize = true});
                    _report.Add("Added the gameplay group to GameplaySystemRoot's PoolSubContext");
                }

                FileConfigs(contents);
                var view = contents.GetComponentInChildren<NightShiftView>(true);
                var level = AssetDatabase.LoadAssetAtPath<LevelAuthoring>(LevelPrefab);
                var serialized = new SerializedObject(view);
                SerializedProperty slot = serialized.FindProperty("_levelPrefab");
                if (slot.objectReferenceValue == null || _overwrite)
                {
                    slot.objectReferenceValue = level;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    _report.Add("Assigned PB_Level_NightShift to NightShiftView");
                }
                PrefabUtility.SaveAsPrefabAsset(contents, RootPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private Object Script(string className)
        {
            foreach (string guid in AssetDatabase.FindAssets(className + " t:MonoScript"))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                if (script != null && script.GetClass() != null && script.GetClass().Name == className) return script;
            }
            return null;
        }

        // ---------------------------------------------------------------- shared helpers

        private void Save(Object asset, string path)
        {
            if (Exists(path)) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
            _report.Add("Wrote " + path);
        }

        // True when the prefab should be (re)written.
        private bool ShouldWrite(string path) => _overwrite || !Exists(path);

        private T SavePrefab<T>(GameObject go, string path) where T : Object
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            _report.Add("Wrote " + path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private GameObject Node(string name, Transform parent, Vector2 position)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go;
        }

        // One flat-coloured rectangle, 1 unit = 1 world unit before scale.
        private SpriteRenderer Part(string name, Transform parent, Vector2 position, Vector2 size, Color color, int order, float angle = 0f)
        {
            GameObject go = Node(name, parent, position);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = _pixel;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void Line(string name, Transform parent, Vector2 from, Vector2 to, float thickness, Color color, int order)
        {
            Vector2 delta = to - from;
            Part(name, parent, (from + to) * 0.5f, new Vector2(delta.magnitude, thickness), color, order,
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private void Sign(string text, Transform parent, Vector2 position, float size, Color color)
        {
            GameObject go = Node("Sign " + text, parent, position);
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = _font;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontSize = 48;
            mesh.characterSize = size * 0.4f;
            mesh.color = color;
            var renderer = go.GetComponent<MeshRenderer>();
            if (_font != null) renderer.sharedMaterial = _font.material;
            renderer.sortingOrder = -5;
        }

        private void Assign(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignArray(Object target, string field, IReadOnlyList<Object> values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty array = serialized.FindProperty(field);
            array.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private AudioSource Audio(GameObject go, string clip, float volume)
        {
            var source = go.AddComponent<AudioSource>();
            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Art + "/Audio/" + clip + ".wav");
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }
    }
}
#endif
