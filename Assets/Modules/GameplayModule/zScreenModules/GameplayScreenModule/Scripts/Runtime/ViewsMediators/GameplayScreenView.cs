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
        private Text _location;
        private Text _status;
        private Text _objective;
        private Button _retryButton;
        private Button _menuButton;

        private GameObject _terminal;
        private Text _terminalTitle;
        private InputField _terminalText;
        private Button _terminalSave;
        private Button _terminalClose;

        // The fixed labels by the key they are translated from, and the translator the screen was last given.
        private readonly System.Collections.Generic.List<(Text text, string key)> _labels = new();
        private Func<string, string> _t = key => key;

        public Action RetryClicked;
        public Action MenuClicked;
        public Action<string> TerminalSaveClicked;
        public Action TerminalCloseClicked;
        public Action<int> MenuEntryClicked;
        public Action MenuCloseClicked;

        // The now-playing card is a cassette: slides in from the right edge, holds with its reels turning, slides out.
        private const float CardIn = 0.7f;
        private const float CardHold = 4.5f;
        private const float CardOut = 0.8f;
        private RectTransform _card;
        private CanvasGroup _cardFade;
        private Text _cardTitle;
        private Text _cardArtist;
        private readonly RectTransform[] _cardReels = new RectTransform[2];
        private float _cardAge = -1f;
        private Sprite _disc;
        private Sprite _ring;

        // The tape deck panel: a boombox with the loaded cassette in its window and transport buttons.
        private GameObject _deck;
        private Text _deckTitle;
        private Text _deckArtist;
        private Text _deckPlayLabel;
        private Text _deckState;
        private readonly RectTransform[] _deckReels = new RectTransform[2];
        private Image _deckLight;
        private bool _deckPlaying;
        private Button _deckPrevious;
        private Button _deckPlay;
        private Button _deckNext;
        private Button _deckClose;
        public Action<int> DeckButtonClicked;
        public Action DeckCloseClicked;
        public Action<float> DeckVolumeChanged;
        private GameObject _call;
        private AudioSource _ui;
        private AudioClip _click;
        private Text _callCaller;
        private Text _callLine;
        private Image _callPhone;
        private Text _callNext;
        private Text _callIncoming;
        private bool _callRinging;
        private Text _banner;
        private CanvasGroup _bannerFade;
        private float _bannerAge = -1f;
        private DeckKnob _deckVolume;
        private GameObject _touch;
        private Sprite _thinRing;

        private const float MenuRowHeight = 62f;
        private GameObject _menu;
        private Text _menuTitle;
        private Text _menuSubtitle;
        // As many rows as the longest list needed so far; the list scrolls - wheel, drag or the bar.
        private readonly System.Collections.Generic.List<Button> _menuButtons = new();
        private readonly System.Collections.Generic.List<Text> _menuLabels = new();
        private RectTransform _menuContent;
        private ScrollRect _menuScroll;
        private Button _menuClose;

        private void Awake()
        {
            _font = Serif(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            CreatePanel("Header", transform, 0, .84f, 1, 1, _ink);
            CreatePanel("HeaderRule", transform, .025f, .84f, .975f, .843f, _mint);
            _location = CreateText("Location", transform, "", .03f, .87f, .27f, .985f, 28, _cream);
            _status = CreateText("Status", transform, "AWAITING CONTRACT", .29f, .925f, .80f, .985f, 23, _mint);
            _objective = CreateText("Objective", transform, "", .29f, .85f, .80f, .925f, 20, _cream);
            _retryButton = CreateButton("Retry", "RETRY", .835f, .925f, .975f, .98f);
            _menuButton = CreateButton("Menu", "MENU", .835f, .855f, .975f, .91f);

            BuildTerminal();
            BuildMenu();
            BuildNowPlaying();
            _ui = gameObject.AddComponent<AudioSource>();
            _ui.playOnAwake = false;
            _ui.ignoreListenerPause = true;
            _click = Resources.Load<AudioClip>("SND_UI_HudClick");
            BuildDeck();
            BuildTouchControls();
            BuildCall();
            BuildBanner();
        }

        // Round shapes for the reels and speakers, drawn once: a filled disc and a ring.
        private Sprite Round(float inner)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) {wrapMode = TextureWrapMode.Clamp};
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = new Vector2(x + 0.5f - size * 0.5f, y + 0.5f - size * 0.5f).magnitude / (size * 0.5f);
                    float a = Mathf.Clamp01((1f - d) * size * 0.5f) * Mathf.Clamp01((d - inner) * size * 0.5f + (inner <= 0f ? 1f : 0f));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private Image Circle(string objectName, Transform parent, float left, float bottom, float right, float top, Color color, bool ring = false)
        {
            Image image = CreatePanel(objectName, parent, left, bottom, right, top, color);
            image.sprite = ring ? _ring : _disc;
            image.preserveAspect = true;
            return image;
        }

        // A cassette reel: a ring with a hub and three spokes, turned by its RectTransform.
        private RectTransform Reel(string objectName, Transform parent, float left, float bottom, float right, float top)
        {
            Image tape = Circle(objectName, parent, left, bottom, right, top, new Color32(52, 34, 22, 255));
            Image hub = Circle("Hub", tape.transform, .3f, .3f, .7f, .7f, new Color32(226, 214, 190, 255));
            for (int i = 0; i < 3; i++)
            {
                Image spoke = CreatePanel("Spoke " + i, hub.transform, .44f, .05f, .56f, .95f, new Color32(20, 14, 12, 255));
                spoke.rectTransform.localEulerAngles = new Vector3(0f, 0f, i * 60f);
            }
            Circle("Pin", hub.transform, .38f, .38f, .62f, .62f, new Color32(20, 14, 12, 255));
            return tape.rectTransform;
        }

        // A cassette: a dark shell, a cream label with the title, the window with two reels, screws, the lower bevel.
        private RectTransform Cassette(Transform parent, float left, float bottom, float right, float top, out Text title, out Text artist, RectTransform[] reels)
        {
            Image shell = CreatePanel("Cassette", parent, left, bottom, right, top, new Color32(28, 24, 22, 255));
            CreatePanel("Shell rim", shell.transform, .015f, .03f, .985f, .97f, new Color32(40, 34, 30, 255));
            Image label = CreatePanel("Label", shell.transform, .06f, .38f, .94f, .92f, new Color32(226, 214, 190, 255));
            CreatePanel("Label band", label.transform, 0f, .78f, 1f, 1f, new Color32(140, 22, 28, 255));
            CreateText("Side", label.transform, "A", .03f, .78f, .12f, 1f, 12, new Color32(226, 214, 190, 255)).alignment = TextAnchor.MiddleCenter;
            CreateText("Brand", label.transform, "NIGHT SHIFT  C-60", .14f, .78f, .97f, 1f, 11, new Color32(226, 214, 190, 255));
            title = CreateText("Title", label.transform, "", .05f, .4f, .97f, .76f, 20, new Color32(20, 14, 12, 255));
            title.fontStyle = FontStyle.BoldAndItalic;
            artist = CreateText("Artist", label.transform, "", .05f, .06f, .97f, .4f, 14, new Color32(60, 44, 34, 255));
            artist.fontStyle = FontStyle.Italic;
            Image window = CreatePanel("Window", shell.transform, .26f, .12f, .74f, .46f, new Color32(10, 8, 8, 255));
            CreatePanel("Tape", window.transform, .22f, .2f, .78f, .8f, new Color32(52, 34, 22, 255));
            reels[0] = Reel("Reel L", window.transform, .04f, .06f, .4f, .94f);
            reels[1] = Reel("Reel R", window.transform, .6f, .06f, .96f, .94f);
            CreatePanel("Bevel", shell.transform, .18f, 0f, .82f, .1f, new Color32(20, 17, 16, 255));
            foreach (Vector2 screw in new[] {new Vector2(.035f, .06f), new Vector2(.935f, .06f), new Vector2(.035f, .9f), new Vector2(.935f, .9f)})
                Circle("Screw", shell.transform, screw.x, screw.y, screw.x + .03f, screw.y + .07f, new Color32(150, 130, 100, 255));
            return shell.rectTransform;
        }

        private void BuildNowPlaying()
        {
            _disc = Round(0f);
            _ring = Round(0.35f);
            RectTransform holder = CreateRect("Now Playing", transform, .7f, .125f, .985f, .3f);
            _card = holder;
            _cardFade = holder.gameObject.AddComponent<CanvasGroup>();
            _cardFade.blocksRaycasts = false;
            _cardFade.interactable = false;
            Label(CreateText("Label", holder, "", .02f, .86f, .98f, 1f, 13, _mint), "NOW PLAYING").fontStyle = FontStyle.Italic;
            Cassette(holder, 0f, 0f, 1f, .85f, out _cardTitle, out _cardArtist, _cardReels);
            holder.gameObject.SetActive(false);
        }

        public void ShowNowPlaying(string title, string artist)
        {
            _cardTitle.text = title ?? string.Empty;
            _cardArtist.text = artist ?? string.Empty;
            _cardAge = 0f;
            _card.gameObject.SetActive(true);
            AnimateCard();
        }

        // Eased in from beyond the right edge and faded up, held with its reels turning, then eased back out.
        private void AnimateCard()
        {
            if (_cardAge < 0f) return;
            _cardAge += Time.unscaledDeltaTime;
            float width = _card.rect.width + 40f;
            float shown;
            if (_cardAge < CardIn) shown = Mathf.SmoothStep(0f, 1f, _cardAge / CardIn);
            else if (_cardAge < CardIn + CardHold) shown = 1f;
            else shown = 1f - Mathf.SmoothStep(0f, 1f, (_cardAge - CardIn - CardHold) / CardOut);
            float eased = 1f - Mathf.Pow(1f - shown, 3f);
            _card.anchoredPosition = new Vector2((1f - eased) * width, 0f);
            _cardFade.alpha = shown;
            Spin(_cardReels, 1f);
            if (_cardAge < CardIn + CardHold + CardOut) return;
            _cardAge = -1f;
            _card.gameObject.SetActive(false);
        }

        // The take-up reel turns a little slower than the supply reel, the way tape does.
        private void Spin(RectTransform[] reels, float speed)
        {
            float step = -180f * speed * Time.unscaledDeltaTime;
            if (reels[0] != null) reels[0].Rotate(0f, 0f, step);
            if (reels[1] != null) reels[1].Rotate(0f, 0f, step * 0.8f);
        }

        // ---------------------------------------------------------------- tape deck panel

        private void BuildDeck()
        {
            Image body = CreatePanel("Tape Deck", transform, .2f, .2f, .8f, .78f, new Color32(18, 15, 14, 250));
            body.raycastTarget = true;
            _deck = body.gameObject;
            CreatePanel("Chrome top", body.transform, 0f, .955f, 1f, 1f, new Color32(120, 110, 95, 255));
            CreatePanel("Handle", body.transform, .3f, 1f, .7f, 1.04f, new Color32(80, 72, 62, 255));
            CreateText("Brand", body.transform, "SHAPE & SONS  ·  STEREO CASSETTE", .05f, .87f, .95f, .95f, 15, _mint).alignment = TextAnchor.MiddleCenter;
            foreach (float x in new[] {.03f, .75f})
            {
                Image grille = Circle("Speaker", body.transform, x, .4f, x + .22f, .84f, new Color32(8, 7, 7, 255));
                Circle("Rim", grille.transform, 0f, 0f, 1f, 1f, new Color32(150, 130, 100, 255), true);
                Circle("Cone", grille.transform, .3f, .3f, .7f, .7f, new Color32(40, 34, 30, 255));
                Circle("Dust cap", grille.transform, .43f, .43f, .57f, .57f, new Color32(150, 130, 100, 255));
            }
            Image bay = CreatePanel("Cassette bay", body.transform, .27f, .34f, .73f, .84f, new Color32(6, 5, 5, 255));
            Cassette(bay.transform, .05f, .06f, .95f, .94f, out _deckTitle, out _deckArtist, _deckReels);
            _deckLight = Circle("Light", body.transform, .47f, .26f, .53f, .32f, new Color32(140, 22, 28, 255));
            _deckState = CreateText("State", body.transform, "", .27f, .2f, .73f, .27f, 13, _cream);
            _deckVolume = BuildKnob(body.transform, new Vector2(.86f, .29f), 80f);
            int notch = -1;
            _deckVolume.Changed = volume =>
            {
                // The pot clicks through its detents.
                int now = Mathf.RoundToInt(volume * 10f);
                if (now != notch && notch >= 0 && _click != null) _ui.PlayOneShot(_click, 0.3f);
                notch = now;
                DeckVolumeChanged?.Invoke(volume);
            };
            _deckState.alignment = TextAnchor.MiddleCenter;
            _deckPrevious = CreateButton("Previous", "|<  PREV", .1f, .05f, .3f, .17f, body.transform);
            _deckPlay = CreateButton("Play", "PLAY", .32f, .05f, .52f, .17f, body.transform);
            _deckPlayLabel = _deckPlay.GetComponentInChildren<Text>();
            _deckNext = CreateButton("Next", "NEXT  >|", .54f, .05f, .74f, .17f, body.transform);
            _deckClose = CreateButton("Close", "CLOSE", .78f, .05f, .92f, .17f, body.transform);
            _deckPlay.onClick.AddListener(() => DeckButtonClicked?.Invoke(0));
            _deckNext.onClick.AddListener(() => DeckButtonClicked?.Invoke(1));
            _deckPrevious.onClick.AddListener(() => DeckButtonClicked?.Invoke(2));
            _deckClose.onClick.AddListener(() => DeckCloseClicked?.Invoke());
            _deck.SetActive(false);
        }

        public void ShowDeck(string title, string artist, bool playing)
        {
            _deckTitle.text = title ?? string.Empty;
            _deckArtist.text = artist ?? string.Empty;
            _deckPlaying = playing;
            ShowDeckState();
            _deckLight.color = playing ? new Color32(230, 40, 40, 255) : new Color32(60, 14, 16, 255);
            _deck.SetActive(true);
        }

        private void ShowDeckState()
        {
            _deckPlayLabel.text = _t(_deckPlaying ? "PAUSE" : "PLAY");
            _deckState.text = _t(_deckPlaying ? "PLAYING" : "STOPPED");
        }

        public void HideDeck() => _deck.SetActive(false);

        // ---------------------------------------------------------------- touch controls

        // A stick on the left (walk, pull down to crouch) and buttons on the right, shown on a phone or a tablet. Each
        // presses the same key a keyboard would, so the game reads them without knowing they are on a screen. Aiming
        // and firing is a finger anywhere else on the game, which the game reads itself.
        private void BuildTouchControls()
        {
            RectTransform layer = CreateRect("Touch Controls", transform, 0f, 0f, 1f, .84f);
            _touch = layer.gameObject;
            // The stick's corner of the screen is the stick's: a thumb that misses the knob there must not fire.
            Image guard = CreatePanel("Stick Zone", layer, 0f, 0f, .32f, .62f, new Color(0f, 0f, 0f, 0f));
            guard.raycastTarget = true;
            // Off while the controls are made: an on-screen control binds its path as it is enabled.
            _touch.SetActive(false);
            // The stick: a ring the knob is dragged around in.
            RectTransform pad = CreateRect("Stick", layer, .03f, .06f, .03f, .06f);
            pad.pivot = Vector2.zero;
            pad.sizeDelta = new Vector2(230f, 230f);
            _thinRing ??= Round(0.9f);
            Circle("Ring", pad, 0f, 0f, 1f, 1f, new Color(0f, 0f, 0f, .25f));
            Circle("Rim", pad, 0f, 0f, 1f, 1f, new Color32(196, 160, 92, 140)).sprite = _thinRing;
            Image knob = Circle("Knob", pad, .3f, .3f, .7f, .7f, new Color32(196, 160, 92, 170));
            knob.raycastTarget = true;
            var stick = knob.gameObject.AddComponent<UnityEngine.InputSystem.OnScreen.OnScreenStick>();
            stick.controlPath = "<Gamepad>/leftStick";
            stick.movementRange = 80f;
            // The buttons, in a thumb's arc on the right: the big one is Use.
            TouchButton("Use", "E", "<Keyboard>/e", layer, new Vector2(-.18f, .16f), 120f, _mint);
            TouchButton("Jump", "\u25B2", "<Keyboard>/space", layer, new Vector2(-.07f, .3f), 96f, _cream);
            TouchButton("Melee", "F", "<Keyboard>/f", layer, new Vector2(-.07f, .12f), 96f, new Color32(220, 70, 70, 255));
            TouchButton("Reload", "R", "<Keyboard>/r", layer, new Vector2(-.18f, .38f), 80f, _cream);
            TouchButton("Swap", "Q", "<Keyboard>/q", layer, new Vector2(-.29f, .13f), 80f, _cream);
            TouchButton("Next Song", "\u266A", "<Keyboard>/t", layer, new Vector2(-.07f, .48f), 70f, _mint);
            SetTouchControls(Application.isMobilePlatform);
        }

        private void TouchButton(string objectName, string glyph, string control, Transform parent, Vector2 fromBottomRight, float size, Color color)
        {
            RectTransform rect = CreateRect(objectName, parent, 1f + fromBottomRight.x, fromBottomRight.y, 1f + fromBottomRight.x, fromBottomRight.y);
            rect.sizeDelta = new Vector2(size, size);
            Image face = Circle("Face", rect, 0f, 0f, 1f, 1f, new Color(color.r * .3f, color.g * .3f, color.b * .3f, .45f));
            face.raycastTarget = true;
            _thinRing ??= Round(0.88f);
            Circle("Rim", rect, 0f, 0f, 1f, 1f, new Color(color.r, color.g, color.b, .85f)).sprite = _thinRing;
            Text label = CreateText("Glyph", rect, glyph, 0f, 0f, 1f, 1f, (int) (size * .38f), color);
            label.alignment = TextAnchor.MiddleCenter;
            var button = face.gameObject.AddComponent<UnityEngine.InputSystem.OnScreen.OnScreenButton>();
            button.controlPath = control;
        }

        // On by default on a phone or tablet; the Editor can turn them on to try them with the mouse.
        public void SetTouchControls(bool on)
        {
            if (_touch != null) _touch.SetActive(on);
        }

        // ---------------------------------------------------------------- phone call

        // A call card along the bottom: a phone that shakes while it speaks, who is calling, and the line.
        private void BuildCall()
        {
            Image card = CreatePanel("Phone Call", transform, .18f, .04f, .82f, .25f, new Color32(16, 12, 11, 245));
            _call = card.gameObject;
            CreatePanel("Rule", card.transform, 0f, .97f, 1f, 1f, new Color32(140, 22, 28, 255));
            Image phone = CreatePanel("Phone", card.transform, .03f, .14f, .1f, .86f, new Color32(30, 24, 22, 255));
            _callPhone = phone;
            CreatePanel("Screen", phone.transform, .14f, .3f, .86f, .88f, new Color32(150, 210, 230, 255));
            CreatePanel("Earpiece", phone.transform, .35f, .91f, .65f, .95f, _mint);
            _callCaller = CreateText("Caller", card.transform, "", .13f, .66f, .7f, .92f, 18, _mint);
            _callCaller.fontStyle = FontStyle.Bold;
            _callIncoming = Label(CreateText("Incoming", card.transform, "", .7f, .66f, .97f, .92f, 13, new Color32(200, 60, 60, 255)), "INCOMING CALL");
            _callIncoming.alignment = TextAnchor.MiddleRight;
            _callLine = CreateText("Line", card.transform, "", .13f, .2f, .97f, .66f, 22, _cream);
            _callLine.fontStyle = FontStyle.Italic;
            _callNext = Label(CreateText("Next", card.transform, "", .5f, .02f, .97f, .2f, 12, _mint), "E / SPACE  —  next");
            _callNext.alignment = TextAnchor.MiddleRight;
            _call.SetActive(false);
        }

        // The phone rings: the caller's name and a prompt to pick up; nothing is said until then.
        public void ShowCallRinging(string caller)
        {
            _callRinging = true;
            _callCaller.text = caller ?? string.Empty;
            _callLine.text = _t("Click, E or Space to answer");
            _callIncoming.gameObject.SetActive(true);
            _callNext.gameObject.SetActive(false);
            _call.transform.SetAsLastSibling();
            _call.SetActive(true);
        }

        public void ShowCall(string caller, string line)
        {
            _callRinging = false;
            _callIncoming.gameObject.SetActive(false);
            _callNext.gameObject.SetActive(true);
            _callCaller.text = caller ?? string.Empty;
            _callLine.text = "\u201C" + (line ?? string.Empty) + "\u201D";
            _call.transform.SetAsLastSibling();
            _call.SetActive(true);
        }

        public void HideCall() => _call.SetActive(false);

        private void AnimateCall()
        {
            if (_call == null || !_call.activeSelf) return;
            // Shakes in bursts while it rings, still once answered; the prompt pulses until then.
            bool burst = _callRinging && Mathf.Repeat(Time.unscaledTime, 1.4f) < 0.8f;
            _callPhone.rectTransform.localEulerAngles = new Vector3(0f, 0f, burst ? Mathf.Sin(Time.unscaledTime * 40f) * 8f : 0f);
            if (_callRinging) _callLine.color = Color.Lerp(_cream, _mint, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f));
            else _callLine.color = _cream;
        }

        // ---------------------------------------------------------------- banner

        private const float BannerTime = 2.8f;

        private void BuildBanner()
        {
            RectTransform holder = CreateRect("Banner", transform, 0f, .55f, 1f, .7f);
            _bannerFade = holder.gameObject.AddComponent<CanvasGroup>();
            _bannerFade.blocksRaycasts = false;
            CreatePanel("Band", holder, 0f, .1f, 1f, .9f, new Color32(10, 8, 8, 220));
            CreatePanel("Rule top", holder, .3f, .86f, .7f, .9f, new Color32(140, 22, 28, 255));
            CreatePanel("Rule bottom", holder, .3f, .1f, .7f, .14f, new Color32(140, 22, 28, 255));
            _banner = CreateText("Text", holder, "", 0f, .15f, 1f, .85f, 46, _cream);
            _banner.alignment = TextAnchor.MiddleCenter;
            _banner.fontStyle = FontStyle.Bold;
            holder.gameObject.SetActive(false);
        }

        public void ShowBanner(string text)
        {
            _banner.text = text ?? string.Empty;
            _bannerAge = 0f;
            _bannerFade.transform.SetAsLastSibling();
            _bannerFade.gameObject.SetActive(true);
        }

        private void AnimateBanner()
        {
            if (_bannerAge < 0f) return;
            _bannerAge += Time.unscaledDeltaTime;
            _bannerFade.alpha = Mathf.Clamp01(_bannerAge / 0.2f) * Mathf.Clamp01((BannerTime - _bannerAge) / 0.5f);
            _banner.transform.localScale = Vector3.one * (1f + 0.15f * Mathf.Clamp01(1f - _bannerAge / 0.25f));
            if (_bannerAge < BannerTime) return;
            _bannerAge = -1f;
            _bannerFade.gameObject.SetActive(false);
        }

        public void ShowMusicVolume(float volume) => _deckVolume.SetWithoutNotify(volume);

        // The volume pot, drawn like a 70s deck's: a brass skirt with a dotted scale, a knurled
        // bakelite cap that turns, a cream pointer, and VOLUME engraved beside it.
        private DeckKnob BuildKnob(Transform parent, Vector2 anchor, float size)
        {
            RectTransform root = CreateRect("Volume knob", parent, anchor.x, anchor.y, anchor.x, anchor.y);
            root.sizeDelta = new Vector2(size, size);
            Color brass = new Color32(176, 140, 74, 255);
            Color bakelite = new Color32(34, 24, 20, 255);
            // The scale: eleven dots over the knob's sweep, the loud end in red.
            for (int i = 0; i <= 10; i++)
            {
                RectTransform arm = CreateRect("Tick " + i, root, 0f, 0f, 1f, 1f);
                arm.localEulerAngles = new Vector3(0f, 0f, 135f - i * 27f);
                bool major = i % 5 == 0;
                Circle("Dot", arm, .47f, major ? .93f : .95f, .53f, major ? 1.05f : 1.01f, i >= 8 ? new Color32(200, 40, 36, 255) : _cream);
            }
            Text min = CreateText("Min", root, "0", -.2f, -.1f, .2f, .1f, 10, _mint);
            min.alignment = TextAnchor.MiddleCenter;
            Text max = CreateText("Max", root, "10", .8f, -.1f, 1.2f, .1f, 10, _mint);
            max.alignment = TextAnchor.MiddleCenter;
            Label(CreateText("Engraving", root, "VOLUME", -1.1f, .38f, -.05f, .62f, 12, brass), "VOLUME").alignment = TextAnchor.MiddleRight;

            Image skirt = Circle("Skirt", root, .1f, .1f, .9f, .9f, brass);
            skirt.raycastTarget = true;
            Circle("Skirt shade", root, .13f, .13f, .87f, .87f, new Color32(110, 84, 42, 255));
            RectTransform dial = CreateRect("Dial", root, .17f, .17f, .83f, .83f);
            Circle("Cap", dial, 0f, 0f, 1f, 1f, bakelite);
            // Knurling: ridges round the cap's edge, turning with it.
            for (int i = 0; i < 24; i++)
            {
                RectTransform arm = CreateRect("Ridge " + i, dial, 0f, 0f, 1f, 1f);
                arm.localEulerAngles = new Vector3(0f, 0f, i * 15f);
                CreatePanel("Ridge", arm, .485f, .9f, .515f, 1f, new Color32(80, 62, 50, 255));
            }
            Circle("Face", dial, .2f, .2f, .8f, .8f, new Color32(52, 38, 32, 255));
            Circle("Shine", dial, .3f, .52f, .52f, .74f, new Color(1f, 1f, 1f, .08f));
            CreatePanel("Pointer", dial, .47f, .6f, .53f, .97f, _cream);
            Circle("Hub", dial, .44f, .44f, .56f, .56f, brass);

            DeckKnob knob = root.gameObject.AddComponent<DeckKnob>();
            knob.Dial = dial;
            knob.SetWithoutNotify(0.8f);
            return knob;
        }

        private void AnimateDeck()
        {
            if (_deck == null || !_deck.activeSelf || !_deckPlaying) return;
            Spin(_deckReels, 1f);
            _deckLight.color = Color.Lerp(new Color32(150, 20, 24, 255), new Color32(240, 50, 50, 255), 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f));
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
            // The list: a masked viewport over a column that grows with the entries, and a thin bar beside it.
            RectTransform viewport = CreateRect("Viewport", frame.transform, .04f, .12f, .93f, .88f);
            viewport.gameObject.AddComponent<RectMask2D>();
            Image hit = viewport.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);
            _menuContent = CreateRect("Content", viewport, 0f, 1f, 1f, 1f);
            _menuContent.pivot = new Vector2(.5f, 1f);
            var column = _menuContent.gameObject.AddComponent<VerticalLayoutGroup>();
            column.spacing = 6f;
            column.childControlWidth = true;
            column.childControlHeight = false;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            _menuContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Image track = CreatePanel("Scrollbar", frame.transform, .945f, .12f, .96f, .88f, new Color32(40, 30, 28, 255));
            track.raycastTarget = true;
            Image handle = CreatePanel("Handle", track.transform, 0f, 0f, 1f, 1f, _mint);
            handle.raycastTarget = true;
            var bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle.rectTransform;
            bar.targetGraphic = handle;
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.navigation = new Navigation {mode = Navigation.Mode.None};
            _menuScroll = frame.gameObject.AddComponent<ScrollRect>();
            _menuScroll.viewport = viewport;
            _menuScroll.content = _menuContent;
            _menuScroll.horizontal = false;
            _menuScroll.vertical = true;
            _menuScroll.movementType = ScrollRect.MovementType.Clamped;
            _menuScroll.scrollSensitivity = 30f;
            _menuScroll.verticalScrollbar = bar;
            _menuScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            _menuClose = CreateButton("Close", "CLOSE", .78f, .03f, .96f, .1f, frame.transform);
            _menuClose.onClick.AddListener(() => MenuCloseClicked?.Invoke());
            _menu.SetActive(false);
        }

        public void ShowMenu(string title, string subtitle, string[] entries)
        {
            _menuTitle.text = title ?? string.Empty;
            _menuSubtitle.text = subtitle ?? string.Empty;
            int count = entries?.Length ?? 0;
            while (_menuButtons.Count < count) AddMenuRow();
            for (int i = 0; i < _menuButtons.Count; i++)
            {
                bool used = i < count;
                _menuButtons[i].gameObject.SetActive(used);
                if (used) _menuLabels[i].text = entries[i];
            }
            _menu.SetActive(true);
            // A list opened again starts at its top, unless it is the same list refreshed after a pick.
            bool sameList = _menuShownTitle == _menuTitle.text && _menuShownCount == count;
            _menuShownTitle = _menuTitle.text;
            _menuShownCount = count;
            if (!sameList)
            {
                Canvas.ForceUpdateCanvases();
                _menuScroll.verticalNormalizedPosition = 1f;
            }
        }

        private string _menuShownTitle;
        private int _menuShownCount = -1;

        private void AddMenuRow()
        {
            int index = _menuButtons.Count;
            Button button = CreateButton("Entry " + index, "", 0f, 1f, 1f, 1f, _menuContent);
            var rect = (RectTransform) button.transform;
            rect.sizeDelta = new Vector2(0f, MenuRowHeight);
            button.gameObject.AddComponent<LayoutElement>().preferredHeight = MenuRowHeight;
            Text label = button.GetComponentInChildren<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            label.fontSize = 16;
            label.resizeTextMaxSize = 16;
            button.onClick.AddListener(() => MenuEntryClicked?.Invoke(index));
            _menuButtons.Add(button);
            _menuLabels.Add(label);
        }

        public void HideMenu()
        {
            _menu.SetActive(false);
            _menuShownTitle = null;
        }

        // The computer screen: its title, the text on it, and the player's edits to it.
        private void BuildTerminal()
        {
            Image frame = CreatePanel("Terminal", transform, .2f, .16f, .8f, .8f, new Color32(12, 9, 6, 246));
            frame.raycastTarget = true;
            _terminal = frame.gameObject;
            CreatePanel("Bezel", frame.transform, .0f, .0f, 1f, .006f, _mint);
            CreatePanel("BezelTop", frame.transform, .0f, .994f, 1f, 1f, _mint);
            _terminalTitle = CreateText("Title", frame.transform, "", .04f, .87f, .96f, .96f, 24, _mint);
            Label(CreateText("Hint", frame.transform, "", .04f, .81f, .96f, .86f, 13, _cream), "Type to edit.  SAVE keeps the text on this computer.  ESC closes.");
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
            AnimateCard();
            AnimateDeck();
            AnimateCall();
            AnimateBanner();
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
            if (_terminal != null && _terminal.activeSelf) TerminalCloseClicked?.Invoke();
            else if (_menu != null && _menu.activeSelf) MenuCloseClicked?.Invoke();
            else if (_deck != null && _deck.activeSelf) DeckCloseClicked?.Invoke();
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
            HideDeck();
        }

        public void ShowLocation(string place) => _location.text = place ?? string.Empty;

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

        // Writes every fixed label again in the language the translator returns.
        public void Localize(Func<string, string> translate)
        {
            _t = translate ?? (key => key);
            foreach ((Text text, string key) in _labels) if (text != null) text.text = _t(key);
            if (_deckPlayLabel != null) ShowDeckState();
        }

        private Text Label(Text text, string key)
        {
            _labels.Add((text, key));
            text.text = _t(key);
            return text;
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
            button.onClick.AddListener(() => { if (_click != null) _ui.PlayOneShot(_click, 0.8f); });
            Label(CreateText("Label", panel.transform, caption, .08f, .05f, .92f, .95f, 20, _cream), caption).alignment = TextAnchor.MiddleCenter;
            return button;
        }
    }
}
