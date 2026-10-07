using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // The hideout cat: pads along the floor, sits a while, swishes its tail and blinks. Only for looks.
    public class CatView : MonoBehaviour
    {
        [SerializeField] private Transform _body;
        [SerializeField] private Transform _tail;
        [SerializeField] private Transform[] _legs = new Transform[0];
        [SerializeField] private GameObject _eyesOpen;
        [SerializeField] private GameObject _eyesShut;
        [SerializeField] private float _left = -6f;
        [SerializeField] private float _right = 6f;
        [SerializeField] private float _speed = 0.8f;

        private float _target;
        private float _rest;
        private float _blink = 3f;
        private float _step;

        private void OnEnable()
        {
            _target = transform.localPosition.x;
            _rest = Random.Range(1f, 3f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector3 p = transform.localPosition;
            bool walking = _rest <= 0f && Mathf.Abs(_target - p.x) > 0.05f;
            if (_rest > 0f)
            {
                _rest -= dt;
                if (_rest <= 0f) _target = Random.Range(_left, _right);
            }
            else if (!walking) _rest = Random.Range(2.5f, 7f);
            if (walking)
            {
                float dir = Mathf.Sign(_target - p.x);
                p.x = Mathf.MoveTowards(p.x, _target, _speed * dt);
                transform.localPosition = p;
                transform.localScale = new Vector3(dir, 1f, 1f);
                _step += dt * 10f;
            }
            for (int i = 0; i < _legs.Length; i++)
                if (_legs[i] != null) _legs[i].localRotation = Quaternion.Euler(0f, 0f, walking ? Mathf.Sin(_step + i * Mathf.PI) * 25f : 0f);
            if (_body != null) _body.localPosition = new Vector3(0f, walking ? Mathf.Abs(Mathf.Sin(_step)) * 0.02f : 0f, 0f);
            if (_tail != null) _tail.localRotation = Quaternion.Euler(0f, 0f, (walking ? 35f : 15f) + Mathf.Sin(Time.time * (walking ? 6f : 1.6f)) * 18f);
            _blink -= dt;
            bool shut = _blink < 0.12f;
            if (_blink < 0f) _blink = Random.Range(2f, 5f);
            if (_eyesOpen != null) _eyesOpen.SetActive(!shut);
            if (_eyesShut != null) _eyesShut.SetActive(shut);
        }
    }
}
