#if UNITY_EDITOR
using FlowIoC.Editor.ModulePanels;
using UnityEditor;

namespace Modules.GameplayModule.Editor
{
    // Builds the prefabs, art and pool group Night Shift is drawn with, and says what is already there.
    public class NightShiftAssetsPanel : ModulePanel
    {
        private readonly NightShiftAssetBuilder _builder = new();

        public override string Title => "Night Shift Assets";
        public override string Module => "GameplayModule";
        public override string Subtitle => "Prefabs, pool group and level the run is drawn with";

        [MenuItem("Tools/FlowIoC-Modules/Gameplay/Night Shift Assets")]
        private static void Open() => ModulePanelWindow.Open<NightShiftAssetsPanel>();

        public override void Draw(ModulePanelPainter painter)
        {
            painter.Heading("Status");
            painter.Field("Level prefab", State(NightShiftAssetBuilder.LevelPrefab));
            painter.Field("Pool group", State(NightShiftAssetBuilder.PoolAsset));
            painter.Field("Root", NightShiftAssetBuilder.RootPrefab);
            painter.Note("Build missing writes only what does not exist yet, so prefabs you have edited are kept. " +
                         "It also lists Pool_Gameplay on GameplaySystemRoot and gives NightShiftView the level prefab.");
            painter.Actions(
                new ModulePanelAction("Build missing", () => _builder.Build(false)),
                new ModulePanelAction("Rebuild all", Rebuild, destructive: true));
            if (_builder.Report.Count == 0) return;
            painter.Heading("Last build");
            foreach (string line in _builder.Report) painter.Text(line);
        }

        private void Rebuild()
        {
            if (EditorUtility.DisplayDialog("Rebuild Night Shift assets",
                    "Every generated prefab, sprite, sound and the level prefab are written again. Changes you made to them are lost.",
                    "Rebuild", "Cancel"))
                _builder.Build(true);
        }

        private string State(string path) => _builder.Exists(path) ? path : "missing - press Build missing";
    }
}
#endif
