#if UNITY_EDITOR
using System.Collections.Generic;
using FlowIoC.Editor.ModulePanels;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // Places what a level is made of by clicking in the Scene view, inside PB_Level_NightShift's prefab mode:
    // pick a tool, click where it goes, and it lands on the floor below the click. Delete and move things the
    // usual way in the Hierarchy and Scene view; the list at the bottom says what the level still needs.
    public class NightShiftLevelEditorPanel : ModulePanel
    {
        private enum Tool
        {
            None = 0,
            PlayerSpawn = 1,
            Character = 2,
            Weapon = 3,
            Door = 4,
            Stairs = 5,
            Vent = 6,
            Computer = 7,
            Crate = 8,
            LightRoom = 9,
            Exit = 10,
            MissionBoard = 11,
            UpgradeBench = 12,
            Prop = 13,
        }

        private const string Fixtures = NightShiftAssetBuilder.ModuleRoot + "/Prefabs/Fixtures/";
        private Tool _tool;
        private int _character;
        private int _weapon;
        private VentView _pendingVent;
        private int _levelChoice;
        private int _propKind;

        public override string Title => "Level Editor";
        public override string Module => "GameplayModule";
        public override string Subtitle => "Click in the Scene view to place things in a level prefab";

        [MenuItem("Tools/FlowIoC-Modules/Gameplay/Level Editor")]
        private static void Open() => ModulePanelWindow.Open<NightShiftLevelEditorPanel>();

        public override void Draw(ModulePanelPainter painter)
        {
            LevelAuthoring level = OpenLevel();
            if (level == null)
            {
                painter.Note("A level is edited in its prefab. Open one, then pick a tool here and click in the Scene view.");
                string[] levels = LevelPaths();
                string[] names = new string[levels.Length];
                for (int i = 0; i < levels.Length; i++) names[i] = System.IO.Path.GetFileNameWithoutExtension(levels[i]);
                _levelChoice = painter.Popup("Level", Mathf.Clamp(_levelChoice, 0, Mathf.Max(0, levels.Length - 1)), names,
                    "The hideout and every contract level CD_Missions lists.");
                painter.Actions(new ModulePanelAction("Open level", () =>
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(levels[_levelChoice])), levels.Length > 0));
                Disarm();
                return;
            }

            painter.Heading("Place");
            painter.Field("Tool", _tool == Tool.None ? "none - pick one below" : _tool + (_pendingVent != null ? " - click where the second vent goes" : " - click in the Scene view, Esc to stop"));
            painter.Actions(Pick("Player spawn", Tool.PlayerSpawn), Pick("Character", Tool.Character), Pick("Weapon", Tool.Weapon), Pick("Exit", Tool.Exit));
            painter.Actions(Pick("Door", Tool.Door), Pick("Stairs (pair)", Tool.Stairs), Pick("Vent (pair)", Tool.Vent), Pick("Computer", Tool.Computer));
            painter.Actions(Pick("Crate", Tool.Crate), Pick("Light room", Tool.LightRoom), Pick("Mission board", Tool.MissionBoard), Pick("Upgrade bench", Tool.UpgradeBench));
            painter.Actions(Pick("Breakable prop", Tool.Prop), new ModulePanelAction("Stop", Disarm, _tool != Tool.None));
            _propKind = painter.Popup("Prop", _propKind, System.Enum.GetNames(typeof(Enums.PropKind)),
                "What the Breakable prop tool places. A lamp hangs from the ceiling of the floor you click; a barrel explodes.");

            string[] characters = CharacterIds();
            string[] weapons = WeaponIds();
            _character = painter.Popup("Character type", Mathf.Clamp(_character, 0, Mathf.Max(0, characters.Length - 1)), characters,
                "Which CD_Characters entry the Character tool places. Friend or enemy is the type's Faction.");
            _weapon = painter.Popup("Weapon", Mathf.Clamp(_weapon, 0, Mathf.Max(0, weapons.Length - 1)), weapons,
                "What the Weapon tool lays down, and what a placed character carries (first entry: its type's own).");

            painter.Heading("Level");
            painter.Field("Difficulty", level.Difficulty + "  (set on the level's LevelAuthoring; per enemy: Toughness, Aggression on its spawn)");
            painter.Field("Camera", level.FollowCamera ? "follows the player" : "frames the whole level");
            painter.Field("Props", Count<PropView>(level) + "");
            painter.Field("Characters", Count<GuardSpawn>(level) + "");
            painter.Field("Weapons on the floor", Count<WeaponSpawn>(level) + "");
            painter.Field("Doors / stairs / vents", Count<DoorView>(level) + " / " + Count<StairDoorView>(level) + " / " + Count<VentView>(level));
            painter.Field("Computers / crates / rooms", Count<TerminalView>(level) + " / " + Count<CoverView>(level) + " / " + Count<LightRoomView>(level));
            foreach (string problem in Problems(level)) painter.Warning(problem);
        }

        private ModulePanelAction Pick(string label, Tool tool) => new(label, () => Arm(tool), _tool != tool);

        private void Arm(Tool tool)
        {
            _tool = tool;
            _pendingVent = null;
            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;
            SceneView.lastActiveSceneView?.Focus();
        }

        private void Disarm()
        {
            _tool = Tool.None;
            _pendingVent = null;
            SceneView.duringSceneGui -= OnScene;
        }

        private LevelAuthoring OpenLevel()
        {
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null) return null;
            return stage.prefabContentsRoot.GetComponent<LevelAuthoring>();
        }

        // ---------------------------------------------------------------- placing

        private void OnScene(SceneView view)
        {
            LevelAuthoring level = OpenLevel();
            if (level == null || _tool == Tool.None) { Disarm(); return; }
            Event e = Event.current;
            if (e.type == EventType.Layout) HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { Disarm(); e.Use(); return; }
            if (e.type != EventType.MouseDown || e.button != 0 || e.alt) return;
            Vector3 world = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            Vector3 local = level.transform.InverseTransformPoint(world);
            float floor = Mathf.Clamp(Mathf.Floor((local.y + 0.5f) / level.FloorHeight), 0f, 1f) * level.FloorHeight;
            float x = Mathf.Clamp(Mathf.Round(local.x * 10f) / 10f, -level.HalfWidth + 0.5f, level.HalfWidth - 0.5f);
            Place(level, new Vector2(x, floor));
            EditorSceneManager.MarkSceneDirty(PrefabStageUtility.GetCurrentPrefabStage().scene);
            e.Use();
        }

        private void Place(LevelAuthoring level, Vector2 at)
        {
            Transform fixtures = Group(level, "Fixtures");
            Transform spawns = Group(level, "Spawns");
            float other = at.y > 0.1f ? 0f : level.FloorHeight;
            switch (_tool)
            {
                case Tool.PlayerSpawn:
                    Move<PlayerSpawn>(level, spawns, "Player Spawn", at);
                    break;
                case Tool.Exit:
                    ExitView exit = level.GetComponentInChildren<ExitView>(true);
                    if (exit != null) { Undo.RecordObject(exit.transform, "Move exit"); exit.transform.localPosition = at; Selection.activeObject = exit.gameObject; }
                    else Prefab("PB_Exit", fixtures, at);
                    break;
                case Tool.Character:
                    string[] types = CharacterIds();
                    string[] arms = WeaponIds();
                    var guard = Marker<GuardSpawn>(spawns, "Spawn " + types[_character], at);
                    guard.CharacterType = types[_character];
                    guard.Weapon = _weapon == 0 ? "" : arms[_weapon];
                    guard.PatrolLeft = at.x - 2f;
                    guard.PatrolRight = at.x + 2f;
                    break;
                case Tool.Weapon:
                    string[] ids = WeaponIds();
                    string id = _weapon == 0 && ids.Length > 1 ? ids[1] : ids[_weapon];
                    Marker<WeaponSpawn>(spawns, "Weapon " + id, at).Weapon = id;
                    break;
                case Tool.Door: Prefab("PB_Door", fixtures, at); break;
                case Tool.Computer: Prefab("PB_Computer", fixtures, at); break;
                case Tool.Crate: Prefab("PB_Crate", fixtures, at); break;
                case Tool.MissionBoard: Prefab("PB_MissionBoard", fixtures, at); break;
                case Tool.UpgradeBench: Prefab("PB_UpgradeBench", fixtures, at); break;
                case Tool.Prop: Prefab("PB_Prop_" + (Enums.PropKind) _propKind, fixtures, at); break;
                case Tool.LightRoom:
                    var room = Prefab("PB_LightSwitch", fixtures, at)?.GetComponent<LightRoomView>();
                    if (room != null) { room.Left = Mathf.Max(-level.HalfWidth, at.x - 5f); room.Right = Mathf.Min(level.HalfWidth, at.x + 5f); }
                    break;
                case Tool.Stairs:
                    var up = Prefab("PB_StairDoor", fixtures, at)?.GetComponent<StairDoorView>();
                    var down = Prefab("PB_StairDoor", fixtures, new Vector2(at.x, other))?.GetComponent<StairDoorView>();
                    if (up != null && down != null) { up.Link = down; down.Link = up; }
                    break;
                case Tool.Vent:
                    var vent = Prefab("PB_Vent", fixtures, at)?.GetComponent<VentView>();
                    if (vent == null) break;
                    if (_pendingVent == null) { _pendingVent = vent; return; }
                    vent.Link = _pendingVent;
                    _pendingVent.Link = vent;
                    _pendingVent = null;
                    break;
            }
        }

        private GameObject Prefab(string name, Transform parent, Vector2 at)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Fixtures + name + ".prefab");
            if (prefab == null)
            {
                FlowIoC.ConsoleModule.FlowLogger.LogWarning(name + " is missing; run Tools/FlowIoC-Modules/Gameplay/Night Shift Assets first.");
                return null;
            }
            var made = (GameObject) PrefabUtility.InstantiatePrefab(prefab, parent);
            made.transform.localPosition = at;
            Undo.RegisterCreatedObjectUndo(made, "Place " + name);
            Selection.activeObject = made;
            return made;
        }

        private T Marker<T>(Transform parent, string name, Vector2 at) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            Undo.RegisterCreatedObjectUndo(go, "Place " + name);
            Selection.activeObject = go;
            return go.AddComponent<T>();
        }

        private void Move<T>(LevelAuthoring level, Transform parent, string name, Vector2 at) where T : Component
        {
            var existing = level.GetComponentInChildren<T>(true);
            if (existing == null) { Marker<T>(parent, name, at); return; }
            Undo.RecordObject(existing.transform, "Move " + name);
            existing.transform.localPosition = at;
            Selection.activeObject = existing.gameObject;
        }

        private Transform Group(LevelAuthoring level, string name)
        {
            Transform group = level.transform.Find(name);
            if (group != null) return group;
            var go = new GameObject(name);
            go.transform.SetParent(level.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Add " + name);
            return go.transform;
        }

        // ---------------------------------------------------------------- checks

        private int Count<T>(LevelAuthoring level) where T : Component => level.GetComponentsInChildren<T>(true).Length;

        private IEnumerable<string> Problems(LevelAuthoring level)
        {
            if (Count<PlayerSpawn>(level) == 0) yield return "No player spawn - the player starts at the default spot.";
            if (Count<PlayerSpawn>(level) > 1) yield return "More than one player spawn - only the first is used.";
            bool hideout = Count<StationView>(level) > 0;
            if (!hideout && Count<ExitView>(level) == 0) yield return "No exit - the run cannot be finished.";
            var characters = AssetDatabase.LoadAssetAtPath<CD_Characters>(NightShiftAssetBuilder.CharactersAsset);
            var weapons = AssetDatabase.LoadAssetAtPath<CD_Weapons>(NightShiftAssetBuilder.WeaponsAsset);
            int enemies = 0;
            foreach (GuardSpawn spawn in level.GetComponentsInChildren<GuardSpawn>(true))
            {
                CharacterTypeCVO type = characters != null ? characters.Find(spawn.CharacterType) : null;
                if (type == null) yield return spawn.name + " names character '" + spawn.CharacterType + "', which CD_Characters does not list.";
                else if (type.Faction == Enums.Faction.Enemy) enemies++;
                if (!string.IsNullOrEmpty(spawn.Weapon) && weapons != null && weapons.Find(spawn.Weapon) == null)
                    yield return spawn.name + " carries weapon '" + spawn.Weapon + "', which CD_Weapons does not list.";
            }
            if (!hideout && enemies == 0) yield return "No enemies - the exit is open from the start.";
            foreach (WeaponSpawn spawn in level.GetComponentsInChildren<WeaponSpawn>(true))
                if (weapons != null && weapons.Find(spawn.Weapon) == null)
                    yield return spawn.name + " names weapon '" + spawn.Weapon + "', which CD_Weapons does not list.";
            foreach (VentView vent in level.GetComponentsInChildren<VentView>(true))
                if (vent.Link == null || vent.Link.Link != vent) yield return vent.name + " is not linked both ways to another vent.";
            foreach (StairDoorView stair in level.GetComponentsInChildren<StairDoorView>(true))
                if (stair.Link == null || stair.Link.Link != stair) yield return stair.name + " is not linked both ways to a stairwell door on the other floor.";
        }

        private string[] LevelPaths()
        {
            var paths = new List<string>();
            var missions = AssetDatabase.LoadAssetAtPath<CD_Missions>(NightShiftAssetBuilder.MissionsAsset);
            if (missions != null && missions.Hideout != null) paths.Add(AssetDatabase.GetAssetPath(missions.Hideout));
            if (missions != null)
                foreach (MissionCVO mission in missions.Missions)
                    if (mission.Level != null && !paths.Contains(AssetDatabase.GetAssetPath(mission.Level))) paths.Add(AssetDatabase.GetAssetPath(mission.Level));
            if (!paths.Contains(NightShiftAssetBuilder.LevelPrefab)) paths.Add(NightShiftAssetBuilder.LevelPrefab);
            return paths.ToArray();
        }

        private string[] CharacterIds()
        {
            var config = AssetDatabase.LoadAssetAtPath<CD_Characters>(NightShiftAssetBuilder.CharactersAsset);
            var ids = new List<string>();
            if (config != null) foreach (CharacterTypeCVO type in config.Types) ids.Add(type.Id);
            if (ids.Count == 0) ids.Add("triangle");
            return ids.ToArray();
        }

        // The first entry means "the character type's own weapon".
        private string[] WeaponIds()
        {
            var config = AssetDatabase.LoadAssetAtPath<CD_Weapons>(NightShiftAssetBuilder.WeaponsAsset);
            var ids = new List<string> {"(type default)"};
            if (config != null) foreach (WeaponCVO weapon in config.Weapons) ids.Add(weapon.Id);
            return ids.ToArray();
        }
    }
}
#endif
