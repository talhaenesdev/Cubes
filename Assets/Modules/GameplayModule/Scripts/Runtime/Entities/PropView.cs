using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // Something a bullet breaks. Intact and Broken are its two looks; Body is what drops when a lamp falls.
    // HitCenter and HitSize are the box bullets strike, measured from the marker, which stands on the floor.
    public class PropView : MonoBehaviour
    {
        public PropKind Kind = PropKind.Bottles;
        public Vector2 HitCenter = new(0f, 1f);
        public Vector2 HitSize = new(0.4f, 0.4f);
        [SerializeField] private Transform _body;
        [SerializeField] private GameObject _intact;
        [SerializeField] private GameObject _broken;

        private float _rest;
        private bool _cached;

        public void Apply(bool broken, float lift)
        {
            if (_intact != null && _intact.activeSelf == broken) _intact.SetActive(!broken);
            if (_broken != null && _broken.activeSelf != broken) _broken.SetActive(broken);
            if (_body == null) return;
            if (!_cached) { _rest = _body.localPosition.y; _cached = true; }
            _body.localPosition = new Vector3(_body.localPosition.x, _rest + lift, _body.localPosition.z);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Kind == PropKind.Barrel ? new Color(1f, 0.3f, 0.2f) : new Color(0.9f, 0.9f, 0.5f);
            Gizmos.DrawWireCube(transform.position + (Vector3) HitCenter, HitSize);
            if (Kind == PropKind.Barrel) Gizmos.DrawWireSphere(transform.position + (Vector3) HitCenter, 2.4f);
        }
    }
}
