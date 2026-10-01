using System;
using FlowIoC.BaseModule.Injectable.Components;
using FlowIoC.ScreenModule.ViewsMediators.Screen;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.MainModule.MainScreenModule.ViewsMediators
{
    [RequireComponent(typeof(ViewInjector))]
    public class MainScreenView : ScreenView
    {
        [SerializeField] private Button _playButton;

        private readonly Color _cream = new Color32(226, 214, 190, 255);
        private readonly Color _mint = new Color32(196, 160, 92, 255);
        private readonly Color _red = new Color32(140, 22, 28, 255);
        private Font _font;

        public Action PlayClicked;

        private void Awake()
        {
            // Replace the prefab's original placeholder visuals once per pooled instance.
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);

            _font = Serif(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            Image backdrop = CreatePanel("Night", transform, 0, 0, 1, 1, new Color32(8, 6, 6, 255));
            backdrop.raycastTarget = true;
            CreatePanel("District", transform, .58f, .18f, 1, 1, new Color32(22, 16, 15, 255));
            CreatePanel("Moonlight", transform, .79f, .62f, .92f, .90f, new Color32(150, 120, 70, 255));
            CreatePanel("MoonShadow", transform, .77f, .65f, .90f, .92f, new Color32(22, 16, 15, 255));

            for (int building = 0; building < 7; building++)
            {
                float left = .57f + building * .065f;
                float roof = .39f + (building % 3) * .09f;
                CreatePanel("Building" + building, transform, left, .18f, left + .057f, roof, new Color32(6, 5, 5, 255));
                for (int floor = 0; floor < 3; floor++)
                    CreatePanel("Window" + building + "_" + floor, transform, left + .014f, .25f + floor * .046f, left + .024f, .265f + floor * .046f, new Color32(120, 90, 45, 255));
            }

            CreatePanel("RedSpine", transform, .04f, .30f, .044f, .87f, _red);
            CreateText("Edition", transform, "DEAD OF NIGHT  /  A stealth-action prototype", .07f, .85f, .58f, .92f, 18, _mint);
            CreateText("Title", transform, "NIGHT\nSHIFT", .065f, .54f, .59f, .85f, 100, _cream).fontStyle = FontStyle.Bold;
            CreateText("Premise", transform, "The city sleeps. Your work begins.\nEnter the building. Finish the contract. Get out alive.", .07f, .405f, .55f, .53f, 23, _cream);
            _playButton = CreateButton("PlayContract", "PLAY CONTRACT   /   01", .07f, .29f, .43f, .38f);
            ColorBlock colors = _playButton.colors;
            colors.normalColor = _mint;
            colors.highlightedColor = _cream;
            colors.selectedColor = _cream;
            colors.pressedColor = _red;
            _playButton.colors = colors;
            _playButton.GetComponentInChildren<Text>().color = new Color32(8, 6, 6, 255);

            CreatePanel("ContractCard", transform, .64f, .29f, .94f, .56f, new Color32(24, 17, 15, 250));
            CreatePanel("ContractRule", transform, .64f, .55f, .94f, .555f, _red);
            CreateText("ContractNumber", transform, "01 / NIGHT OPERATIONS", .665f, .485f, .92f, .54f, 19, _mint);
            CreateText("ContractBrief", transform, "WATCH THE ANGLES.\nMAKE EVERY SHOT COUNT.", .665f, .375f, .92f, .48f, 24, _cream);
            CreateText("ContractFooter", transform, "ONE BUILDING. ONE WAY OUT.", .665f, .31f, .92f, .36f, 14, _mint);

            CreatePanel("Footer", transform, 0, 0, 1, .205f, new Color32(6, 4, 4, 255));
            CreatePanel("FooterRule", transform, .04f, .202f, .96f, .205f, new Color32(50, 67, 63, 255));
            CreateText("ControlsHeading", transform, "FIELD MANUAL", .07f, .145f, .32f, .19f, 15, _mint);
            CreateText("Movement", transform, "A/D  Move     SPACE  Jump\nW/S  Ladders     CTRL  Crouch", .07f, .04f, .34f, .14f, 18, _cream);
            CreateText("Combat", transform, "LMB  Fire     RMB/F  Melee\nE  Door / Exit", .37f, .04f, .66f, .14f, 18, _cream);
            CreateText("Session", transform, "R  Reload / Retry\nESC  Pause", .72f, .04f, .94f, .14f, 18, _cream);
        }

        private void OnEnable() => _playButton.onClick.AddListener(OnPlayClicked);
        private void OnDisable() => _playButton.onClick.RemoveListener(OnPlayClicked);
        private void OnPlayClicked() => PlayClicked?.Invoke();

        private RectTransform CreateRect(string objectName, Transform parent, float left, float bottom, float right, float top)
        {
            RectTransform rect = new GameObject(objectName, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer = gameObject.layer;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(left, bottom);
            rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private Image CreatePanel(string objectName, Transform parent, float left, float bottom, float right, float top, Color color)
        {
            Image panel = CreateRect(objectName, parent, left, bottom, right, top).gameObject.AddComponent<Image>();
            panel.color = color;
            panel.raycastTarget = false;
            return panel;
        }

        private Text CreateText(string objectName, Transform parent, string content, float left, float bottom, float right, float top, int size, Color color)
        {
            Text label = CreateRect(objectName, parent, left, bottom, right, top).gameObject.AddComponent<Text>();
            label.font = _font;
            label.text = content;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAnchor.MiddleLeft;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = size;
            label.supportRichText = false;
            label.raycastTarget = false;
            return label;
        }

        private Button CreateButton(string objectName, string caption, float left, float bottom, float right, float top)
        {
            Image panel = CreatePanel(objectName, transform, left, bottom, right, top, Color.white);
            panel.raycastTarget = true;
            Button button = panel.gameObject.AddComponent<Button>();
            button.targetGraphic = panel;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color32(42, 58, 56, 255);
            colors.highlightedColor = new Color32(77, 111, 98, 255);
            colors.pressedColor = new Color32(132, 55, 52, 255);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.navigation = new Navigation {mode = Navigation.Mode.None};
            CreateText("Label", panel.transform, caption, .08f, .05f, .92f, .95f, 20, _cream).alignment = TextAnchor.MiddleCenter;
            return button;
        }

        // A serif face from the machine when it has one - the look is a 1940s dossier - or the built-in font.
        private Font Serif(Font fallback)
        {
            Font serif = Font.CreateDynamicFontFromOSFont(new[] {"Georgia", "Times New Roman", "Garamond", "Palatino Linotype", "DejaVu Serif"}, 32);
            return serif != null ? serif : fallback;
        }
    }
}
