using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    // Zooms and pans the city map inside its frame: two fingers pinch, the mouse wheel rolls, one finger or the mouse
    // drags. The shop pins keep their size while the streets grow under them, and the map never leaves the frame.
    [RequireComponent(typeof(RectTransform))]
    public class MapZoom : MonoBehaviour, IDragHandler, IScrollHandler
    {
        private const float MaxZoom = 3f;
        public RectTransform Content;
        private float _zoom = 1f;
        private float _pinch;

        private RectTransform Viewport => (RectTransform) transform;

        private Camera EventCamera
        {
            get
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                return canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            }
        }

        // Every opening starts with the whole city in view.
        private void OnEnable()
        {
            _zoom = 1f;
            _pinch = 0f;
            if (Content == null) return;
            Content.localScale = Vector3.one;
            Content.anchoredPosition = Vector2.zero;
            KeepPinsSized();
        }

        public void OnScroll(PointerEventData eventData) => ZoomAt(eventData.position, 1f + eventData.scrollDelta.y * 0.1f);

        public void OnDrag(PointerEventData eventData)
        {
            // Two fingers down is a pinch, not a drag.
            if (Content == null || TouchesOnMap(out _, out _) >= 2) return;
            Vector2 scale = new(Viewport.lossyScale.x, Viewport.lossyScale.y);
            Content.anchoredPosition += eventData.delta / new Vector2(Mathf.Max(0.0001f, scale.x), Mathf.Max(0.0001f, scale.y));
            Clamp();
        }

        private void Update()
        {
            if (Content == null) return;
            if (TouchesOnMap(out Vector2 a, out Vector2 b) < 2)
            {
                _pinch = 0f;
                return;
            }
            float span = Vector2.Distance(a, b);
            if (_pinch > 0f && span > 0f) ZoomAt((a + b) * 0.5f, span / _pinch);
            _pinch = span;
        }

        private void LateUpdate() => KeepPinsSized();

        private int TouchesOnMap(out Vector2 a, out Vector2 b)
        {
            a = b = default;
            Touchscreen screen = Touchscreen.current;
            if (screen == null) return 0;
            int found = 0;
            foreach (var touch in screen.touches)
            {
                if (!touch.press.isPressed) continue;
                Vector2 at = touch.position.ReadValue();
                if (!RectTransformUtility.RectangleContainsScreenPoint(Viewport, at, EventCamera)) continue;
                if (found == 0) a = at;
                else if (found == 1) b = at;
                found++;
            }
            return found;
        }

        // Scales the map by `factor`, keeping the spot under `screenPoint` where it is.
        private void ZoomAt(Vector2 screenPoint, float factor)
        {
            if (Content == null || factor <= 0f) return;
            float zoom = Mathf.Clamp(_zoom * factor, 1f, MaxZoom);
            if (Mathf.Approximately(zoom, _zoom)) return;
            Camera camera = EventCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Content, screenPoint, camera, out Vector2 local);
            Vector3 before = Content.TransformPoint(local);
            _zoom = zoom;
            Content.localScale = new Vector3(zoom, zoom, 1f);
            Vector3 after = Content.TransformPoint(local);
            Content.position += before - after;
            Clamp();
            KeepPinsSized();
        }

        // The map's edges never come inside the frame.
        private void Clamp()
        {
            Vector2 size = Viewport.rect.size;
            Vector2 room = size * (_zoom - 1f) * 0.5f;
            Vector2 at = Content.anchoredPosition;
            Content.anchoredPosition = new Vector2(Mathf.Clamp(at.x, -room.x, room.x), Mathf.Clamp(at.y, -room.y, room.y));
        }

        private void KeepPinsSized()
        {
            if (Content == null) return;
            Vector3 pin = new(1f / _zoom, 1f / _zoom, 1f);
            for (int i = 0; i < Content.childCount; i++)
            {
                Transform child = Content.GetChild(i);
                if (child.name.StartsWith("Shop ") && child.localScale != pin) child.localScale = pin;
            }
        }
    }
}
