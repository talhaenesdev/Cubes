using FlowIoC.PoolModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A one-shot effect: muzzle flash, sparks, blood, a casing. Every ParticleSystem and AudioSource under it
    // plays when it comes out of the pool, and it goes home after Duration. Its +X is the direction it was fired in.
    public class PooledEffect : PoolableItem
    {
        [SerializeField] private float _duration = 1.5f;
        [Tooltip("Keep 'up' up when fired to the left, so smoke rises and casings eject upward either way.")]
        [SerializeField] private bool _keepUpright = true;

        private ParticleSystem[] _systems = new ParticleSystem[0];
        private AudioSource[] _sounds = new AudioSource[0];
        private float _age;

        public override void OnInitialized()
        {
            _systems = GetComponentsInChildren<ParticleSystem>(true);
            _sounds = GetComponentsInChildren<AudioSource>(true);
        }

        public void Play(Vector2 position, Vector2 direction)
        {
            transform.localPosition = new Vector3(position.x, position.y, 0f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            transform.localScale = new Vector3(1f, _keepUpright && direction.x < 0f ? -1f : 1f, 1f);
            _age = 0f;
            foreach (ParticleSystem system in _systems)
            {
                system.Clear(true);
                system.Play(true);
            }
            foreach (AudioSource sound in _sounds)
            {
                sound.pitch = Random.Range(0.92f, 1.08f);
                sound.Play();
            }
        }

        public override void OnReturnToPool()
        {
            foreach (ParticleSystem system in _systems) system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= _duration) Dismiss();
        }
    }
}
