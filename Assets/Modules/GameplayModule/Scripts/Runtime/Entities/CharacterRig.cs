using System.Collections.Generic;
using FlowIoC.PoolModule.Entities;
using Modules.GameplayModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // The player and every guard. The hierarchy is the prefab's to change - swap sprites, add parts,
    // retune the numbers below; the rig only moves the transforms it is given.
    //   Root (this) → Pose (death rotation) → Body (facing flip, crouch) → legs, torso, head, ArmPivot → weapon
    public class CharacterRig : PoolableItem
    {
        [Header("Parts")]
        [SerializeField] private Transform _pose;
        [SerializeField] private Transform _body;
        [SerializeField] private Transform _legFront;
        [SerializeField] private Transform _legBack;
        [SerializeField] private Transform _torso;
        [SerializeField] private Transform _armPivot;
        [Tooltip("The held weapon's prefab is placed here; +X points down the barrel.")]
        [SerializeField] private Transform _weaponSocket;
        [SerializeField] private SpriteRenderer[] _coat = new SpriteRenderer[0];
        [SerializeField] private RevolverCylinder _cylinder;
        [SerializeField] private MagazineSwap _magazine;

        [Header("Marks")]
        [SerializeField] private GameObject _spottedMark;
        [SerializeField] private GameObject _investigateMark;
        [SerializeField] private GameObject _stunMark;
        [SerializeField] private GameObject _ventEyes;
        [SerializeField] private Transform _shadow;
        [SerializeField] private Transform _bloodPool;

        [Header("Look")]
        [SerializeField] private Color _coatColor = new(0.28f, 0.25f, 0.30f);
        [SerializeField] private Color _alertColor = new(0.95f, 0.22f, 0.30f);
        [SerializeField, Range(0f, 1f)] private float _hiddenAlpha = 0.55f;

        [Header("Motion")]
        [SerializeField] private float _crouchScale = 0.68f;
        [SerializeField] private float _strideAngle = 28f;
        [SerializeField] private float _strideSpeed = 13f;
        [SerializeField] private float _recoilAngle = 24f;
        [SerializeField] private float _recoilRecovery = 7f;
        [SerializeField] private float _deathDuration = 0.8f;
        [SerializeField] private float _deathSlide = 0.6f;

        private readonly List<SpriteRenderer> _renderers = new();
        private readonly List<Color> _colors = new();
        private bool _wasAlive = true;
        private float _dead;
        private float _fallDirection = 1f;
        private float _walk;
        private float _recoil;
        private float _facing = 1f;

        private GameObject _weaponPrefab;
        private GameObject _weapon;

        public override void OnInitialized() => Recache();

        // Puts the weapon's prefab in the hand; null leaves the hand empty. Nothing happens when it is already held.
        public void SetWeapon(GameObject prefab)
        {
            if (prefab == _weaponPrefab || _weaponSocket == null) return;
            _weaponPrefab = prefab;
            if (_weapon != null) Destroy(_weapon);
            _weapon = prefab != null ? Instantiate(prefab, _weaponSocket, false) : null;
            if (_weapon != null)
            {
                _weapon.transform.localPosition = Vector3.zero;
                _weapon.transform.localRotation = Quaternion.identity;
            }
            // Destroy happens at the end of the frame; leave the old weapon out of the tint list now.
            Recache(_weapon == null ? null : _weapon.transform);
        }

        private void Recache(Transform keep = null)
        {
            _renderers.Clear();
            _colors.Clear();
            foreach (SpriteRenderer part in GetComponentsInChildren<SpriteRenderer>(true))
            {
                bool oldWeapon = _weaponSocket != null && part.transform.IsChildOf(_weaponSocket) && (keep == null || !part.transform.IsChildOf(keep));
                if (oldWeapon) continue;
                _renderers.Add(part);
                _colors.Add(part.color);
            }
        }

        public override void OnGetFromPool()
        {
            _wasAlive = true;
            _dead = 0f;
            _walk = 0f;
            _recoil = 0f;
        }

        public void Kick() => _recoil = 1f;

        public void Apply(CharacterPoseVO pose)
        {
            float dt = Time.deltaTime;
            transform.localPosition = pose.Position;
            if (_wasAlive && !pose.Alive) _fallDirection = pose.FallDirection >= 0f ? 1f : -1f;
            _wasAlive = pose.Alive;
            _dead = pose.Alive ? 0f : _dead + dt;
            _recoil = Mathf.Max(0f, _recoil - dt * _recoilRecovery);
            if (pose.Alive) _facing = pose.Aim.x < pose.Position.x ? -1f : 1f;
            else _facing = -_fallDirection;

            _pose.gameObject.SetActive(!pose.InVent);
            if (_ventEyes != null) _ventEyes.SetActive(pose.InVent);
            PoseDeath();
            _body.localScale = new Vector3(_facing, pose.Crouching && pose.Alive ? _crouchScale : 1f, 1f);
            PoseLegs(pose);
            PoseArm(pose);
            Tint(pose);
            bool reloading = pose.Alive && pose.Reloading;
            if (_cylinder != null) _cylinder.Apply(reloading && (!pose.MagazineReload || _magazine == null), pose.Ammo, pose.Magazine, pose.ReloadProgress);
            if (_magazine != null) _magazine.Apply(reloading && pose.MagazineReload, pose.ReloadProgress);
            Mark(_spottedMark, pose.Alive && pose.Spotted && !pose.Stunned);
            Mark(_investigateMark, pose.Alive && pose.Investigating && !pose.Spotted && !pose.Stunned);
            Mark(_stunMark, pose.Alive && pose.Stunned);
            if (_stunMark != null && pose.Stunned) _stunMark.transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 240f);
        }

        // A jolt back from the hit, knees buckling, a topple that settles with a small bounce.
        private void PoseDeath()
        {
            float t = Mathf.Clamp01(_dead / _deathDuration);
            float topple = t < 0.7f ? 1f - Mathf.Pow(1f - t / 0.7f, 3f) : 1f + Mathf.Sin((t - 0.7f) / 0.3f * Mathf.PI) * 0.06f;
            if (_dead <= 0f) topple = 0f;
            float jolt = Mathf.Sin(Mathf.Clamp01(t * 3f) * Mathf.PI * 0.5f) * 0.25f;
            _pose.localRotation = Quaternion.Euler(0f, 0f, -90f * _fallDirection * topple);
            _pose.localPosition = _dead <= 0f ? Vector3.zero
                : new Vector3(_fallDirection * (jolt + _deathSlide * Mathf.Clamp01(topple)), 0.19f * Mathf.Clamp01(topple), 0f);
            if (_shadow != null)
                _shadow.localScale = new Vector3(Mathf.Lerp(0.8f, 1.65f, Mathf.Clamp01(topple)), 0.07f, 1f);
            if (_bloodPool == null) return;
            float pool = Mathf.Clamp01((_dead - 0.6f) / 2.5f);
            _bloodPool.gameObject.SetActive(_dead > 0.6f);
            _bloodPool.localPosition = new Vector3(_fallDirection * 1.25f, 0.025f, 0f);
            _bloodPool.localScale = new Vector3(1.3f * Mathf.Sqrt(pool), 0.045f, 1f);
        }

        private void PoseLegs(CharacterPoseVO pose)
        {
            if (pose.Moving && pose.Alive) _walk += Time.deltaTime * _strideSpeed;
            else _walk = Mathf.MoveTowards(_walk, Mathf.Round(_walk / Mathf.PI) * Mathf.PI, Time.deltaTime * _strideSpeed);
            float swing = Mathf.Sin(_walk) * _strideAngle;
            if (_legFront != null) _legFront.localRotation = Quaternion.Euler(0f, 0f, swing);
            if (_legBack != null) _legBack.localRotation = Quaternion.Euler(0f, 0f, -swing);
            if (_torso != null) _torso.localPosition = new Vector3(_torso.localPosition.x, TorsoRest + Mathf.Abs(Mathf.Sin(_walk)) * 0.04f, 0f);
        }

        private float TorsoRest => _torsoRest ??= _torso != null ? _torso.localPosition.y : 0f;
        private float? _torsoRest;

        private void PoseArm(CharacterPoseVO pose)
        {
            if (_armPivot == null) return;
            float angle;
            if (!pose.Alive) angle = Mathf.Lerp(-10f, -80f, Mathf.Clamp01(_dead / _deathDuration));
            else if (pose.Reloading) angle = 70f;
            else
            {
                Vector2 shoulder = _armPivot.position;
                Vector2 toAim = pose.Aim - shoulder;
                float world = Mathf.Atan2(toAim.y, toAim.x) * Mathf.Rad2Deg;
                angle = _facing > 0f ? world : 180f - world;
            }
            angle += _recoil * _recoilAngle;
            _armPivot.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Tint(CharacterPoseVO pose)
        {
            float alpha = pose.Hidden ? _hiddenAlpha : 1f;
            for (int i = 0; i < _renderers.Count; i++)
            {
                Color color = _colors[i];
                color.a *= alpha;
                _renderers[i].color = color;
            }
            Color coat = Color.Lerp(_coatColor, _alertColor, pose.Alert * 0.65f);
            if (!pose.Alive) coat *= new Color(0.65f, 0.65f, 0.65f, 1f);
            coat.a = alpha;
            foreach (SpriteRenderer part in _coat) part.color = coat;
        }

        private void Mark(GameObject mark, bool on)
        {
            if (mark != null && mark.activeSelf != on) mark.SetActive(on);
        }
    }
}
