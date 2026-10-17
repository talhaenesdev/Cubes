#if UNITY_EDITOR
using Modules.GameplayModule.GameplayScreenModule.ViewsMediators;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.GameplayModule.GameplayScreenModule.Editor
{
    // Creates each editable panel once and wires it to GameplayScreen. Existing panel prefabs are never overwritten.
    internal sealed class GameplayScreenPanelPrefabBuilder
    {
        private const string Module = "Assets/Modules/GameplayModule/zScreenModules/GameplayScreenModule";
        private const string Folder = Module + "/Prefabs/Panels";
        private const string Screen = Module + "/Prefabs/GameplayScreen.prefab";

        private readonly Panel[] _panels =
        {
            new("_headerPrefab", "PB_UI_GameplayHeader", 0f, .84f, 1f, 1f, new Color32(10, 8, 8, 242)),
            new("_terminalPrefab", "PB_UI_TerminalPanel", .2f, .16f, .8f, .8f, new Color32(12, 9, 6, 246)),
            new("_menuPrefab", "PB_UI_StationPanel", .22f, .14f, .78f, .82f, new Color32(14, 10, 10, 246)),
            new("_cityMapPrefab", "PB_UI_CityMapPanel", .02f, .1f, .98f, .885f, Color.clear),
            new("_nowPlayingPrefab", "PB_UI_NowPlayingPanel", .7f, .125f, .985f, .3f, Color.clear),
            new("_deckPrefab", "PB_UI_TapeDeckPanel", .2f, .2f, .8f, .78f, new Color32(18, 15, 14, 250)),
            new("_touchControlsPrefab", "PB_UI_TouchControlsPanel", 0f, 0f, 1f, .84f, Color.clear),
            new("_phoneCallPrefab", "PB_UI_PhoneCallPanel", .18f, .04f, .82f, .25f, new Color32(16, 12, 11, 245)),
            new("_bannerPrefab", "PB_UI_BannerPanel", 0f, .55f, 1f, .7f, Color.clear),
            new("_menuRowPrefab", "PB_UI_MenuRow", 0f, 1f, 1f, 1f, new Color32(40, 30, 28, 255), true),
            new("_mapPinPrefab", "PB_UI_MapPin", .5f, .5f, .5f, .5f, Color.clear, true),
        };

        [InitializeOnLoadMethod]
        private static void Schedule() => EditorApplication.delayCall += Build;

        private static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            new GameplayScreenPanelPrefabBuilder().Execute();
        }

        private void Execute()
        {
            EnsureFolder();
            GameObject screen = AssetDatabase.LoadAssetAtPath<GameObject>(Screen);
            if (screen == null) return;
            GameplayScreenView view = screen.GetComponent<GameplayScreenView>();
            if (view == null) return;

            var serialized = new SerializedObject(view);
            bool changed = false;
            foreach (Panel panel in _panels)
            {
                string path = Folder + "/" + panel.Name + ".prefab";
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null) asset = Create(panel, path);
                SerializedProperty property = serialized.FindProperty(panel.Field);
                if (property == null || property.objectReferenceValue != null) continue;
                property.objectReferenceValue = asset;
                changed = true;
            }
            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(screen);
                AssetDatabase.SaveAssets();
            }
            BakeScreenLayout();
            BakeDynamicTemplates();
        }

        private static void BakeScreenLayout()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(Screen);
            try
            {
                if (contents.transform.childCount > 0) return;
                GameplayScreenView view = contents.GetComponent<GameplayScreenView>();
                view.BuildLayout();
                PrefabUtility.SaveAsPrefabAsset(contents, Screen);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void BakeDynamicTemplates()
        {
            string rowPath = Folder + "/PB_UI_MenuRow.prefab";
            string pinPath = Folder + "/PB_UI_MapPin.prefab";
            GameObject rowAsset = AssetDatabase.LoadAssetAtPath<GameObject>(rowPath);
            GameObject pinAsset = AssetDatabase.LoadAssetAtPath<GameObject>(pinPath);
            bool rowMissing = rowAsset == null || rowAsset.transform.childCount == 0;
            bool pinMissing = pinAsset == null || pinAsset.transform.childCount == 0;
            if (!rowMissing && !pinMissing) return;

            GameObject contents = PrefabUtility.LoadPrefabContents(Screen);
            try
            {
                GameplayScreenView view = contents.GetComponent<GameplayScreenView>();
                if (rowMissing)
                {
                    GameObject row = view.BuildMenuRowTemplate();
                    PrefabUtility.SaveAsPrefabAsset(row, rowPath);
                }
                if (pinMissing)
                {
                    GameObject pin = view.BuildMapPinTemplate();
                    PrefabUtility.SaveAsPrefabAsset(pin, pinPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static GameObject Create(Panel panel, string path)
        {
            var root = new GameObject(panel.Name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(panel.Left, panel.Bottom);
            rect.anchorMax = new Vector2(panel.Right, panel.Top);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = root.GetComponent<Image>();
            image.color = panel.Color;
            image.raycastTarget = panel.Button;
            if (panel.Button)
            {
                Button button = root.AddComponent<Button>();
                button.targetGraphic = image;
                button.navigation = new Navigation {mode = Navigation.Mode.None};
            }
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void EnsureFolder()
        {
            if (AssetDatabase.IsValidFolder(Folder)) return;
            AssetDatabase.CreateFolder(Module + "/Prefabs", "Panels");
        }

        private readonly struct Panel
        {
            public readonly string Field;
            public readonly string Name;
            public readonly float Left;
            public readonly float Bottom;
            public readonly float Right;
            public readonly float Top;
            public readonly Color Color;
            public readonly bool Button;

            public Panel(string field, string name, float left, float bottom, float right, float top, Color color, bool button = false)
            {
                Field = field;
                Name = name;
                Left = left;
                Bottom = bottom;
                Right = right;
                Top = top;
                Color = color;
                Button = button;
            }
        }
    }
}
#endif
