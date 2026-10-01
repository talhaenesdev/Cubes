using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // Shown over the head during a whole-magazine reload: the empty magazine drops out of the grip, a full one
    // rises in and clicks home, and the bar underneath fills with the reload.
    public class MagazineSwap : MonoBehaviour
    {
        [SerializeField] private Transform _empty;
        [SerializeField] private SpriteRenderer _emptyRenderer;
        [SerializeField] private Transform _full;
        [SerializeField] private Transform _bar;
        [SerializeField] private Vector2 _seated = new(0.05f, -0.12f);
        [SerializeField] private float _drop = 0.55f;
        [Tooltip("Share of the reload spent pulling the empty magazine out.")]
        [SerializeField, Range(0.1f, 0.9f)] private float _outShare = 0.4f;

        public void Apply(bool visible, float progress)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
            if (!visible) return;
            float t = Mathf.Clamp01(progress);
            float out01 = Mathf.Clamp01(t / _outShare);
            float in01 = Mathf.Clamp01((t - _outShare) / (1f - _outShare));
            if (_empty != null)
            {
                _empty.localPosition = _seated + Vector2.down * (_drop * out01 * out01);
                _empty.localRotation = Quaternion.Euler(0f, 0f, -25f * out01);
            }
            if (_emptyRenderer != null)
            {
                Color c = _emptyRenderer.color;
                c.a = 1f - out01;
                _emptyRenderer.color = c;
            }
            if (_full != null)
            {
                _full.gameObject.SetActive(t > _outShare);
                // Eases in and overshoots a touch, the click of the magazine seating.
                float rise = 1f - Mathf.Pow(1f - in01, 3f);
                float click = in01 > 0.85f ? Mathf.Sin((in01 - 0.85f) / 0.15f * Mathf.PI) * 0.03f : 0f;
                _full.localPosition = _seated + Vector2.down * (_drop * (1f - rise)) + Vector2.up * click;
            }
            if (_bar != null) _bar.localScale = new Vector3(Mathf.Max(0.001f, t), 1f, 1f);
        }
    }
}
