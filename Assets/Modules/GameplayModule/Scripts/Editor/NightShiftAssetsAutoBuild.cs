#if UNITY_EDITOR
using FlowIoC.ConsoleModule;
using UnityEditor;

namespace Modules.GameplayModule.Editor
{
    // After a compile, writes whatever Night Shift asset is missing or out of date, the way Build missing does,
    // so a code change that adds a fixture shows up without a trip to the panel. Nothing that exists is overwritten.
    [InitializeOnLoad]
    public static class NightShiftAssetsAutoBuild
    {
        static NightShiftAssetsAutoBuild() => EditorApplication.delayCall += Run;

        private static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            var builder = new NightShiftAssetBuilder();
            if (!builder.Exists(NightShiftAssetBuilder.RootPrefab) || !builder.HasWork()) return;
            builder.Build(false);
            FlowLogger.Log("Night Shift assets brought up to date:\n" + string.Join("\n", builder.Report));
        }
    }
}
#endif
