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
        [Tooltip("Where the pocket weapon hangs; made at runtime under the torso when empty.")]
        [SerializeField] private Transform _holsterSocket;
        [Tooltip("The hip, relative to the torso, used when the socket is made at runtime.")]
        [SerializeField] private Vector3 _holsterOffset = new(-0.18f, -0.42f, 0f);
        private GameObject _holsterPrefab;
        private GameObject _holstered;
        [Tooltip("Shown in the hand instead of the weapon while the player starts the music at a contract's start.")]
        [SerializeField] private GameObject _phone;
        [SerializeField] private SpriteRenderer _phoneScreen;
        [Tooltip("Worn through a contract; put on at its start, before the phone comes out.")]
        [SerializeField] private Transform _headphones;
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

        [Header("Outfit (dyed from CD_Outfits; empty on characters that wear none)")]
        [SerializeField] private SpriteRenderer[] _outfitCoat = new SpriteRenderer[0];
        [SerializeField] private SpriteRenderer[] _outfitCoatShade = new SpriteRenderer[0];
        [SerializeField] private SpriteRenderer[] _outfitWaistcoat = new SpriteRenderer[0];
        [SerializeField] private SpriteRenderer[] _outfitShirt = new SpriteRenderer[0];
        [SerializeField] private SpriteRenderer[] _outfitTie = new SpriteRenderer[0];
        [SerializeField] private SpriteRenderer[] _outfitHat = new SpriteRenderer[0];
        [SerializeField] private SpriteRenderer[] _outfitHatBand = new SpriteRenderer[0];
        [SerializeField] private SpriteRenderer[] _outfitScarf = new SpriteRenderer[0];
        [SerializeField] private GameObject[] _outfitHatParts = new GameObject[0];
        [SerializeField] private GameObject[] _outfitScarfParts = new GameObject[0];
        [SerializeField] private GameObject[] _outfitCigarette = new GameObject[0];

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
        // The weapon carried in the pocket, worn at the hip, muzzle down; null leaves the hip empty.
        public void SetHolster(GameObject prefab)
        {
            if (prefab == _holsterPrefab) return;
            _holsterPrefab = prefab;
            if (_holstered != null)
            {
                // Out of the hierarchy at once, so the tint list rebuilt below no longer finds it.
                _holstered.transform.SetParent(null, false);
                Destroy(_holstered);
                _holstered = null;
            }
            if (prefab != null)
            {
                Transform hip = HolsterSocket();
                if (hip != null)
                {
                    _holstered = Instantiate(prefab, hip, false);
                    _holstered.transform.localPosition = Vector3.zero;
                    _holstered.transform.localRotation = Quaternion.identity;
                    _holstered.transform.localScale = Vector3.one * 0.8f;
                    // Behind the coat's front, in front of the back leg.
                    foreach (SpriteRenderer part in _holstered.GetComponentsInChildren<SpriteRenderer>(true)) part.sortingOrder -= 8;
                    foreach (Transform child in _holstered.GetComponentsInChildren<Transform>(true))
                        if (child.name == "Muzzle") child.gameObject.SetActive(false);
                }
            }
            Recache(_weapon == null ? null : _weapon.transform);
        }

        // The hip: made once under the torso if the prefab has none of its own.
        private Transform HolsterSocket()
        {
            if (_holsterSocket != null) return _holsterSocket;
            Transform parent = _torso != null ? _torso : _body;
            if (parent == null) return null;
            _holsterSocket = new GameObject("Holster").transform;
            _holsterSocket.SetParent(parent, false);
            _holsterSocket.localPosition = _holsterOffset;
            _holsterSocket.localRotation = Quaternion.Euler(0f, 0f, -100f);
            return _holsterSocket;
        }

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

        // The swing: wound back, thrown forward, the body lunging behind it.
        public void Punch() => _punch = 1f;

        // A hit lands: every part goes white for an instant.
        public void Flash() => _flash = 1f;

        private float _punch;
        private float _flash;
        private const float PunchTime = 0.28f;

        private string _wearing;

        // Dyes the coat, waistcoat, shirt, tie, hat and scarf and shows or hides the hat, scarf and cigarette.
        public void Wear(Data.ValueObjects.OutfitCVO outfit)
        {
            if (outfit == null || outfit.Id == _wearing) return;
            _wearing = outfit.Id;
            Dye(_outfitCoat, outfit.Coat);
            Dye(_outfitCoatShade, outfit.Coat * new Color(0.6f, 0.6f, 0.6f, 1f));
            Dye(_outfitWaistcoat, outfit.Waistcoat);
            Dye(_outfitShirt, outfit.Shirt);
            Dye(_outfitTie, outfit.Tie);
            Dye(_outfitHat, outfit.Hat);
            Dye(_outfitHatBand, outfit.HatBand);
            Dye(_outfitScarf, outfit.Scarf);
            Show(_outfitHatParts, outfit.WearHat);
            Show(_outfitScarfParts, outfit.WearScarf);
            Show(_outfitCigarette, outfit.Cigarette);
            if (_outfitCoat.Length > 0) _coatColor = outfit.Coat;
            Recache(_weapon == null ? null : _weapon.transform);
        }

        private void Dye(SpriteRenderer[] parts, Color color)
        {
            foreach (SpriteRenderer part in parts)
                if (part != null) part.color = new Color(color.r, color.g, color.b, part.color.a);
        }

        private void Show(GameObject[] parts, bool on)
        {
            foreach (GameObject part in parts)
                if (part != null) part.SetActive(on);
        }

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
            PosePhone(pose);
            PoseHeadphones(pose);
            PosePunch(pose);
            PoseTrackSwitch(pose);
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

        // Raise the phone to the face, tap, hold while the music starts, lower it and take the weapon back.
        private void PosePhone(CharacterPoseVO pose)
        {
            bool using_ = pose.Alive && pose.Phone >= 0f && pose.Phone < 1f;
            if (_phone != null && _phone.activeSelf != using_) _phone.SetActive(using_);
            if (_weaponSocket != null && _weaponSocket.gameObject.activeSelf == using_) _weaponSocket.gameObject.SetActive(!using_);
            if (!using_ || _armPivot == null) return;
            float t = pose.Phone;
            float raise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.22f)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.68f) / 0.25f)));
            float tap = t > 0.3f && t < 0.38f ? Mathf.Sin((t - 0.3f) / 0.08f * Mathf.PI) * 6f : 0f;
            _armPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-60f, 72f, raise) - tap);
            if (_phoneScreen != null)
                _phoneScreen.color = t >= 0.35f ? new Color(0.75f, 0.95f, 1f) : new Color(0.25f, 0.35f, 0.4f);
        }

        // Hung round the neck, lifted up over the ears with the hand, then worn.
        private void PoseHeadphones(CharacterPoseVO pose)
        {
            if (_headphones == null) return;
            bool worn = pose.Alive && pose.Headphones >= 0f;
            if (_headphones.gameObject.activeSelf != worn) _headphones.gameObject.SetActive(worn);
            if (!worn) return;
            float t = Mathf.SmoothStep(0f, 1f, pose.Headphones);
            _headphones.localPosition = new Vector3(0f, Mathf.Lerp(-0.45f, 0f, t), 0f);
            if (pose.Headphones >= 1f || _armPivot == null) return;
            float reach = Mathf.Sin(Mathf.Clamp01(pose.Headphones) * Mathf.PI);
            _armPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-60f, 110f, reach));
            if (_weaponSocket != null && _weaponSocket.gameObject.activeSelf) _weaponSocket.gameObject.SetActive(false);
        }

        // The weapon lowered and away, the hand up to the ear cup, a tap, and the weapon back in hand.
        private void PoseTrackSwitch(CharacterPoseVO pose)
        {
            bool switching = pose.Alive && pose.TrackSwitch >= 0f && pose.TrackSwitch < 1f;
            if (!switching)
            {
                if (_trackSwitching && _weaponSocket != null && pose.Phone < 0f) _weaponSocket.gameObject.SetActive(true);
                _trackSwitching = false;
                return;
            }
            _trackSwitching = true;
            if (_weaponSocket != null && _weaponSocket.gameObject.activeSelf) _weaponSocket.gameObject.SetActive(false);
            if (_armPivot == null) return;
            float t = pose.TrackSwitch;
            float reach = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.35f)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.7f) / 0.3f)));
            // A small press against the cup at the tap.
            float tap = t > 0.45f && t < 0.58f ? Mathf.Sin((t - 0.45f) / 0.13f * Mathf.PI) * 8f : 0f;
            _armPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-60f, 112f, reach) - tap);
        }

        private bool _trackSwitching;

        private void PosePunch(CharacterPoseVO pose)
        {
            if (_punch <= 0f || !pose.Alive) { _punch = 0f; if (_body != null) _body.localPosition = new Vector3(0f, _body.localPosition.y, 0f); return; }
            _punch = Mathf.Max(0f, _punch - Time.deltaTime / PunchTime);
            float t = 1f - _punch;
            // A quick wind-up behind, then the arm thrown straight out, then back.
            float swing = t < 0.25f ? Mathf.Lerp(0f, -55f, t / 0.25f) : t < 0.5f ? Mathf.Lerp(-55f, 15f, (t - 0.25f) / 0.25f) : Mathf.Lerp(15f, 0f, (t - 0.5f) / 0.5f);
            if (_armPivot != null) _armPivot.localRotation = Quaternion.Euler(0f, 0f, swing);
            float lunge = Mathf.Sin(Mathf.Clamp01((t - 0.2f) / 0.5f) * Mathf.PI) * 0.18f;
            if (_body != null) _body.localPosition = new Vector3(lunge, _body.localPosition.y, 0f);
        }

        private void Tint(CharacterPoseVO pose)
        {
            float alpha = pose.Hidden ? _hiddenAlpha : 1f;
            _flash = Mathf.Max(0f, _flash - Time.unscaledDeltaTime * 9f);
            for (int i = 0; i < _renderers.Count; i++)
            {
                Color color = _flash > 0f ? Color.Lerp(_colors[i], Color.white, _flash * 0.85f) : _colors[i];
                color.a = _colors[i].a * alpha;
                _renderers[i].color = color;
            }
            Color coat = Color.Lerp(_coatColor, _alertColor, pose.Alert * 0.65f);
            if (!pose.Alive) coat *= new Color(0.65f, 0.65f, 0.65f, 1f);
            if (_flash > 0f) coat = Color.Lerp(coat, Color.white, _flash * 0.85f);
            coat.a = alpha;
            foreach (SpriteRenderer part in _coat) part.color = coat;
        }

        private void Mark(GameObject mark, bool on)
        {
            if (mark != null && mark.activeSelf != on) mark.SetActive(on);
        }
    }
}
