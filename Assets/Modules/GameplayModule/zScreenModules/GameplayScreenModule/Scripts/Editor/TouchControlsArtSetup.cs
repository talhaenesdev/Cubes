#if UNITY_EDITOR
using Modules.GameplayModule.GameplayScreenModule.ViewsMediators;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.GameplayModule.GameplayScreenModule.Editor
{
    // Puts the drawn touch controls on GameplayScreen: the stick's dial and knob, and each action button with its
    // pressed drawing. The flat circles, rims and letters they replace are switched off, not deleted.
    internal static class TouchControlsArtSetup
    {
        private const string Screen = "Assets/Modules/GameplayModule/zScreenModules/GameplayScreenModule/Prefabs/GameplayScreen.prefab";
        private const string Art = "Assets/Modules/GameplayModule/Art/Incoming/Touch/";

        // Button object in the screen -> drawing.
        private static readonly (string node, string art)[] Buttons =
        {
            ("Use", "Use"), ("Jump", "Jump"), ("Melee", "Melee"), ("Reload", "Reload"), ("Swap", "Swap"), ("Next Song", "Song"),
        };

        [MenuItem("Tools/FlowIoC-Modules/Gameplay/Apply Touch Controls Art")]
        public static void Apply() => Debug.Log(ApplyAll());

        public static string ApplyAll()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] {Art.TrimEnd('/')})) Import(AssetDatabase.GUIDToAssetPath(guid));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            GameObject root = PrefabUtility.LoadPrefabContents(Screen);
            string report = "";
            try
            {
                Transform touch = Find(root.transform, "Touch Controls");
                if (touch == null) return "no Touch Controls in GameplayScreen";

                Transform stick = touch.Find("Stick");
                if (stick != null)
                {
                    Off(stick, "Rim");
                    Paint(stick.Find("Ring"), Sprite("Stick_Base"));
                    Image knob = Paint(stick.Find("Knob"), Sprite("Stick_Knob"));
                    if (knob != null) Press(knob, Sprite("Stick_Knob"), Sprite("Stick_Knob_Pressed"));
                    report += "stick ";
                }
                foreach ((string node, string art) in Buttons)
                {
                    Transform button = touch.Find(node);
                    if (button == null) continue;
                    Off(button, "Rim");
                    Off(button, "Glyph");
                    Image face = Paint(button.Find("Face"), Sprite("Button_" + art));
                    if (face == null) continue;
                    Press(face, Sprite("Button_" + art), Sprite("Button_" + art + "_Pressed"));
                    report += node + " ";
                }
                PrefabUtility.SaveAsPrefabAsset(root, Screen);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return "Touch controls drawn: " + report;
        }

        private static void Import(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + "TOUCH_" + name + ".png");

        private static Image Paint(Transform node, Sprite sprite)
        {
            if (node == null || sprite == null) return null;
            var image = node.GetComponent<Image>();
            if (image == null) return null;
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            // The drawing carries its own see-through glass; the image adds no tint over it.
            image.color = Color.white;
            return image;
        }

        private static void Press(Image image, Sprite normal, Sprite pressed)
        {
            var swap = image.GetComponent<PressedSprite>() ?? image.gameObject.AddComponent<PressedSprite>();
            swap.Set(image, normal, pressed);
        }

        private static void Off(Transform parent, string child)
        {
            Transform t = parent.Find(child);
            if (t != null) t.gameObject.SetActive(false);
        }

        private static Transform Find(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }
    }
}
#endif
