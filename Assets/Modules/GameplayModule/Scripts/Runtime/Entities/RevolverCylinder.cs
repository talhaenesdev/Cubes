using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // Sits on the rig root, outside the flipping body, so it always reads upright over the head.
    // The swung-out cylinder shown while reloading any weapon: it turns one chamber per shell and a shell slides into the next empty one.
    public class RevolverCylinder : MonoBehaviour
    {
        [SerializeField] private Transform _wheel;
        [SerializeField] private SpriteRenderer[] _chambers = new SpriteRenderer[6];
        [SerializeField] private Transform _shell;
        [SerializeField] private Color _loaded = new(1f, 0.65f, 0.30f);
        [SerializeField] private Color _empty = new(0.025f, 0.035f, 0.055f);
        [SerializeField] private Vector2 _shellStart = new(0.45f, 0.5f);

        // A magazine larger than the chambers drawn fills them in proportion.
        public void Apply(bool visible, int ammo, int magazine, float progress)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
            if (!visible) return;
            int shown = Mathf.Clamp(magazine, 1, _chambers.Length);
            int filled = magazine <= _chambers.Length ? ammo : Mathf.RoundToInt(ammo / (float) magazine * shown);
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            if (_wheel != null) _wheel.localRotation = Quaternion.Euler(0f, 0f, (filled + eased) * 360f / shown);
            for (int i = 0; i < _chambers.Length; i++)
            {
                if (_chambers[i] == null) continue;
                _chambers[i].enabled = i < shown;
                _chambers[i].color = i < filled ? _loaded : _empty;
            }
            if (_shell == null) return;
            float slide = Mathf.SmoothStep(0f, 1f, progress * 1.4f);
            _shell.localPosition = Vector2.Lerp(_shellStart, new Vector2(0f, 0.2f), slide);
            _shell.localRotation = Quaternion.Euler(0f, 0f, 35f * (1f - progress));
        }
    }
}
