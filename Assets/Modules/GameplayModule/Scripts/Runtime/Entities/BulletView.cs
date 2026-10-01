using FlowIoC.PoolModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A bullet in flight. The run moves it; this only follows, pointing along its velocity, trailing behind.
    public class BulletView : PoolableItem
    {
        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private Color _playerColor = new(1f, 0.96f, 0.82f);
        [SerializeField] private Color _guardColor = new(1f, 0.55f, 0.45f);

        private bool _placed;

        public override void OnGetFromPool()
        {
            _placed = false;
            if (_trail != null) _trail.emitting = false;
        }

        public override void OnReturnToPool()
        {
            if (_trail != null) _trail.Clear();
        }

        public void Follow(Vector2 position, Vector2 velocity, bool fromPlayer)
        {
            transform.localPosition = new Vector3(position.x, position.y, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            if (_body != null) _body.color = fromPlayer ? _playerColor : _guardColor;
            if (_placed || _trail == null) return;
            _placed = true;
            _trail.Clear();
            _trail.startColor = fromPlayer ? _playerColor : _guardColor;
            _trail.emitting = true;
        }
    }
}
