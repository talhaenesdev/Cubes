#if UNITY_EDITOR
using FlowIoC.ConsoleModule;
using UnityEditor;

namespace Modules.GameplayModule.Editor
{
    // After a compile, writes whatever Night Shift asset is missing or out of date, the way Build missing does,
    // so a code change that adds a fixture shows up without a trip to the panel. Nothing that exists is overwritten.
    // A compile that happened on the way into Play is caught up when Play ends - assets are never written mid-Play.
    [InitializeOnLoad]
    public static class NightShiftAssetsAutoBuild
    {
        static NightShiftAssetsAutoBuild()
        {
            EditorApplication.delayCall += Run;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        private static void OnPlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += Run;
        }

        private static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            // Still importing or compiling: try again on the next editor tick rather than giving up.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Run;
                return;
            }
            var builder = new NightShiftAssetBuilder();
            if (!builder.Exists(NightShiftAssetBuilder.RootPrefab) || !builder.HasWork()) return;
            builder.Build(false);
            FlowLogger.LogWarning("Night Shift assets brought up to date:\n" + string.Join("\n", builder.Report));
        }
    }
}
#endif
