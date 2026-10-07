using System;
using FlowIoC.BaseModule.Injectable.Components;
using FlowIoC.ScreenModule.ViewsMediators.Screen;
using Modules.LoadingModule.Shared.Data.ValueObjects;
using Modules.LoadingModule.Shared.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.LoadingModule.LoadingScreenModule.ViewsMediators
{
    /// <summary>
    /// The big bar. It draws whatever snapshot it is handed and remembers only which set that was;
    /// the smoothing towards the target is the one piece of motion it owns. The art behind it is
    /// the one thing not reset between openings: what the addressable load brought is the same for
    /// every set, so it stays on the pooled instance.
    /// </summary>
    [RequireComponent(typeof(ViewInjector))]
    public class LoadingScreenView : ScreenView
    {
        private const float SMOOTH_SPEED = 8f;

        [SerializeField] private Image _background;
        [SerializeField] private Image _fill;
        [SerializeField] private Text _percent;
        [SerializeField] private Text _message;
        [SerializeField] private Text _detail;

        [SerializeField] private GameObject _childGroup;
        [SerializeField] private Image _childFill;
        [SerializeField] private Text _childMessage;
        [SerializeField] private Text _childDetail;

        [SerializeField] private GameObject _failedGroup;
        [SerializeField] private Text _failedText;
        [SerializeField] private Button _retryButton;

        private readonly Color _gold = new(0.77f, 0.63f, 0.36f, 1f);
        private bool _styled;
        private Color _messageColor;
        private float _target;
        private float _shown;
        private float _childTarget;
        private float _childShown;

        public Action Retry;

        /// <summary>The set this screen is drawing, or null before the first Apply.</summary>
        public string ShowingSet { get; private set; }

        private void OnEnable() => _retryButton.onClick.AddListener(RetryClicked);

        private void OnDisable() => _retryButton.onClick.RemoveListener(RetryClicked);

        public void RetryClicked() => Retry?.Invoke();

        public override void BeforeScreenActivation()
        {
            base.BeforeScreenActivation();
            Style();

            ShowingSet = null;
            _target = _shown = 0f;
            _childTarget = _childShown = 0f;
            SetFill(_fill, 0f);
            SetFill(_childFill, 0f);
            _percent.text = "0%";
            _message.text = string.Empty;
            _detail.text = string.Empty;
            _childGroup.SetActive(false);
            _failedGroup.SetActive(false);
        }

        public void Apply(LoadingSetStatusRVO status)
        {
            ShowingSet = status.Set;
            _target = status.Progress;
            _message.text = status.Message ?? string.Empty;
            _detail.text = status.Detail ?? string.Empty;

            _childGroup.SetActive(status.HasChild);
            if (status.HasChild)
            {
                _childTarget = status.ChildProgress;
                _childMessage.text = status.ChildMessage ?? string.Empty;
                _childDetail.text = status.ChildDetail ?? string.Empty;
            }

            if (status.State == LoadingSetState.Running)
                _failedGroup.SetActive(false);
        }

        public void ShowFailed(string step)
        {
            _failedText.text = $"Loading stopped at {step}.";
            _failedGroup.SetActive(true);
        }

        public void HideFailed() => _failedGroup.SetActive(false);

        /// <summary>The addressable art, over the sprite the prefab bundles.</summary>
        public void ShowBackground(Sprite background) => _background.sprite = background;

        private void Update()
        {
            _shown = Approach(_shown, _target);
            // A full bar on a set that is still running is waiting on the player: the message breathes.
            float pulse = _target >= 0.99f ? 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.6f)) : 1f;
            _message.color = new Color(_messageColor.r, _messageColor.g, _messageColor.b, _messageColor.a * pulse);
            SetFill(_fill, _shown);
            _percent.text = $"{Mathf.RoundToInt(_shown * 100f)}%";

            if (!_childGroup.activeSelf) return;
            _childShown = Approach(_childShown, _childTarget);
            SetFill(_childFill, _childShown);
        }

        // The fill is a plain quad stretched to a share of the track - an Image with no sprite
        // draws its whole rect whatever its fill amount says, so the width is the progress.
        private static void SetFill(Image fill, float amount) =>
            fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);

        private static float Approach(float shown, float target)
        {
            if (Mathf.Abs(target - shown) < 0.002f) return target;
            return Mathf.Lerp(shown, target, SMOOTH_SPEED * Time.unscaledDeltaTime);
        }

        // Noir dressing laid over the prefab once: a serif face, a dark vignette, gold rules and the title.
        private void Style()
        {
            if (_styled) return;
            _styled = true;
            _messageColor = _message.color;
            Font serif = Serif(_message.font);
            foreach (Text text in GetComponentsInChildren<Text>(true)) text.font = serif;
            _message.fontStyle = FontStyle.Italic;
            RectTransform backdrop = _background.rectTransform;
            Image shade = Overlay("Shade", backdrop.parent, new Color(0.03f, 0.02f, 0.02f, 0.55f));
            shade.transform.SetSiblingIndex(backdrop.GetSiblingIndex() + 1);
            Image rule = Overlay("Rule top", backdrop.parent, _gold);
            rule.rectTransform.anchorMin = new Vector2(0.08f, 0.9f);
            rule.rectTransform.anchorMax = new Vector2(0.92f, 0.903f);
            Image low = Overlay("Rule bottom", backdrop.parent, _gold);
            low.rectTransform.anchorMin = new Vector2(0.08f, 0.1f);
            low.rectTransform.anchorMax = new Vector2(0.92f, 0.103f);
            var title = new GameObject("Title", typeof(RectTransform)).AddComponent<Text>();
            title.transform.SetParent(backdrop.parent, false);
            title.rectTransform.anchorMin = new Vector2(0.1f, 0.91f);
            title.rectTransform.anchorMax = new Vector2(0.9f, 0.99f);
            title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
            title.font = serif;
            title.fontSize = 34;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = _gold;
            title.text = "C U B E S";
            title.raycastTarget = false;
        }

        private Image Overlay(string objectName, Transform parent, Color color)
        {
            var image = new GameObject(objectName, typeof(RectTransform)).AddComponent<Image>();
            image.transform.SetParent(parent, false);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // A serif face from the machine when it has one - the look is a 1940s dossier - or the built-in font.
        private Font Serif(Font fallback)
        {
            Font serif = Font.CreateDynamicFontFromOSFont(new[] {"Georgia", "Times New Roman", "Garamond", "Palatino Linotype", "DejaVu Serif"}, 32);
            return serif != null ? serif : fallback;
        }

        protected override void PlayShowAnimation() => ShowCompleted?.Invoke(this);

        protected override void PlayHideAnimation() => HideCompleted?.Invoke(this);
    }
}
