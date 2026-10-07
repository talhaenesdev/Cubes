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

        private Button _languageButton;
        // Every text on the screen, by the key it is translated from.
        private readonly System.Collections.Generic.List<(Text text, string key)> _texts = new();

        public Action PlayClicked;
        public Action LanguageClicked;
        public Action SettingsClicked;
        public Action SettingsCloseClicked;
        public Action<float> MasterVolumeChanged;
        public Action<float> MusicVolumeChanged;

        private Button _settingsButton;
        // The title screen's parts, hidden when the menu opens over a paused game, and the pause panel shown instead.
        private readonly System.Collections.Generic.List<GameObject> _titleScreen = new();
        private GameObject _pause;
        private GameObject _credits;
        private Button _creditsButton;
        private AudioSource _ui;
        private AudioClip _click;
        private AudioClip _confirm;
        private GameObject _settings;
        private Slider _masterSlider;
        private Slider _musicSlider;

        private void Awake()
        {
            // Replace the prefab's original placeholder visuals once per pooled instance.
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);

            _font = Serif(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            _ui = gameObject.AddComponent<AudioSource>();
            _ui.playOnAwake = false;
            _ui.ignoreListenerPause = true;
            _click = Resources.Load<AudioClip>("SND_UI_MainClick");
            _confirm = Resources.Load<AudioClip>("SND_UI_MainConfirm");
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
            Keyed(CreateText("Edition", transform, "", .07f, .85f, .58f, .92f, 18, _mint), "DEAD OF NIGHT  /  A stealth-action prototype");
            CreateText("Title", transform, "CUBES", .065f, .54f, .59f, .85f, 130, _cream).fontStyle = FontStyle.Bold;
            Keyed(CreateText("Premise", transform, "", .07f, .405f, .55f, .53f, 23, _cream), "The city sleeps. Your work begins.\nEnter the building. Finish the contract. Get out alive.");
            _playButton = CreateButton("PlayContract", "", .07f, .29f, .43f, .38f);
            Keyed(_playButton.GetComponentInChildren<Text>(), "PLAY");
            _languageButton = CreateButton("Language", "", .8f, .86f, .96f, .93f);
            Keyed(_languageButton.GetComponentInChildren<Text>(), "LANGUAGE: ENGLISH");
            // The settings icon: three slider bars with their knobs.
            _settingsButton = CreateButton("Settings", "", .745f, .86f, .79f, .93f);
            for (int bar = 0; bar < 3; bar++)
            {
                float y = .25f + bar * .25f;
                float knob = bar == 1 ? .3f : bar == 0 ? .65f : .5f;
                CreatePanel("Bar", _settingsButton.transform, .2f, y - .03f, .8f, y + .03f, _cream);
                CreatePanel("Knob", _settingsButton.transform, knob - .07f, y - .09f, knob + .07f, y + .09f, _mint);
            }
            ColorBlock colors = _playButton.colors;
            colors.normalColor = _mint;
            colors.highlightedColor = _cream;
            colors.selectedColor = _cream;
            colors.pressedColor = _red;
            _playButton.colors = colors;
            _playButton.GetComponentInChildren<Text>().color = new Color32(8, 6, 6, 255);

            CreatePanel("ContractCard", transform, .64f, .29f, .94f, .56f, new Color32(24, 17, 15, 250));
            CreatePanel("ContractRule", transform, .64f, .55f, .94f, .555f, _red);
            Keyed(CreateText("ContractNumber", transform, "", .665f, .485f, .92f, .54f, 19, _mint), "NIGHT OPERATIONS");
            Keyed(CreateText("ContractBrief", transform, "", .665f, .375f, .92f, .48f, 24, _cream), "WATCH THE ANGLES.\nMAKE EVERY SHOT COUNT.");
            Keyed(CreateText("ContractFooter", transform, "", .665f, .31f, .92f, .36f, 14, _mint), "ONE BUILDING. ONE WAY OUT.");

            CreatePanel("Footer", transform, 0, 0, 1, .205f, new Color32(6, 4, 4, 255));
            CreatePanel("FooterRule", transform, .04f, .202f, .96f, .205f, new Color32(50, 67, 63, 255));
            Keyed(CreateText("ControlsHeading", transform, "", .07f, .145f, .32f, .19f, 15, _mint), "FIELD MANUAL");
            Keyed(CreateText("Movement", transform, "", .07f, .04f, .34f, .14f, 18, _cream), "A/D  Move     SPACE  Jump\nCTRL  Crouch     E  Use");
            Keyed(CreateText("Combat", transform, "", .37f, .04f, .66f, .14f, 18, _cream), "LMB  Fire     RMB/F  Melee\nQ  Swap weapon");
            Keyed(CreateText("Session", transform, "", .72f, .04f, .94f, .14f, 18, _cream), "R  Reload / Retry\nT  Next song     ESC  Pause");
            // Credits: under Play, small; the music's licence asks that it be credited where players can see it.
            _creditsButton = CreateButton("Credits", "", .07f, .225f, .25f, .27f);
            Keyed(_creditsButton.GetComponentInChildren<Text>(), "CREDITS");
            _creditsButton.onClick.AddListener(ShowCredits);
            foreach (Transform child in transform)
                if (child.gameObject.activeSelf) _titleScreen.Add(child.gameObject);
            BuildPause();
            BuildSettings();
            BuildCredits();
            Localize(key => key);
        }

        // Over a paused game: the game shows dimmed behind a small panel - the name, Continue, settings and language.
        private void BuildPause()
        {
            Image shade = CreatePanel("Pause", transform, 0, 0, 1, 1, new Color(0f, 0f, 0f, .6f));
            shade.raycastTarget = true;
            _pause = shade.gameObject;
            Image window = CreatePanel("Window", shade.transform, .36f, .22f, .64f, .8f, new Color32(18, 13, 12, 245));
            window.raycastTarget = true;
            CreatePanel("Rule", window.transform, 0f, .985f, 1f, 1f, _red);
            CreatePanel("Spine", window.transform, .06f, .1f, .07f, .9f, _red);
            CreateText("Title", window.transform, "CUBES", .14f, .74f, .94f, .94f, 56, _cream).fontStyle = FontStyle.Bold;
            Keyed(CreateText("Paused", window.transform, "", .14f, .66f, .94f, .74f, 16, _mint), "PAUSED");
            Button resume = PauseButton("Continue", window.transform, .55f, "CONTINUE");
            ColorBlock colors = resume.colors;
            colors.normalColor = _mint;
            colors.highlightedColor = _cream;
            colors.selectedColor = _cream;
            resume.colors = colors;
            resume.GetComponentInChildren<Text>().color = new Color32(8, 6, 6, 255);
            resume.onClick.AddListener(OnPlayClicked);
            PauseButton("Settings", window.transform, .39f, "SETTINGS").onClick.AddListener(OnSettingsClicked);
            PauseButton("Language", window.transform, .23f, "LANGUAGE: ENGLISH").onClick.AddListener(OnLanguageClicked);
            _pause.SetActive(false);
        }

        private Button PauseButton(string objectName, Transform parent, float bottom, string key)
        {
            Button button = CreateButton(objectName, "", .14f, bottom, .9f, bottom + .12f);
            button.transform.SetParent(parent, false);
            Keyed(button.GetComponentInChildren<Text>(), key);
            return button;
        }

        // Who made what: the text in Resources/TXT_Credits. Nothing is decided here, so the button opens it directly.
        private void BuildCredits()
        {
            Image shade = CreatePanel("Credits Panel", transform, 0, 0, 1, 1, new Color(0f, 0f, 0f, .75f));
            shade.raycastTarget = true;
            _credits = shade.gameObject;
            Image window = CreatePanel("Window", shade.transform, .2f, .08f, .8f, .92f, new Color32(24, 17, 15, 255));
            window.raycastTarget = true;
            CreatePanel("Rule", window.transform, 0f, .985f, 1f, 1f, _red);
            Keyed(CreateText("Title", window.transform, "", .06f, .88f, .94f, .97f, 28, _cream), "CREDITS");
            var asset = Resources.Load<TextAsset>("TXT_Credits");
            Text body = CreateText("Body", window.transform, asset != null ? asset.text : "", .06f, .12f, .94f, .87f, 16, _cream);
            body.alignment = TextAnchor.UpperLeft;
            Button close = CreateButton("Close", "", .7f, .025f, .94f, .1f);
            close.transform.SetParent(window.transform, false);
            Keyed(close.GetComponentInChildren<Text>(), "CLOSE");
            close.onClick.AddListener(() => _credits.SetActive(false));
            _credits.SetActive(false);
        }

        private void ShowCredits()
        {
            _credits.transform.SetAsLastSibling();
            _credits.SetActive(true);
        }

        // The title screen, or the pause panel over the game behind it.
        public void ShowAsPause(bool paused)
        {
            foreach (GameObject part in _titleScreen) part.SetActive(!paused);
            _pause.SetActive(paused);
            HideSettings();
            if (_credits != null) _credits.SetActive(false);
        }

        private void BuildSettings()
        {
            Image shade = CreatePanel("Settings", transform, 0, 0, 1, 1, new Color(0f, 0f, 0f, .7f));
            shade.raycastTarget = true;
            _settings = shade.gameObject;
            Image window = CreatePanel("Window", shade.transform, .3f, .28f, .7f, .74f, new Color32(24, 17, 15, 255));
            window.raycastTarget = true;
            CreatePanel("Rule", window.transform, 0f, .97f, 1f, 1f, _red);
            Keyed(CreateText("Title", window.transform, "", .08f, .8f, .92f, .94f, 30, _cream), "SETTINGS");
            Keyed(CreateText("Master label", window.transform, "", .08f, .62f, .92f, .72f, 18, _mint), "MASTER VOLUME");
            _masterSlider = CreateSlider("Master", window.transform, .08f, .52f, .92f, .61f);
            Keyed(CreateText("Music label", window.transform, "", .08f, .37f, .92f, .47f, 18, _mint), "MUSIC VOLUME");
            _musicSlider = CreateSlider("Music", window.transform, .08f, .27f, .92f, .36f);
            Button close = CreateButton("Close", "", .62f, .06f, .92f, .17f);
            close.transform.SetParent(window.transform, false);
            Keyed(close.GetComponentInChildren<Text>(), "CLOSE");
            close.onClick.AddListener(() => SettingsCloseClicked?.Invoke());
            _masterSlider.onValueChanged.AddListener(volume => MasterVolumeChanged?.Invoke(volume));
            _musicSlider.onValueChanged.AddListener(volume => MusicVolumeChanged?.Invoke(volume));
            _settings.SetActive(false);
        }

        public void ShowSettings(float master, float music)
        {
            _masterSlider.SetValueWithoutNotify(master);
            _musicSlider.SetValueWithoutNotify(music);
            _settings.transform.SetAsLastSibling();
            _settings.SetActive(true);
        }

        public void HideSettings()
        {
            if (_settings != null) _settings.SetActive(false);
        }

        private Slider CreateSlider(string objectName, Transform parent, float left, float bottom, float right, float top)
        {
            RectTransform root = CreateRect(objectName, parent, left, bottom, right, top);
            Image hit = root.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);
            CreatePanel("Track", root, 0f, .35f, 1f, .65f, new Color32(50, 40, 34, 255));
            RectTransform fillArea = CreateRect("Fill Area", root, 0f, .35f, 1f, .65f);
            Image fill = CreatePanel("Fill", fillArea, 0f, 0f, 1f, 1f, _mint);
            RectTransform handleArea = CreateRect("Handle Area", root, 0f, 0f, 1f, 1f);
            Image handle = CreatePanel("Handle", handleArea, 0f, 0f, 0f, 1f, _cream);
            handle.rectTransform.sizeDelta = new Vector2(16f, 0f);
            handle.raycastTarget = true;
            Slider slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.navigation = new Navigation {mode = Navigation.Mode.None};
            // A soft tick every tenth of the way, like a stepped pot.
            int notch = -1;
            slider.onValueChanged.AddListener(value =>
            {
                int now = Mathf.RoundToInt(value * 10f);
                if (now != notch && notch >= 0 && _click != null) _ui.PlayOneShot(_click, 0.35f);
                notch = now;
            });
            return slider;
        }

        private void Keyed(Text text, string key) => _texts.Add((text, key));

        // Writes every text again in the language the translator returns.
        public void Localize(Func<string, string> translate)
        {
            foreach ((Text text, string key) in _texts) text.text = translate(key);
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _languageButton.onClick.AddListener(OnLanguageClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _languageButton.onClick.RemoveListener(OnLanguageClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        private void OnSettingsClicked() => SettingsClicked?.Invoke();

        private void OnLanguageClicked() => LanguageClicked?.Invoke();
        private void OnPlayClicked() => PlayClicked?.Invoke();

        private void Sound(AudioClip clip)
        {
            if (clip != null && _ui != null) _ui.PlayOneShot(clip, 0.8f);
        }

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
            button.onClick.AddListener(() => Sound(objectName == "PlayContract" ? _confirm : _click));
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
