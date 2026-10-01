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
        [Header("Camera feel")]
        [SerializeField] private float _shotShake = 0.55f;
        [SerializeField] private float _guardShotShake = 0.35f;
        [SerializeField] private float _impactShake = 0.25f;
        [SerializeField] private float _explosionShake = 1.4f;
        [SerializeField] private float _shakeStrength = 0.6f;
        [SerializeField] private float _shakeRecovery = 2.2f;

        private readonly Dictionary<int, CharacterRig> _rigs = new();
        private readonly Dictionary<int, string> _rigTypes = new();
        private readonly Dictionary<int, WeaponPickupView> _pickups = new();
        private readonly List<int> _gonePickups = new();
        private readonly HashSet<int> _livePickups = new();
        private CD_Characters _characters;
        private CD_Weapons _weapons;
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
        private float _followX;
        private bool _active;
        private bool _hasSnapshot;
        private Vector2 _aim;
        private Vector2 _previousPlayer;

        public void SetPool(IPoolService pool) => _pool = pool;

        public void SetRoster(CD_Characters characters, CD_Weapons weapons)
        {
            _characters = characters;
            _weapons = weapons;
        }

        private readonly Dictionary<LevelAuthoring, LevelAuthoring> _levels = new();
        private readonly Dictionary<LevelAuthoring, Vector3> _cameraHomes = new();
        private readonly Dictionary<LevelAuthoring, float> _cameraSizes = new();
        private float _cameraSize = 9f;

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
            if (!_active || _level == null || _level.Camera == null) return;
            // The run stops ticking once it is decided; keep drawing so deaths, bullets and effects finish.
            if (_last != null && (!_last.Alive || _last.Won)) Draw(_last);
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            Camera view = _level.Camera;
            if (mouse != null)
            {
                Vector2 screen = mouse.position.ReadValue();
                Vector3 world = view.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -view.transform.position.z));
                _aim = _level.transform.InverseTransformPoint(world);
            }

            InputSampled?.Invoke(new ContractInputVO
            {
                Move = (Held(keyboard, Key.D) || Held(keyboard, Key.RightArrow) ? 1f : 0f)
                     - (Held(keyboard, Key.A) || Held(keyboard, Key.LeftArrow) ? 1f : 0f),
                Crouch = Held(keyboard, Key.LeftCtrl) || Held(keyboard, Key.RightCtrl)
                      || Held(keyboard, Key.LeftShift) || Held(keyboard, Key.RightShift) || Held(keyboard, Key.S),
                Jump = Pressed(keyboard, Key.Space),
                Fire = mouse != null && mouse.leftButton.wasPressedThisFrame && mouse.position.ReadValue().y > Screen.height * 0.105f && mouse.position.ReadValue().y < Screen.height * 0.84f,
                FireHeld = mouse != null && mouse.leftButton.isPressed && mouse.position.ReadValue().y > Screen.height * 0.105f && mouse.position.ReadValue().y < Screen.height * 0.84f,
                Melee = (mouse != null && mouse.rightButton.wasPressedThisFrame) || Pressed(keyboard, Key.F),
                Interact = Pressed(keyboard, Key.E),
                Reload = Pressed(keyboard, Key.R),
                Swap = Pressed(keyboard, Key.Q),
                ReloadHeld = Held(keyboard, Key.R),
                Pause = Pressed(keyboard, Key.Escape),
                Aim = _aim,
            });
        }

        private bool Held(Keyboard keyboard, Key key) => keyboard != null && keyboard[key].isPressed;
        private bool Pressed(Keyboard keyboard, Key key) => keyboard != null && keyboard[key].wasPressedThisFrame;

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
                view.orthographicSize = _level.FollowCamera ? _cameraSize : Mathf.Max(_cameraSize, (_level.HalfWidth + 2f) / aspect);
                _shake = Mathf.Max(0f, _shake - Time.deltaTime * _shakeRecovery);
                Vector2 jitter = UnityEngine.Random.insideUnitCircle * _shake * _shake * _shakeStrength;
                Vector3 home = _cameraHome;
                if (_level.FollowCamera)
                {
                    float halfView = view.orthographicSize * aspect;
                    float edge = Mathf.Max(0f, _level.HalfWidth + 1f - halfView);
                    float target = Mathf.Clamp(state.Player.x + (state.Aim.x - state.Player.x) * 0.2f, -edge, edge);
                    _followX = reset ? target : Mathf.Lerp(_followX, target, 1f - Mathf.Exp(-6f * Time.deltaTime));
                    home.x = _followX;
                }
                view.transform.localPosition = home + new Vector3(jitter.x, jitter.y, 0f);
            }
            if (_level.Crosshair != null) _level.Crosshair.localPosition = state.Aim;

            if (!reset) Emit(state);
            DrawFixtures(state);
            DrawPlayer(state, reset);
            DrawGuards(state, reset);
            DrawBullets(state);
            DrawDecals(state);
            DrawPickups(state);

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
            for (int i = 0; i < state.Doors.Length && i < _level.Doors.Length; i++)
                _level.Doors[i].Apply(state.Doors[i].Open);
            for (int i = 0; i < state.Lights.Length && i < _level.Rooms.Length; i++)
                _level.Rooms[i].Apply(state.Lights[i].On, state.Lights[i].Floor);
            for (int i = 0; i < state.Props.Length && i < _level.Props.Length; i++)
                _level.Props[i].Apply(state.Props[i].Broken, state.Props[i].Lift);
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
            if (player == null) return;
            WeaponCVO weapon = _weapons != null ? _weapons.Find(state.Weapon) : null;
            player.SetWeapon(weapon != null ? weapon.Prefab : null);
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
                Ammo = state.Ammo,
                Magazine = weapon != null ? weapon.Magazine : 0,
            });
        }

        private void DrawGuards(ContractRVO state, bool reset)
        {
            for (int i = 0; i < state.Guards.Length; i++)
            {
                GuardRVO guard = state.Guards[i];
                CharacterRig rig = Rig(i, guard.Type);
                if (rig == null) continue;
                // A body has let go of its weapon; it lies on the floor as a pickup instead.
                rig.SetWeapon(guard.Alive ? WeaponPrefab(guard.Weapon) : null);
                float fall = 1f;
                if (_guardsAlive[i] && !guard.Alive) fall = FallDirection(guard.Position, state.Player, state);
                _guardsAlive[i] = guard.Alive;
                rig.Apply(new CharacterPoseVO
                {
                    Position = guard.Position,
                    Aim = guard.Position + new Vector2(guard.Facing * 4f, 1.25f),
                    Alive = guard.Alive,
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
                _shake = Mathf.Max(_shake, _shotShake);
                Play(NightShiftPoolKeys.Muzzle, state.ShotFrom, state.ShotTo - state.ShotFrom);
                if (_rigs.TryGetValue(PlayerSlot, out CharacterRig player) && player != null) player.Kick();
            }
            if (state.GuardShots > _guardShots)
            {
                _shake = Mathf.Max(_shake, _guardShotShake);
                Play(NightShiftPoolKeys.GuardMuzzle, state.GuardShotFrom, state.GuardShotDirection);
            }
            if (state.Ammo > _ammo) Play(NightShiftPoolKeys.ShellLoad, state.Player + Vector2.up * 2.4f, Vector2.right);
            if (state.MeleeCooldown > _meleeCooldown + 0.05f)
                Play(NightShiftPoolKeys.Melee, state.ShotTo, state.ShotTo - state.ShotFrom);
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
        }

        private void OnDisable()
        {
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
