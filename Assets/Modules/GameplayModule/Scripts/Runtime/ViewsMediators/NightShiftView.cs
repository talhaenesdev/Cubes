using System;
using System.Collections.Generic;
using FlowIoC.BaseModule.Injectable.Components;
using FlowIoC.BaseModule.ViewsMediators.View;
using FlowIoC.ConsoleModule;
using FlowIoC.PoolModule.Entities;
using FlowIoC.PoolModule.Services;
using Modules.GameplayModule.Constants;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modules.GameplayModule.ViewsMediators
{
    // Shows a run with prefabs. The building and its fixtures come from the level prefab; each character and
    // weapon is the prefab CD_Characters and CD_Weapons name for it; bullets, effects, marks and weapons on the
    // floor come from the Pool_Gameplay pool group. Nothing here decides anything.
    [RequireComponent(typeof(ViewInjector))]
    public class NightShiftView : MonoBehaviour, IView
    {
        public bool IsRegistered { get; set; }
        public event Action<ContractInputVO> InputSampled;

        [SerializeField] private LevelAuthoring _levelPrefab;
        [Tooltip("Footsteps, switches, lamp hum, the cat and the ambience.")]
        [SerializeField] private CD_Sounds _sounds;
        [Header("Camera feel")]
        [SerializeField] private float _shotShake = 0.8f;
        [Tooltip("How far the camera is knocked back from the aim by each of the player's shots.")]
        [SerializeField] private float _shotKick = 0.22f;
        [Tooltip("Real seconds the game freezes on a kill - the punch that makes a hit land.")]
        [SerializeField] private float _hitStop = 0.06f;
        [SerializeField] private float _meleeShake = 0.6f;
        [SerializeField] private float _guardShotShake = 0.35f;
        [SerializeField] private float _impactShake = 0.25f;
        [SerializeField] private float _explosionShake = 1.4f;
        [Header("Music")]
        [SerializeField] private float _musicFade = 1.2f;
        [Tooltip("The music drops to this share of its volume while the player lies dead.")]
        [SerializeField, Range(0f, 1f)] private float _deathDuck = 0.35f;
        [SerializeField] private float _shakeStrength = 0.6f;
        [SerializeField] private float _shakeRecovery = 2.2f;

        private readonly Dictionary<int, CharacterRig> _rigs = new();
        private readonly Dictionary<int, string> _rigTypes = new();
        private readonly Dictionary<int, WeaponPickupView> _pickups = new();
        private readonly Dictionary<int, CashPickupView> _loot = new();
        private readonly HashSet<int> _liveLoot = new();
        private readonly List<int> _goneLoot = new();
        private int _cashHeard;
        private readonly List<int> _gonePickups = new();
        private readonly HashSet<int> _livePickups = new();
        private CD_Characters _characters;
        private CD_Weapons _weapons;
        private CD_Outfits _outfits;
        private readonly Dictionary<BulletRVO, BulletView> _bullets = new();
        private readonly Dictionary<int, DecalView> _decals = new();
        private readonly List<BulletRVO> _gone = new();
        private readonly List<int> _goneDecals = new();
        private readonly HashSet<BulletRVO> _live = new();
        private readonly HashSet<int> _liveDecals = new();
        private IPoolService _pool;
        private LevelAuthoring _level;
        private Vector3 _cameraHome;
        private ContractRVO _last;
        private bool[] _doorsOpen = Array.Empty<bool>();
        private Vector2[] _previousGuards = Array.Empty<Vector2>();
        private bool[] _guardsAlive = Array.Empty<bool>();
        private bool _playerAlive = true;
        private float _playerFall = 1f;
        private int _shots;
        private int _guardShots;
        private int _lastImpact;
        private int _ammo;
        private float _meleeCooldown;
        private float _lastElapsed;
        private float _shake;
        private Vector2 _kick;
        private float _hitStopUntil = -1f;
        private int[] _guardHealth = System.Array.Empty<int>();
        private float _followX;
        private AudioSource _musicNow;
        private AudioSource _musicOut;
        private float _musicVolume;
        private float _duck = 1f;
        // The player's music volume setting, 0 to 1.
        private float _musicSetting = 0.8f;
        private bool _musicPaused;
        private bool _active;
        private bool _hasSnapshot;
        private Vector2 _aim;
        private Vector2 _previousPlayer;
        private GameObject _hideoutClock;
        private GameObject _hideoutDesk;
        private TextMesh _clockText;
        private TextMesh _calendarText;
        private string _lastDeviceTime;

        public void SetPool(IPoolService pool) => _pool = pool;

        public void SetMusicVolume(float volume) => _musicSetting = Mathf.Clamp01(volume);

        // Fades the playing track out and the new one in; a null clip just fades out.
        public void PlayMusic(AudioClip clip, float volume)
        {
            if (_musicNow == null)
            {
                _musicNow = MusicSource("Music A");
                _musicOut = MusicSource("Music B");
            }
            (_musicNow, _musicOut) = (_musicOut, _musicNow);
            _musicPaused = false;
            _musicVolume = clip != null ? volume : 0f;
            if (clip == null) { _musicNow.Stop(); _musicNow.volume = 0f; return; }
            _musicNow.clip = clip;
            _musicNow.volume = 0f;
            _musicNow.Play();
        }

        // Fades the playing track down and holds it where it is, or picks it up again and fades it back.
        public void PauseMusic(bool paused)
        {
            if (_musicNow == null || _musicNow.clip == null) return;
            _musicPaused = paused;
            if (!paused && !_musicNow.isPlaying) _musicNow.UnPause();
        }

        private AudioSource MusicSource(string sourceName)
        {
            var source = new GameObject(sourceName).AddComponent<AudioSource>();
            source.transform.SetParent(transform, false);
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            return source;
        }

        private void FadeMusic()
        {
            if (_musicNow == null) return;
            float step = Time.unscaledDeltaTime / Mathf.Max(0.05f, _musicFade);
            _musicNow.volume = Mathf.MoveTowards(_musicNow.volume, _musicPaused ? 0f : _musicVolume * _duck * _musicSetting, step * (_musicPaused ? 2f : 1f));
            if (_musicPaused && _musicNow.isPlaying && _musicNow.volume <= 0f) _musicNow.Pause();
            _musicOut.volume = Mathf.MoveTowards(_musicOut.volume, 0f, step);
            if (_musicOut.isPlaying && _musicOut.volume <= 0f) _musicOut.Stop();
        }

        public void SetOutfits(CD_Outfits outfits) => _outfits = outfits;

        public void SetRoster(CD_Characters characters, CD_Weapons weapons)
        {
            _characters = characters;
            _weapons = weapons;
        }

        private readonly Dictionary<LevelAuthoring, LevelAuthoring> _levels = new();
        private readonly Dictionary<LevelAuthoring, Vector3> _cameraHomes = new();
        private readonly Dictionary<LevelAuthoring, float> _cameraSizes = new();
        private float _cameraSize = 9f;
        private float _followY;
        private float _shownZoom;

        // Switches to another level prefab - the hideout, a contract - instantiating it the first time.
        public ContractLevelVO LoadLevel(LevelAuthoring prefab)
        {
            if (prefab == null) return null;
            if (!_levels.TryGetValue(prefab, out LevelAuthoring level) || level == null)
            {
                level = Instantiate(prefab, transform);
                level.name = prefab.name;
                level.gameObject.SetActive(false);
                _levels[prefab] = level;
                if (prefab.name == "PB_Level_Hideout") CreateHideoutClock(level);
                _cameraHomes[prefab] = level.Camera != null ? level.Camera.transform.localPosition : Vector3.zero;
                _cameraSizes[prefab] = level.Camera != null ? level.Camera.orthographicSize : 9f;
            }
            if (_level != level)
            {
                ReturnAll();
                if (_level != null) _level.gameObject.SetActive(false);
                _level = level;
                _cameraHome = _cameraHomes[prefab];
                _cameraSize = _cameraSizes[prefab];
                _hasSnapshot = false;
                if (_active) _level.gameObject.SetActive(true);
            }
            return _level.Collect();
        }

        // Instantiates the level prefab once and reads its markers; null when no level prefab is assigned.
        public ContractLevelVO LoadLevel()
        {
            if (_levelPrefab == null)
            {
                FlowLogger.LogError("NightShiftView has no level prefab, so nothing can be drawn. Open Tools/FlowIoC-Modules/Gameplay/Night Shift Assets and press Build missing.", context: this);
                return null;
            }
            return LoadLevel(_levelPrefab);
        }

        private void Update()
        {
            FadeMusic();
            if (!_active || _level == null || _level.Camera == null) return;
            // The run stops ticking once it is decided; keep drawing so deaths, bullets and effects finish.
            if (_last != null && (!_last.Alive || _last.Won)) Draw(_last);
            Mouse mouse = Mouse.current;
            Camera view = _level.Camera;
            // A finger on the game itself - not on the HUD's stick or buttons - aims there and fires; on a computer the mouse does.
            bool touchAim = TouchAim(out Vector2 touchAt, out bool touchDown, out bool touchHeld);
            bool mouseAim = !touchAim && mouse != null && !_touchSeen;
            if (touchAim || mouseAim)
            {
                Vector2 screen = touchAim ? touchAt : mouse.position.ReadValue();
                Vector3 world = view.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -view.transform.position.z));
                _aim = _level.transform.InverseTransformPoint(world);
            }
            bool inPlay = mouseAim && mouse.position.ReadValue().y > Screen.height * 0.105f && mouse.position.ReadValue().y < Screen.height * 0.84f;
            float stickX = 0f, stickY = 0f;
            foreach (Gamepad pad in Gamepad.all)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                if (Mathf.Abs(stick.x) > Mathf.Abs(stickX)) stickX = stick.x;
                if (Mathf.Abs(stick.y) > Mathf.Abs(stickY)) stickY = stick.y;
            }

            InputSampled?.Invoke(new ContractInputVO
            {
                Move = Mathf.Clamp((Held(Key.D) || Held(Key.RightArrow) ? 1f : 0f) - (Held(Key.A) || Held(Key.LeftArrow) ? 1f : 0f)
                                   + (Mathf.Abs(stickX) > 0.25f ? Mathf.Sign(stickX) : 0f), -1f, 1f),
                Crouch = Held(Key.LeftCtrl) || Held(Key.RightCtrl) || Held(Key.LeftShift) || Held(Key.RightShift) || Held(Key.S) || stickY < -0.55f,
                Jump = Pressed(Key.Space),
                Fire = touchAim ? touchDown : inPlay && mouse.leftButton.wasPressedThisFrame,
                FireHeld = touchAim ? touchHeld : inPlay && mouse.leftButton.isPressed,
                Melee = (mouseAim && mouse.rightButton.wasPressedThisFrame) || Pressed(Key.F),
                Interact = Pressed(Key.E),
                Reload = Pressed(Key.R),
                Swap = Pressed(Key.Q),
                NextTrack = Pressed(Key.T),
                ReloadHeld = Held(Key.R),
                Pause = Pressed(Key.Escape),
                Aim = _aim,
                Zoom = !mouseAim || PointerOverUi() ? PinchZoom() : mouse.scroll.ReadValue().y,
            });
        }

        // ---------------------------------------------------------------- touch

        // Once a finger has touched the screen the mouse no longer aims: on a phone the "mouse" is only the last tap.
        private bool _touchSeen;
        private int _aimTouch = -1;
        private float _pinch;
        private readonly List<UnityEngine.EventSystems.RaycastResult> _hits = new();
        private UnityEngine.EventSystems.PointerEventData _probe;

        // The finger aiming: the first that came down on the game and not on a control. It keeps aiming wherever it
        // slides, even over a button, until it lifts.
        private bool TouchAim(out Vector2 at, out bool down, out bool held)
        {
            at = default;
            down = held = false;
            Touchscreen screen = Touchscreen.current;
            if (screen == null) return false;
            int touching = 0;
            foreach (var touch in screen.touches)
            {
                if (!touch.press.isPressed) continue;
                touching++;
                _touchSeen = true;
                int id = touch.touchId.ReadValue();
                Vector2 position = touch.position.ReadValue();
                if (_aimTouch < 0 && touch.press.wasPressedThisFrame && !OverControl(position)) { _aimTouch = id; down = true; }
                if (id != _aimTouch) continue;
                at = position;
                held = true;
            }
            if (!held) _aimTouch = -1;
            if (touching < 2) _pinch = 0f;
            return held;
        }

        private bool OverControl(Vector2 position)
        {
            var events = UnityEngine.EventSystems.EventSystem.current;
            if (events == null) return false;
            _probe ??= new UnityEngine.EventSystems.PointerEventData(events);
            _probe.position = position;
            _hits.Clear();
            events.RaycastAll(_probe, _hits);
            return _hits.Count > 0;
        }

        // Two fingers on the game spreading apart zoom in; pinched together, out.
        private float PinchZoom()
        {
            Touchscreen screen = Touchscreen.current;
            if (screen == null) return 0f;
            Vector2 a = default, b = default;
            int found = 0;
            foreach (var touch in screen.touches)
            {
                if (!touch.press.isPressed || OverControl(touch.position.ReadValue())) continue;
                if (found == 0) a = touch.position.ReadValue();
                else if (found == 1) b = touch.position.ReadValue();
                found++;
            }
            if (found < 2) { _pinch = 0f; return 0f; }
            float span = Vector2.Distance(a, b);
            float before = _pinch;
            _pinch = span;
            if (before <= 0f) return 0f;
            // One notch of the wheel for every tenth of the screen's height the fingers move.
            float notches = (span - before) / (Screen.height * 0.1f);
            return Mathf.Abs(notches) >= 1f ? Mathf.Sign(notches) : 0f;
        }

        private static bool PointerOverUi() =>
            UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        // Every keyboard counts: the real one, and the on-screen buttons, which press keys on a keyboard of their own.
        private static bool Held(Key key)
        {
            foreach (InputDevice device in InputSystem.devices)
                if (device is Keyboard keyboard && keyboard[key].isPressed) return true;
            return false;
        }

        private static bool Pressed(Key key)
        {
            foreach (InputDevice device in InputSystem.devices)
                if (device is Keyboard keyboard && keyboard[key].wasPressedThisFrame) return true;
            return false;
        }

        public void Draw(ContractRVO state)
        {
            bool visible = state != null && state.Active && isActiveAndEnabled && _level != null;
            bool entering = visible && !_active;
            _active = visible;
            if (_level != null) _level.gameObject.SetActive(visible);
            if (!visible)
            {
                ReturnAll();
                _hasSnapshot = false;
                _last = null;
                return;
            }

            bool reset = entering || !_hasSnapshot || state.Elapsed < _lastElapsed || state.Shots < _shots;
            if (reset) Restart(state);
            Camera view = _level.Camera;
            if (view != null)
            {
                // The size authored on the level camera, widened when a narrow screen would crop the building.
                float aspect = Mathf.Max(0.1f, view.aspect);
                // A level wider than the screen keeps its authored zoom and follows the player; a small one is framed whole.
                // Zoomed in, every level follows the player, sideways and between floors.
                // The wheel's zoom eases in rather than jumping a notch at a time.
                _shownZoom = reset || _shownZoom <= 0f ? state.CameraZoom : Mathf.Lerp(_shownZoom, state.CameraZoom, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
                float zoom = Mathf.Max(0.1f, _shownZoom);
                bool follow = _level.FollowCamera || zoom > 1f;
                view.orthographicSize = follow ? _cameraSize / zoom : Mathf.Max(_cameraSize / zoom, (_level.HalfWidth + 2f) / aspect);
                _shake = Mathf.Max(0f, _shake - Time.deltaTime * _shakeRecovery);
                Vector2 jitter = UnityEngine.Random.insideUnitCircle * _shake * _shake * _shakeStrength + _kick;
                _kick = Vector2.Lerp(_kick, Vector2.zero, 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime));
                Vector3 home = _cameraHome;
                if (follow)
                {
                    float halfView = view.orthographicSize * aspect;
                    float edge = Mathf.Max(0f, _level.HalfWidth + 1f - halfView);
                    float target = Mathf.Clamp(state.Player.x + (state.Aim.x - state.Player.x) * 0.2f, -edge, edge);
                    _followX = reset ? target : Mathf.Lerp(_followX, target, 1f - Mathf.Exp(-6f * Time.deltaTime));
                    home.x = _followX;
                    if (zoom > 1f)
                    {
                        // Frame the player's floor; the view never drops below the street or above the roof it was authored to show.
                        float low = _cameraHome.y - _cameraSize + view.orthographicSize;
                        float high = _cameraHome.y + _cameraSize - view.orthographicSize;
                        float targetY = Mathf.Clamp(state.Player.y + 1.6f, low, Mathf.Max(low, high));
                        _followY = reset ? targetY : Mathf.Lerp(_followY, targetY, 1f - Mathf.Exp(-5f * Time.deltaTime));
                        home.y = _followY;
                    }
                }
                view.transform.localPosition = home + new Vector3(jitter.x, jitter.y, 0f);
            }
            if (_level.Crosshair != null)
            {
                _level.Crosshair.localPosition = state.Aim;
                if (_level.Crosshair.gameObject.activeSelf == state.InHideout) _level.Crosshair.gameObject.SetActive(!state.InHideout);
            }

            if (!reset) Emit(state);
            DrawFixtures(state);
            DrawPlayer(state, reset);
            DrawGuards(state, reset);
            DrawBullets(state);
            DrawDecals(state);
            DrawPickups(state);
            DrawLoot(state);
            PlaySounds(state, reset);

            _shots = state.Shots;
            _guardShots = state.GuardShots;
            _ammo = state.Ammo;
            _meleeCooldown = state.MeleeCooldown;
            _lastImpact = state.ImpactCount;
            _lastElapsed = state.Elapsed;
            _previousPlayer = state.Player;
            _last = state;
            _hasSnapshot = true;
        }

        // ---------------------------------------------------------------- sounds

        private AudioSource _sfx;
        private AudioSource _ambience;
        private AudioSource _hum;
        private AudioSource _purr;
        private AudioSource _ring;
        private float _playerStep;
        private float[] _guardSteps = Array.Empty<float>();
        private Vector2[] _guardHeard = Array.Empty<Vector2>();
        private bool[] _lightsWere = Array.Empty<bool>();
        private CatView _cat;
        private LevelAuthoring _catLevel;
        private float _meow = 6f;

        private AudioSource Loop(string sourceName)
        {
            var source = new GameObject(sourceName).AddComponent<AudioSource>();
            source.transform.SetParent(transform, false);
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }

        private void PlaySounds(ContractRVO state, bool reset)
        {
            if (_sounds == null) return;
            if (_sfx == null)
            {
                _sfx = Loop("Sfx");
                _sfx.loop = false;
                _sfx.volume = 1f;
                _ambience = Loop("Ambience");
                _hum = Loop("Lamp Hum");
                _purr = Loop("Purr");
                _ring = Loop("Phone Ring");
            }
            float dt = Time.deltaTime;
            Fade(_ambience, state.InHideout ? _sounds.HideoutAmbience : _sounds.ContractAmbience, _sounds.AmbienceVolume, dt);

            // The player's steps: on the boards, moving, not in a vent; softer and slower crouched.
            bool grounded = Mathf.Abs(state.Player.y - Mathf.Round(state.Player.y / state.FloorHeight) * state.FloorHeight) < 0.05f;
            bool walking = !reset && state.Alive && !state.InVent && grounded && (state.Player - _previousPlayer).sqrMagnitude > 0.000001f;
            _playerStep = walking ? _playerStep - dt : 0.05f;
            if (walking && _playerStep <= 0f)
            {
                _playerStep = state.Crouching ? _sounds.CrouchStepInterval : _sounds.StepInterval;
                Step(state.Crouching ? _sounds.CrouchStepVolume : _sounds.StepVolume);
            }

            // Guards: heard the nearer they are, and quieter through a floor.
            if (_guardSteps.Length != state.Guards.Length)
            {
                _guardSteps = new float[state.Guards.Length];
                _guardHeard = new Vector2[state.Guards.Length];
                for (int i = 0; i < _guardHeard.Length; i++) _guardHeard[i] = state.Guards[i].Position;
            }
            for (int i = 0; i < state.Guards.Length; i++)
            {
                GuardRVO guard = state.Guards[i];
                bool moving = !reset && guard.Alive && (guard.Position - _guardHeard[i]).sqrMagnitude > 0.000001f;
                _guardHeard[i] = guard.Position;
                _guardSteps[i] = moving ? _guardSteps[i] - dt : UnityEngine.Random.Range(0f, 0.2f);
                if (!moving || _guardSteps[i] > 0f) continue;
                _guardSteps[i] = guard.Alerted ? _sounds.StepInterval * 0.75f : _sounds.StepInterval * 1.2f;
                float distance = Mathf.Abs(guard.Position.x - state.Player.x) + Mathf.Abs(guard.Position.y - state.Player.y) * 1.5f;
                float loud = Mathf.Clamp01(1f - distance / _sounds.GuardHearRange);
                if (loud > 0.02f) Step(_sounds.GuardStepVolume * loud * loud);
            }

            // A switch clicks whenever a room's light changes; the room the player stands in hums while it is lit.
            if (_lightsWere.Length != state.Lights.Length || reset)
            {
                _lightsWere = new bool[state.Lights.Length];
                for (int i = 0; i < _lightsWere.Length; i++) _lightsWere[i] = state.Lights[i].On;
            }
            bool lit = false;
            for (int i = 0; i < state.Lights.Length; i++)
            {
                LightRVO light = state.Lights[i];
                if (light.On != _lightsWere[i] && _sounds.SwitchClick != null)
                {
                    float near = Mathf.Clamp01(1f - Mathf.Abs(light.Switch - state.Player.x) / _sounds.GuardHearRange);
                    _sfx.PlayOneShot(_sounds.SwitchClick, _sounds.SwitchVolume * Mathf.Max(0.25f, near));
                }
                _lightsWere[i] = light.On;
                if (light.On && Mathf.Abs(state.Player.y - light.Floor) < 1.5f && state.Player.x >= light.Left && state.Player.x <= light.Right) lit = true;
            }
            Fade(_hum, _sounds.LampHum, lit && !state.InVent ? _sounds.HumVolume : 0f, dt);

            // The cat: a meow now and then, a purr when the player stands beside it.
            if (_catLevel != _level)
            {
                _catLevel = _level;
                _cat = _level.GetComponentInChildren<CatView>(true);
            }
            bool purring = false;
            if (_cat != null && _cat.isActiveAndEnabled)
            {
                Vector2 at = _level.transform.InverseTransformPoint(_cat.transform.position);
                purring = Mathf.Abs(at.x - state.Player.x) < _sounds.PurrRange && Mathf.Abs(at.y - state.Player.y) < 1f;
                _meow -= dt;
                if (_meow <= 0f && _sounds.CatMeows.Length > 0)
                {
                    _meow = UnityEngine.Random.Range(_sounds.MeowInterval.x, Mathf.Max(_sounds.MeowInterval.x, _sounds.MeowInterval.y));
                    float near = Mathf.Clamp01(1f - Mathf.Abs(at.x - state.Player.x) / (_sounds.GuardHearRange * 1.5f));
                    AudioClip meow = _sounds.CatMeows[UnityEngine.Random.Range(0, _sounds.CatMeows.Length)];
                    if (meow != null) _sfx.PlayOneShot(meow, _sounds.MeowVolume * Mathf.Max(0.3f, near));
                }
            }
            Fade(_purr, _sounds.CatPurr, purring ? _sounds.PurrVolume : 0f, dt);
            // Cash picked up: a clink for every bundle.
            if (!reset && state.CashCarried > _cashHeard && _sounds.CashPickup != null) _sfx.PlayOneShot(_sounds.CashPickup, _sounds.CashVolume);
            _cashHeard = state.CashCarried;
            if (_ring.clip != _sounds.PhoneRing) _ring.clip = _sounds.PhoneRing;
            _ring.volume = _sounds.RingVolume;
            if (state.CallRinging && !_ring.isPlaying && _ring.clip != null) _ring.Play();
            else if (!state.CallRinging && _ring.isPlaying) _ring.Stop();
        }

        private void Step(float volume)
        {
            if (_sounds.Footsteps.Length == 0 || volume <= 0f) return;
            AudioClip clip = _sounds.Footsteps[UnityEngine.Random.Range(0, _sounds.Footsteps.Length)];
            if (clip != null) _sfx.PlayOneShot(clip, volume);
        }

        // Starts a loop on its clip and eases it toward the volume; at silence it stops.
        private static void Fade(AudioSource source, AudioClip clip, float volume, float dt)
        {
            if (clip == null) volume = 0f;
            if (clip != null && source.clip != clip)
            {
                if (source.volume > 0.01f) volume = 0f;
                else { source.clip = clip; source.Play(); }
            }
            source.volume = Mathf.MoveTowards(source.volume, volume, dt * 0.8f);
            if (source.volume <= 0f && volume <= 0f && source.isPlaying) source.Stop();
            else if (volume > 0f && !source.isPlaying && source.clip != null) source.Play();
        }

        private void Restart(ContractRVO state)
        {
            ReturnAll();
            _aim = state.Aim;
            _shake = 0f;
            _lastImpact = state.ImpactCount;
            _guardShots = state.GuardShots;
            _ammo = state.Ammo;
            _playerAlive = true;
            _doorsOpen = new bool[state.Doors.Length];
            for (int i = 0; i < _doorsOpen.Length; i++) _doorsOpen[i] = state.Doors[i].Open;
            _previousGuards = new Vector2[state.Guards.Length];
            _guardsAlive = new bool[state.Guards.Length];
            for (int i = 0; i < _guardsAlive.Length; i++) _guardsAlive[i] = true;
        }

        private void DrawFixtures(ContractRVO state)
        {
            UpdateHideoutClock(state);
            for (int i = 0; i < state.Doors.Length && i < _level.Doors.Length; i++)
                _level.Doors[i].Apply(state.Doors[i].Open);
            for (int i = 0; i < state.Lights.Length && i < _level.Rooms.Length; i++)
                _level.Rooms[i].Apply(state.Lights[i].On, state.Lights[i].Floor);
            for (int i = 0; i < state.Props.Length && i < _level.Props.Length; i++)
                _level.Props[i].Apply(state.Props[i].Broken, state.Props[i].Lift);
            foreach (TapeDeckView deck in _level.Decks) deck.Apply(state.TapePlaying);
            if (_level.Document != null) _level.Document.Apply(!state.HasDocument || state.DocumentTaken);
            if (_level.Exit != null) _level.Exit.Apply(state.HasDocument && state.DocumentTaken && state.Alive && !state.Won);
            foreach (StationView station in _level.StationViews)
            {
                Vector3 at = station.transform.position;
                station.Apply(!state.InVent && Mathf.Abs(state.Player.x - at.x) < 1f && Mathf.Abs(state.Player.y - at.y) < 0.6f);
            }
        }

        private void CreateHideoutClock(LevelAuthoring level)
        {
            CreateHideoutDesk(level);
            _hideoutClock = new GameObject("Device Clock and Calendar");
            _hideoutClock.transform.SetParent(level.transform, false);
            _hideoutClock.transform.localPosition = new Vector3(3.2f, 0.89f, -0.2f);
            _clockText = ClockLabel("Clock", _hideoutClock.transform, new Vector3(0f, 0.045f, 0f), 0.03f, new Color(0.75f, 0.95f, 0.86f));
            _calendarText = ClockLabel("Calendar", _hideoutClock.transform, new Vector3(0f, -0.045f, 0f), 0.02f, new Color(0.9f, 0.78f, 0.5f));
        }

        private void CreateHideoutDesk(LevelAuthoring level)
        {
            _hideoutDesk = new GameObject("Clock Desk");
            _hideoutDesk.transform.SetParent(level.transform, false);
            _hideoutDesk.transform.localPosition = new Vector3(3.2f, 0f, 0.1f);
            DeskPart("Top", new Vector3(0f, 0.82f, 0f), new Vector2(1.5f, 0.14f), new Color(0.24f, 0.13f, 0.08f), -18);
            DeskPart("Front", new Vector3(0f, 0.58f, 0f), new Vector2(1.25f, 0.34f), new Color(0.17f, 0.09f, 0.06f), -19);
            DeskPart("Left leg", new Vector3(-0.55f, 0.28f, 0f), new Vector2(0.12f, 0.58f), new Color(0.12f, 0.07f, 0.05f), -20);
            DeskPart("Right leg", new Vector3(0.55f, 0.28f, 0f), new Vector2(0.12f, 0.58f), new Color(0.12f, 0.07f, 0.05f), -20);
        }

        private void DeskPart(string name, Vector3 position, Vector2 size, Color color, int order)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(_hideoutDesk.transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
            Texture2D texture = new Texture2D(1, 1);
            texture.name = "Runtime Desk Pixel";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            renderer.color = color;
            renderer.sortingOrder = order;
        }

        private TextMesh ClockLabel(string name, Transform parent, Vector3 position, float size, Color color)
        {
            GameObject label = new GameObject(name);
            label.transform.SetParent(parent, false);
            label.transform.localPosition = position;
            TextMesh text = label.AddComponent<TextMesh>();
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 48;
            text.characterSize = size;
            text.color = color;
            text.fontStyle = FontStyle.Bold;
            MeshRenderer renderer = label.GetComponent<MeshRenderer>();
            renderer.sortingOrder = 12;
            return text;
        }

        private void UpdateHideoutClock(ContractRVO state)
        {
            if (_hideoutClock == null) return;
            bool visible = state.InHideout;
            if (_hideoutClock.activeSelf != visible) _hideoutClock.SetActive(visible);
            if (!visible) return;
            DateTime now = DateTime.Now;
            string current = now.ToString("HH:mm:ss dd/MM/yyyy");
            if (current == _lastDeviceTime) return;
            _lastDeviceTime = current;
            _clockText.text = now.ToString("HH:mm:ss");
            _calendarText.text = now.ToString("dd/MM/yyyy");
        }

        private const int PlayerSlot = -1;

        // The rig for a slot, made from the prefab its character type names and remade if the type changed.
        private CharacterRig Rig(int slot, string type)
        {
            if (_rigs.TryGetValue(slot, out CharacterRig rig) && _rigTypes[slot] == type) return rig;
            if (rig != null) Destroy(rig.gameObject);
            _rigs.Remove(slot);
            CharacterTypeCVO config = _characters != null ? _characters.Find(type) : null;
            if (config == null || config.Prefab == null || _level == null) return null;
            GameObject made = Instantiate(config.Prefab, _level.Dynamic != null ? _level.Dynamic : _level.transform, false);
            made.name = config.Prefab.name + (slot == PlayerSlot ? " (player)" : " #" + slot);
            rig = made.GetComponent<CharacterRig>();
            if (rig == null)
            {
                FlowLogger.LogError($"{config.Prefab.name}, the prefab of character '{type}', has no CharacterRig.", config.Prefab);
                Destroy(made);
                return null;
            }
            rig.OnInitialized();
            rig.OnGetFromPool();
            _rigs[slot] = rig;
            _rigTypes[slot] = type;
            return rig;
        }

        private GameObject WeaponPrefab(string weapon)
        {
            WeaponCVO config = _weapons != null ? _weapons.Find(weapon) : null;
            return config != null ? config.Prefab : null;
        }

        private void ClearRigs()
        {
            foreach (CharacterRig rig in _rigs.Values) if (rig != null) Destroy(rig.gameObject);
            _rigs.Clear();
            _rigTypes.Clear();
        }

        private void DrawPlayer(ContractRVO state, bool reset)
        {
            CharacterRig player = Rig(PlayerSlot, state.PlayerType);
            _duck = state.Alive ? 1f : _deathDuck;
            if (player == null) return;
            WeaponCVO weapon = _weapons != null ? _weapons.Find(state.Weapon) : null;
            // At home the gun stays holstered.
            player.SetWeapon(weapon != null && !state.InHideout ? weapon.Prefab : null);
            WeaponCVO pocket = _weapons != null ? _weapons.Find(state.Holster) : null;
            player.SetHolster(pocket != null && !state.InHideout && state.Alive ? pocket.Prefab : null);
            if (_outfits != null) player.Wear(_outfits.Find(state.Outfit));
            if (_playerAlive && !state.Alive) _playerFall = FallDirection(state.Player, state.GuardShotFrom, state);
            _playerAlive = state.Alive;
            player.Apply(new CharacterPoseVO
            {
                Position = state.Player,
                Aim = state.Aim,
                Alive = state.Alive,
                FallDirection = _playerFall,
                Crouching = state.Crouching,
                Moving = !reset && (state.Player - _previousPlayer).sqrMagnitude > 0.000001f,
                Hidden = state.Hidden || state.InCover,
                InVent = state.InVent,
                Reloading = state.ReloadRemaining > 0f,
                ReloadProgress = weapon != null ? 1f - state.ReloadRemaining / (weapon.ReloadTime * state.ReloadScale) : 0f,
                MagazineReload = weapon != null && weapon.Reload == Enums.ReloadMode.Magazine,
                Phone = PhonePose(state),
                TrackSwitch = state.TrackSwitch > 0f ? 1f - state.TrackSwitch / ContractRVO.TrackSwitchLength : -1f,
                Headphones = state.InHideout ? -1f : state.IntroRemaining > 0f
                    ? Mathf.Clamp01((ContractRVO.IntroLength - state.IntroRemaining) / ContractRVO.IntroHeadset) : 1f,
                Ammo = state.Ammo,
                Magazine = weapon != null ? weapon.Magazine : 0,
            });
        }

        // The intro's phone comes out once the headphones are on; on a call it is held at the ear.
        private static float PhonePose(ContractRVO state)
        {
            if (state.Call >= 0 && !state.CallRinging) return 0.5f;
            if (state.IntroRemaining <= 0f) return -1f;
            float elapsed = ContractRVO.IntroLength - state.IntroRemaining;
            if (elapsed < ContractRVO.IntroHeadset) return -1f;
            return (elapsed - ContractRVO.IntroHeadset) / (ContractRVO.IntroLength - ContractRVO.IntroHeadset);
        }

        private void DrawGuards(ContractRVO state, bool reset)
        {
            for (int i = 0; i < state.Guards.Length; i++)
            {
                GuardRVO guard = state.Guards[i];
                CharacterRig rig = Rig(i, guard.Type);
                if (rig == null) continue;
                rig.gameObject.SetActive(!guard.Escaped);
                if (guard.Escaped) continue;
                // A body has let go of its weapon; it lies on the floor as a pickup instead.
                rig.SetWeapon(guard.Alive ? WeaponPrefab(guard.Weapon) : null);
                float fall = 1f;
                if (_guardsAlive[i] && !guard.Alive) fall = FallDirection(guard.Position, state.Player, state);
                _guardsAlive[i] = guard.Alive;
                rig.Apply(new CharacterPoseVO
                {
                    Position = guard.Position,
                    Aim = guard.Alerted && guard.Suspicion >= 1f && !guard.Crouching
                        ? state.Player + Vector2.up * 1.1f : guard.Position + new Vector2(guard.Facing * 4f, 1.25f),
                    Alive = guard.Alive,
                    Crouching = guard.Crouching && guard.Alive,
                    FallDirection = fall,
                    Moving = !reset && (guard.Position - _previousGuards[i]).sqrMagnitude > 0.000001f,
                    Alert = guard.Alerted ? 1f : Mathf.Clamp01(guard.Suspicion),
                    Spotted = guard.Alerted && guard.Suspicion >= 1f,
                    Investigating = guard.Investigating,
                    Stunned = guard.StunRemaining > 0f,
                });
                _previousGuards[i] = guard.Position;
            }
        }

        // Away from what killed them: the blood the killing bullet left, or else away from the shooter.
        private float FallDirection(Vector2 feet, Vector2 from, ContractRVO state)
        {
            float direction = feet.x >= from.x ? 1f : -1f;
            foreach (ImpactRVO impact in state.Impacts)
                if (impact.Blood && impact.Id > _lastImpact && Mathf.Abs(impact.Normal.x) > 0.3f
                    && Vector2.Distance(impact.Position, feet + Vector2.up) < 1.5f)
                    direction = impact.Normal.x > 0 ? -1f : 1f;
            return direction;
        }

        private void DrawBullets(ContractRVO state)
        {
            _live.Clear();
            foreach (BulletRVO bullet in state.Bullets)
            {
                _live.Add(bullet);
                if (!_bullets.TryGetValue(bullet, out BulletView view))
                {
                    view = Get<BulletView>(NightShiftPoolKeys.Bullet);
                    if (view == null) continue;
                    _bullets[bullet] = view;
                }
                view.Follow(bullet.Position, bullet.Velocity, bullet.FromPlayer);
            }
            _gone.Clear();
            foreach (KeyValuePair<BulletRVO, BulletView> pair in _bullets)
                if (!_live.Contains(pair.Key)) _gone.Add(pair.Key);
            foreach (BulletRVO bullet in _gone)
            {
                Return(_bullets[bullet]);
                _bullets.Remove(bullet);
            }
        }

        private void DrawPickups(ContractRVO state)
        {
            _livePickups.Clear();
            foreach (PickupRVO pickup in state.Pickups)
            {
                _livePickups.Add(pickup.Id);
                if (!_pickups.TryGetValue(pickup.Id, out WeaponPickupView view))
                {
                    view = Get<WeaponPickupView>(NightShiftPoolKeys.Pickup);
                    if (view == null) continue;
                    _pickups[pickup.Id] = view;
                }
                view.Show(pickup.Position, WeaponPrefab(pickup.Weapon));
            }
            _gonePickups.Clear();
            foreach (KeyValuePair<int, WeaponPickupView> pair in _pickups)
                if (!_livePickups.Contains(pair.Key)) _gonePickups.Add(pair.Key);
            foreach (int id in _gonePickups)
            {
                Return(_pickups[id]);
                _pickups.Remove(id);
            }
        }

        private void DrawLoot(ContractRVO state)
        {
            _liveLoot.Clear();
            foreach (LootRVO loot in state.Loot)
            {
                _liveLoot.Add(loot.Id);
                if (!_loot.TryGetValue(loot.Id, out CashPickupView view))
                {
                    view = Get<CashPickupView>(NightShiftPoolKeys.Cash);
                    if (view == null) continue;
                    _loot[loot.Id] = view;
                }
                view.Show(loot.Position);
            }
            _goneLoot.Clear();
            foreach (KeyValuePair<int, CashPickupView> pair in _loot)
                if (!_liveLoot.Contains(pair.Key)) _goneLoot.Add(pair.Key);
            foreach (int id in _goneLoot)
            {
                Return(_loot[id]);
                _loot.Remove(id);
            }
        }

        private void DrawDecals(ContractRVO state)
        {
            _liveDecals.Clear();
            foreach (ImpactRVO impact in state.Impacts)
            {
                _liveDecals.Add(impact.Id);
                if (_decals.ContainsKey(impact.Id)) continue;
                string mark = Mark(impact.Kind);
                if (mark == null) continue;
                DecalView decal = Get<DecalView>(mark);
                if (decal == null) continue;
                decal.Place(impact.Position, impact.Normal, impact.Id);
                _decals[impact.Id] = decal;
            }
            _goneDecals.Clear();
            foreach (KeyValuePair<int, DecalView> pair in _decals)
                if (!_liveDecals.Contains(pair.Key)) _goneDecals.Add(pair.Key);
            foreach (int id in _goneDecals)
            {
                Return(_decals[id]);
                _decals.Remove(id);
            }
        }

        private void Emit(ContractRVO state)
        {
            if (state.Shots > _shots)
            {
                // Heavier guns shake harder: a shotgun's spread of pellets, a rifle's weight.
                WeaponCVO held = _weapons != null ? _weapons.Find(state.Weapon) : null;
                float weight = held != null ? Mathf.Clamp(0.6f + 0.18f * held.Damage * held.Pellets, 0.6f, 1.8f) : 1f;
                _shake = Mathf.Max(_shake, _shotShake * weight);
                Vector2 back = (state.ShotFrom - state.ShotTo).normalized;
                _kick += back * _shotKick * weight;
                Play(NightShiftPoolKeys.Muzzle, state.ShotFrom, state.ShotTo - state.ShotFrom);
                if (_rigs.TryGetValue(PlayerSlot, out CharacterRig player) && player != null) player.Kick();
            }
            // Every hit flashes the guard it found; a kill freezes the frame for a breath.
            if (_guardHealth.Length != state.Guards.Length)
            {
                _guardHealth = new int[state.Guards.Length];
                for (int i = 0; i < _guardHealth.Length; i++) _guardHealth[i] = state.Guards[i].Health;
            }
            for (int i = 0; i < state.Guards.Length; i++)
            {
                GuardRVO guard = state.Guards[i];
                if (guard.Health < _guardHealth[i])
                {
                    if (_rigs.TryGetValue(i, out CharacterRig hit) && hit != null) hit.Flash();
                    if (!guard.Alive) HitStop();
                    _shake = Mathf.Max(_shake, _impactShake * 2f);
                }
                _guardHealth[i] = guard.Health;
            }
            if (state.GuardShots > _guardShots)
            {
                _shake = Mathf.Max(_shake, _guardShotShake);
                Play(NightShiftPoolKeys.GuardMuzzle, state.GuardShotFrom, state.GuardShotDirection);
            }
            if (state.Ammo > _ammo) Play(NightShiftPoolKeys.ShellLoad, state.Player + Vector2.up * 2.4f, Vector2.right);
            if (state.MeleeCooldown > _meleeCooldown + 0.05f)
            {
                // A swing: the arm comes round, the body lunges, and a landed blow shakes the screen.
                if (_rigs.TryGetValue(PlayerSlot, out CharacterRig puncher) && puncher != null) puncher.Punch();
                bool landed = state.MeleeLanded;
                if (landed)
                {
                    Play(NightShiftPoolKeys.Melee, state.ShotTo, state.ShotTo - state.ShotFrom);
                    _shake = Mathf.Max(_shake, _meleeShake);
                    _kick += (state.ShotTo - state.ShotFrom).normalized * 0.18f;
                }
            }
            for (int i = 0; i < state.Doors.Length && i < _doorsOpen.Length; i++)
            {
                if (state.Doors[i].Open && !_doorsOpen[i])
                    Play(NightShiftPoolKeys.DoorKick, state.Doors[i].Position + Vector2.up * 1.2f, new Vector2(state.Doors[i].Position.x - state.Player.x, 0f));
                _doorsOpen[i] = state.Doors[i].Open;
            }
            foreach (ImpactRVO impact in state.Impacts)
            {
                if (impact.Id <= _lastImpact) continue;
                _shake = Mathf.Max(_shake, impact.Kind == ImpactKind.Explosion ? _explosionShake : _impactShake);
                Play(Burst(impact.Kind), impact.Position, impact.Blood ? -impact.Normal : impact.Normal);
                // A shot into the ceiling also brings plaster down; a blast leaves smoke rising and dust falling.
                if (impact.Kind == ImpactKind.Ceiling) Play(NightShiftPoolKeys.CeilingDebris, impact.Position + Vector2.down * 0.1f, Vector2.down);
            }
        }

        // A few hundredths of a second of stillness. Real time, so it ends even with the game frozen.
        private void HitStop()
        {
            if (_hitStop <= 0f) return;
            _hitStopUntil = Time.unscaledTime + _hitStop;
            Time.timeScale = 0.05f;
        }

        private void LateUpdate()
        {
            if (_hitStopUntil > 0f && Time.unscaledTime >= _hitStopUntil)
            {
                _hitStopUntil = -1f;
                Time.timeScale = 1f;
            }
        }

        private string Burst(ImpactKind kind) => kind switch
        {
            ImpactKind.Blood => NightShiftPoolKeys.ImpactBlood,
            ImpactKind.Ceiling => NightShiftPoolKeys.ImpactWall,
            ImpactKind.Floor => NightShiftPoolKeys.FloorDust,
            ImpactKind.Wood => NightShiftPoolKeys.Splinters,
            ImpactKind.Glass => NightShiftPoolKeys.Glass,
            ImpactKind.Metal => NightShiftPoolKeys.MetalSparks,
            ImpactKind.Dirt => NightShiftPoolKeys.Dirt,
            ImpactKind.Explosion => NightShiftPoolKeys.Explosion,
            _ => NightShiftPoolKeys.ImpactWall,
        };

        // The mark an impact leaves; a broken prop shows its own broken look instead.
        private string Mark(ImpactKind kind) => kind switch
        {
            ImpactKind.Blood => NightShiftPoolKeys.BloodMark,
            ImpactKind.Explosion => NightShiftPoolKeys.Scorch,
            ImpactKind.Glass or ImpactKind.Metal or ImpactKind.Dirt => null,
            _ => NightShiftPoolKeys.BulletHole,
        };

        private void Play(string key, Vector2 position, Vector2 direction)
        {
            PooledEffect effect = Get<PooledEffect>(key);
            if (effect == null) return;
            effect.Play(position, direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right);
        }

        private T Get<T>(string key) where T : class, IPoolableItem
        {
            if (_pool == null || _level == null) return null;
            return _pool.Get<T>(key, _level.Dynamic != null ? _level.Dynamic : _level.transform);
        }

        private void Return(IPoolableItem item)
        {
            if (item != null && _pool != null) _pool.Return.Item(item);
        }

        private void ReturnAll()
        {
            // Effects still playing come home too, so nothing is left parked under a hidden level.
            if (_pool != null && _pool.Check.IsGroupReady(NightShiftPoolKeys.Group)) _pool.Return.Group(NightShiftPoolKeys.Group);
            ClearRigs();
            _bullets.Clear();
            _decals.Clear();
            _pickups.Clear();
            _loot.Clear();
        }

        private void OnDisable()
        {
            if (_hitStopUntil > 0f) { _hitStopUntil = -1f; Time.timeScale = 1f; }
            _active = false;
            _hasSnapshot = false;
            if (_level != null) _level.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            InputSampled = null;
            _pool = null;
        }
    }
}
