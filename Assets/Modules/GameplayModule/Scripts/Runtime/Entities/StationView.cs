using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A hideout station. What it is reads from its icon, not a label: the icon floats over it and swells and
    // glows while the player is close enough to use it with E.
    public class StationView : MonoBehaviour
    {
        public StationKind Kind = StationKind.MissionBoard;
        [SerializeField] private Transform _icon;
        [SerializeField] private SpriteRenderer _iconGlow;
        [SerializeField] private float _nearScale = 1.3f;

        private float _near;
        private Vector3 _rest;
        private bool _cached;

        public void Apply(bool near)
        {
            if (_icon == null) return;
            if (!_cached) { _rest = _icon.localPosition; _cached = true; }
            _near = Mathf.MoveTowards(_near, near ? 1f : 0f, Time.deltaTime * 5f);
            float bob = Mathf.Sin(Time.time * 2.4f) * 0.04f * (0.4f + _near);
            _icon.localPosition = _rest + new Vector3(0f, bob, 0f);
            _icon.localScale = Vector3.one * Mathf.Lerp(1f, _nearScale, _near);
            if (_iconGlow == null) return;
            Color glow = _iconGlow.color;
            glow.a = Mathf.Lerp(0.12f, 0.45f, _near);
            _iconGlow.color = glow;
        }
    }
}
