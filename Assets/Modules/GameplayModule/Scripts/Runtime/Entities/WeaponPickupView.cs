using FlowIoC.PoolModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A weapon lying on the floor. The weapon's own prefab is laid in the holder; the rest is the glint around it.
    public class WeaponPickupView : PoolableItem
    {
        [SerializeField] private Transform _holder;
        [SerializeField] private float _bob = 0.06f;
        [SerializeField] private float _bobSpeed = 3f;

        private GameObject _prefab;
        private GameObject _shown;
        private float _phase;

        public override void OnGetFromPool() => _phase = Random.value * 6f;

        public void Show(Vector2 position, GameObject weaponPrefab)
        {
            transform.localPosition = new Vector3(position.x, position.y, 0f);
            if (weaponPrefab != _prefab)
            {
                _prefab = weaponPrefab;
                if (_shown != null) Destroy(_shown);
                _shown = weaponPrefab != null && _holder != null ? Instantiate(weaponPrefab, _holder, false) : null;
            }
            if (_holder != null) _holder.localPosition = new Vector3(0f, 0.18f + Mathf.Sin(Time.time * _bobSpeed + _phase) * _bob, 0f);
        }
    }
}
