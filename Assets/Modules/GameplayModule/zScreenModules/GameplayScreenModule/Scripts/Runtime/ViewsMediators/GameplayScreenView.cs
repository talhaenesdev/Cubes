using System;
using FlowIoC.BaseModule.Injectable.Components;
using FlowIoC.ScreenModule.ViewsMediators.Screen;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    [RequireComponent(typeof(ViewInjector))]
    public class GameplayScreenView : ScreenView
    {
        // Noir: ivory on near-black, gold for what matters, crimson for danger.
        private readonly Color _cream = new Color32(226, 214, 190, 255);
        private readonly Color _mint = new Color32(196, 160, 92, 255);
        private readonly Color _ink = new Color32(10, 8, 8, 242);
        private Font _font;
        private Text _status;
        private Text _objective;
        private Button _retryButton;
        private Button _menuButton;

        private GameObject _terminal;
        private Text _terminalTitle;
        private InputField _terminalText;
        private Button _terminalSave;
        private Button _terminalClose;

        public Action RetryClicked;
        public Action MenuClicked;
        public Action<string> TerminalSaveClicked;
        public Action TerminalCloseClicked;
        public Action<int> MenuEntryClicked;
        public Action MenuCloseClicked;

        private const int MenuRows = 8;
        private GameObject _menu;
        private Text _menuTitle;
        private Text _menuSubtitle;
        private readonly Button[] _menuButtons = new Button[MenuRows];
        private readonly Text[] _menuLabels = new Text[MenuRows];
        private Button _menuClose;

        private void Awake()
        {
            _font = Serif(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            CreatePanel("Header", transform, 0, .84f, 1, 1, _ink);
            CreatePanel("HeaderRule", transform, .025f, .84f, .975f, .843f, _mint);
            CreateText("Title", transform, "NIGHT SHIFT", .03f, .925f, .25f, .985f, 30, _cream);
            CreateText("Contract", transform, "CONTRACT 01  /  FIELD OPERATIONS", .03f, .864f, .27f, .92f, 14, _mint);
            _status = CreateText("Status", transform, "AWAITING CONTRACT", .29f, .925f, .80f, .985f, 23, _mint);
            _objective = CreateText("Objective", transform, "", .29f, .85f, .80f, .925f, 20, _cream);
            _retryButton = CreateButton("Retry", "RETRY", .835f, .925f, .975f, .98f);
            _menuButton = CreateButton("Menu", "MENU", .835f, .855f, .975f, .91f);

            CreatePanel("Controls", transform, 0, 0, 1, .105f, _ink);
            CreatePanel("ControlsRule", transform, .025f, .103f, .975f, .106f, new Color32(140, 22, 28, 255));
            CreateText("Movement", transform, "A/D  MOVE     SPACE  JUMP     CTRL  CROUCH     E  STAIRS / VENT / COMPUTER / STATION", .03f, .012f, .49f, .095f, 16, _cream);
            CreateText("Actions", transform, "LMB  FIRE   RMB/F  MELEE   E  USE/PICK UP   Q  SWAP\nR  RELOAD/RETRY   CTRL+E  GRAB WEAPON   ESC  PAUSE", .51f, .012f, .975f, .095f, 16, _cream);
            BuildTerminal();
            BuildMenu();
        }

        // A hideout station's list: a title, the player's cash, and one button per contract or upgrade.
        private void BuildMenu()
        {
            Image frame = CreatePanel("Station", transform, .22f, .14f, .78f, .82f, new Color32(14, 10, 10, 246));
            frame.raycastTarget = true;
            _menu = frame.gameObject;
            CreatePanel("Rule", frame.transform, 0f, .994f, 1f, 1f, _mint);
            _menuTitle = CreateText("Title", frame.transform, "", .04f, .9f, .7f, .97f, 26, _mint);
            _menuSubtitle = CreateText("Cash", frame.transform, "", .6f, .9f, .96f, .97f, 20, _cream);
            _menuSubtitle.alignment = TextAnchor.MiddleRight;
            for (int i = 0; i < MenuRows; i++)
            {
                float top = .88f - i * .095f;
                _menuButtons[i] = CreateButton("Entry " + i, "", .04f, top - .085f, .96f, top, frame.transform);
                _menuLabels[i] = _menuButtons[i].GetComponentInChildren<Text>();
                _menuLabels[i].alignment = TextAnchor.MiddleLeft;
                _menuLabels[i].fontSize = 16;
                _menuLabels[i].resizeTextMaxSize = 16;
                int index = i;
                _menuButtons[i].onClick.AddListener(() => MenuEntryClicked?.Invoke(index));
            }
            _menuClose = CreateButton("Close", "CLOSE", .78f, .03f, .96f, .1f, frame.transform);
            _menuClose.onClick.AddListener(() => MenuCloseClicked?.Invoke());
            _menu.SetActive(false);
        }

        public void ShowMenu(string title, string subtitle, string[] entries)
        {
            _menuTitle.text = title ?? string.Empty;
            _menuSubtitle.text = subtitle ?? string.Empty;
            for (int i = 0; i < MenuRows; i++)
            {
                bool used = entries != null && i < entries.Length;
                _menuButtons[i].gameObject.SetActive(used);
                if (used) _menuLabels[i].text = entries[i];
            }
            _menu.SetActive(true);
        }

        public void HideMenu() => _menu.SetActive(false);

        // The computer screen: its title, the text on it, and the player's edits to it.
        private void BuildTerminal()
        {
            Image frame = CreatePanel("Terminal", transform, .2f, .16f, .8f, .8f, new Color32(12, 9, 6, 246));
            frame.raycastTarget = true;
            _terminal = frame.gameObject;
            CreatePanel("Bezel", frame.transform, .0f, .0f, 1f, .006f, _mint);
            CreatePanel("BezelTop", frame.transform, .0f, .994f, 1f, 1f, _mint);
            _terminalTitle = CreateText("Title", frame.transform, "", .04f, .87f, .96f, .96f, 24, _mint);
            CreateText("Hint", frame.transform, "Type to edit.  SAVE keeps the text on this computer.  ESC closes.", .04f, .81f, .96f, .86f, 13, _cream);
            Image field = CreatePanel("Field", frame.transform, .04f, .16f, .96f, .8f, new Color32(24, 18, 12, 255));
            field.raycastTarget = true;
            Text body = CreateText("Text", field.transform, "", .02f, .02f, .98f, .98f, 20, new Color32(230, 170, 80, 255));
            body.alignment = TextAnchor.UpperLeft;
            body.resizeTextForBestFit = false;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Truncate;
            body.raycastTarget = true;
            _terminalText = field.gameObject.AddComponent<InputField>();
            _terminalText.textComponent = body;
            _terminalText.lineType = InputField.LineType.MultiLineNewline;
            _terminalText.targetGraphic = field;
            _terminalText.caretColor = _mint;
            _terminalText.customCaretColor = true;
            _terminalSave = CreateButton("Save", "SAVE", .56f, .035f, .76f, .12f, frame.transform);
            _terminalClose = CreateButton("Close", "CLOSE", .78f, .035f, .96f, .12f, frame.transform);
            _terminal.SetActive(false);
        }

        public void ShowTerminal(string title, string text)
        {
            _terminalTitle.text = title ?? string.Empty;
            _terminalText.text = text ?? string.Empty;
            _terminal.SetActive(true);
            _terminalText.ActivateInputField();
        }

        public void HideTerminal()
        {
            if (_terminalText.isFocused) _terminalText.DeactivateInputField();
            _terminal.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
            if (_terminal != null && _terminal.activeSelf) TerminalCloseClicked?.Invoke();
            else if (_menu != null && _menu.activeSelf) MenuCloseClicked?.Invoke();
        }

        private void OnEnable()
        {
            _retryButton.onClick.AddListener(OnRetryClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
            _terminalSave.onClick.AddListener(OnTerminalSave);
            _terminalClose.onClick.AddListener(OnTerminalClose);
        }

        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(OnRetryClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
            _terminalSave.onClick.RemoveListener(OnTerminalSave);
            _terminalClose.onClick.RemoveListener(OnTerminalClose);
        }

        public override void BeforeScreenActivation()
        {
            base.BeforeScreenActivation();
            ShowHud("AWAITING CONTRACT", "");
            HideTerminal();
            HideMenu();
        }

        public void ShowHud(string status, string objective)
        {
            _status.text = status ?? string.Empty;
            _objective.text = objective ?? string.Empty;
        }

        // A serif face from the machine when it has one - the look is a 1940s dossier - or the built-in font.
        private Font Serif(Font fallback)
        {
            Font serif = Font.CreateDynamicFontFromOSFont(new[] {"Georgia", "Times New Roman", "Garamond", "Palatino Linotype", "DejaVu Serif"}, 32);
            return serif != null ? serif : fallback;
        }

        private void OnRetryClicked() => RetryClicked?.Invoke();
        private void OnMenuClicked() => MenuClicked?.Invoke();
        private void OnTerminalSave() => TerminalSaveClicked?.Invoke(_terminalText.text);
        private void OnTerminalClose() => TerminalCloseClicked?.Invoke();

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

        private Button CreateButton(string objectName, string caption, float left, float bottom, float right, float top, Transform parent = null)
        {
            Image panel = CreatePanel(objectName, parent != null ? parent : transform, left, bottom, right, top, Color.white);
            panel.raycastTarget = true;
            Button button = panel.gameObject.AddComponent<Button>();
            button.targetGraphic = panel;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color32(40, 30, 28, 255);
            colors.highlightedColor = new Color32(96, 70, 40, 255);
            colors.pressedColor = new Color32(140, 22, 28, 255);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.navigation = new Navigation {mode = Navigation.Mode.None};
            CreateText("Label", panel.transform, caption, .08f, .05f, .92f, .95f, 20, _cream).alignment = TextAnchor.MiddleCenter;
            return button;
        }
    }
}
