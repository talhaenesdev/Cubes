using FlowIoC.PoolModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A bundle of bills a dead guard dropped: bobs a little and glints until it is picked up.
    public class CashPickupView : PoolableItem
    {
        [SerializeField] private Transform _bundle;
        [SerializeField] private SpriteRenderer _glint;
        private float _phase;

        public override void OnGetFromPool() => _phase = Random.value * 6f;

        public void Show(Vector2 position)
        {
            transform.localPosition = new Vector3(position.x, position.y, 0f);
            if (_bundle != null) _bundle.localPosition = new Vector3(0f, 0.14f + Mathf.Sin(Time.time * 3f + _phase) * 0.04f, 0f);
            if (_glint == null) return;
            Color color = _glint.color;
            color.a = 0.25f + 0.25f * Mathf.Max(0f, Mathf.Sin(Time.time * 4f + _phase));
            _glint.color = color;
        }
    }
}
