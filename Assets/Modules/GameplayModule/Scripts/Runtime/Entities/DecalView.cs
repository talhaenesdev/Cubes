using FlowIoC.PoolModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A mark a bullet left: a hole in a wall or blood on a wall or the floor. Each child is one blot;
    // the id seeds a rotation and scale so two marks never look the same.
    public class DecalView : PoolableItem
    {
        [SerializeField] private Vector2 _scaleRange = new(0.8f, 1.25f);
        [SerializeField] private bool _randomRotation = true;
        [Tooltip("On the floor the mark is squashed flat by this much.")]
        [SerializeField] private float _floorSquash = 0.25f;

        public void Place(Vector2 position, Vector2 normal, int id)
        {
            var random = new System.Random(id * 7919);
            float scale = Mathf.Lerp(_scaleRange.x, _scaleRange.y, (float) random.NextDouble());
            bool floor = normal.y > 0.5f;
            transform.localPosition = new Vector3(position.x, position.y, 0f);
            transform.localRotation = _randomRotation && !floor ? Quaternion.Euler(0f, 0f, (float) random.NextDouble() * 360f) : Quaternion.identity;
            transform.localScale = new Vector3(scale * (floor ? 2f : 1f), scale * (floor ? _floorSquash : 1f), 1f);
        }
    }
}
