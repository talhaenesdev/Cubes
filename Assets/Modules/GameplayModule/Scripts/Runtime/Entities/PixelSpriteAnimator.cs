using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // Plays a drawn character frame by frame. CharacterRig tells it what the character is doing; the frames and speeds
    // are set on the prefab, so new drawings need no code. A set left empty falls back to the nearest one there is:
    // an armed set to its plain one, a backward walk to the walk played in reverse, anything else to Still.
    public class PixelSpriteAnimator : MonoBehaviour
    {
        public SpriteRenderer Renderer;
        [Tooltip("The separate gun arm, a child of the rig's ArmPivot: shown while a weapon is held, so it turns with the aim.")]
        public SpriteRenderer Arm;
        public Sprite Still;

        [Header("Unarmed")]
        public Sprite[] Idle = new Sprite[0];
        public Sprite[] Walk = new Sprite[0];
        public Sprite[] Run = new Sprite[0];
        [Tooltip("Frame 0 crouched still, frame 1 crouch-walking.")]
        public Sprite[] Crouch = new Sprite[0];
        [Tooltip("Walking while crouched, a loop. Empty: crouch frame 1 is held.")]
        public Sprite[] CrouchWalk = new Sprite[0];

        [Header("Holding a weapon (drawn without the gun arm)")]
        public Sprite[] IdleArmed = new Sprite[0];
        public Sprite[] WalkArmed = new Sprite[0];
        public Sprite[] RunArmed = new Sprite[0];
        public Sprite[] CrouchArmed = new Sprite[0];
        public Sprite[] CrouchWalkArmed = new Sprite[0];

        [Header("Moves")]
        [Tooltip("Facing one way, stepping the other. Empty: the walk is played backwards.")]
        public Sprite[] WalkBackward = new Sprite[0];
        [Tooltip("Frame 0 taking off, 1 in the air, 2 landing.")]
        public Sprite[] Jump = new Sprite[0];
        public Sprite[] Melee = new Sprite[0];
        public Sprite[] Kick = new Sprite[0];
        public Sprite[] Hurt = new Sprite[0];
        [Tooltip("Played once, holding the last frame.")]
        public Sprite[] Death = new Sprite[0];

        [Header("Speeds")]
        [Min(1f)] public float IdleFps = 4f;
        [Min(1f)] public float WalkFps = 10f;
        [Min(1f)] public float RunFps = 12f;
        [Min(1f)] public float MoveFps = 14f;
        [Min(1f)] public float DeathFps = 8f;
        [Tooltip("Units a second above which a moving character runs rather than walks.")]
        [Min(0f)] public float RunSpeed = 3.2f;

        // How far the top of the frame on show sits above the still drawing's (below it: negative), in units. Each
        // frame is cut to its own height with the feet on the bottom edge, so this is the breath, the bob, the crouch.
        public float Rise => Renderer != null && Renderer.sprite != null && Still != null
            ? Renderer.sprite.bounds.size.y - Still.bounds.size.y : 0f;

        private Sprite[] _playing;
        private float _time;
        private Vector2 _last;
        private bool _hasLast;
        private Sprite[] _move;
        private float _moveTime;
        private float _landed;
        private float _airTime;
        private bool _wasAirborne;

        // A one-off move over whatever else is going on; it plays through once.
        public void PlayMelee() => Start(Melee);
        public void PlayKick() => Start(Kick);
        public void PlayHurt() => Start(Hurt);

        private void Start(Sprite[] set)
        {
            if (set == null || set.Length == 0) return;
            _move = set;
            _moveTime = 0f;
        }

        public void Apply(Vector2 position, bool alive, bool moving, bool crouching, bool armed, bool airborne,
            float verticalSpeed, float facing, float deadFor, float dt)
        {
            if (Renderer == null) Renderer = GetComponent<SpriteRenderer>();
            if (Renderer == null) return;
            float step = _hasLast ? position.x - _last.x : 0f;
            float speed = dt > 0f ? Mathf.Abs(step) / dt : 0f;
            _last = position;
            _hasLast = true;
            Renderer.flipX = facing < 0f;

            if (_wasAirborne && !airborne) _landed = 0.12f;
            _wasAirborne = airborne;
            _airTime = airborne ? _airTime + dt : 0f;
            _landed = Mathf.Max(0f, _landed - dt);

            bool inMove = false;
            if (!alive)
            {
                _move = null;
                Show(Pick(Death, Mathf.FloorToInt(deadFor * DeathFps), false));
                _playing = Death;
            }
            else if (_move != null)
            {
                _moveTime += dt;
                int frame = Mathf.FloorToInt(_moveTime * MoveFps);
                if (frame >= _move.Length) _move = null;
                else
                {
                    inMove = true;
                    Show(_move[frame]);
                }
            }
            if (alive && !inMove) ShowBody(moving, crouching, armed, airborne, verticalSpeed, step, speed, facing, dt);

            // The gun arm only while a weapon is up; a punch, a kick or a hit is drawn with both arms in the frames.
            if (Arm != null)
            {
                bool showArm = alive && armed && !inMove && HasArmedBody(crouching);
                if (Arm.enabled != showArm) Arm.enabled = showArm;
            }
        }

        private bool HasArmedBody(bool crouching) => crouching ? CrouchArmed.Length > 0 || CrouchWalkArmed.Length > 0 : IdleArmed.Length > 0 || WalkArmed.Length > 0;

        private void ShowBody(bool moving, bool crouching, bool armed, bool airborne, float verticalSpeed, float step,
            float speed, float facing, float dt)
        {
            if ((airborne || _landed > 0f) && Jump.Length > 0)
            {
                int frame = !airborne ? Jump.Length - 1 : verticalSpeed > 0f && _airTime < 0.08f ? 0 : Mathf.Min(1, Jump.Length - 1);
                Show(Jump[frame]);
                _playing = Jump;
                return;
            }
            if (crouching && moving && (CrouchWalk.Length > 0 || CrouchWalkArmed.Length > 0))
            {
                Sprite[] sneak = armed && CrouchWalkArmed.Length > 0 ? CrouchWalkArmed : CrouchWalk.Length > 0 ? CrouchWalk : CrouchWalkArmed;
                if (sneak != _playing)
                {
                    _playing = sneak;
                    _time = 0f;
                }
                _time += dt;
                int sneakFrame = Mathf.FloorToInt(_time * WalkFps);
                // Stepping against the way the character faces plays the sneak backwards.
                bool back = Mathf.Abs(step) > 0.0001f && Mathf.Sign(step) != Mathf.Sign(facing);
                if (back) sneakFrame = sneak.Length - 1 - sneakFrame % sneak.Length;
                Show(Pick(sneak, sneakFrame, true));
                return;
            }
            if (crouching)
            {
                Sprite[] crouch = armed && CrouchArmed.Length > 0 ? CrouchArmed : Crouch;
                Show(crouch.Length > 0 ? crouch[moving && crouch.Length > 1 ? 1 : 0] : Still);
                _playing = crouch;
                return;
            }
            // Stepping against the way the character faces is a careful walk backwards, never a run.
            bool backward = moving && Mathf.Abs(step) > 0.0001f && Mathf.Sign(step) != Mathf.Sign(facing);
            Sprite[] set;
            bool reverse = false;
            float fps;
            if (!moving)
            {
                set = armed && IdleArmed.Length > 0 ? IdleArmed : Idle;
                fps = IdleFps;
            }
            else if (backward)
            {
                set = WalkBackward.Length > 0 ? WalkBackward : armed && WalkArmed.Length > 0 ? WalkArmed : Walk;
                reverse = WalkBackward.Length == 0;
                fps = WalkFps;
            }
            else if (speed > RunSpeed && (Run.Length > 0 || RunArmed.Length > 0))
            {
                set = armed && RunArmed.Length > 0 ? RunArmed : Run.Length > 0 ? Run : RunArmed;
                fps = RunFps;
            }
            else
            {
                set = armed && WalkArmed.Length > 0 ? WalkArmed : Walk;
                fps = WalkFps;
            }
            if (set != _playing)
            {
                _playing = set;
                _time = 0f;
            }
            _time += dt;
            int index = Mathf.FloorToInt(_time * fps);
            if (reverse && set.Length > 0) index = set.Length - 1 - index % set.Length;
            Show(Pick(set, index, true));
        }

        private Sprite Pick(Sprite[] set, int frame, bool loop)
        {
            if (set == null || set.Length == 0) return Still;
            return set[loop ? frame % set.Length : Mathf.Min(frame, set.Length - 1)];
        }

        private void Show(Sprite sprite)
        {
            if (sprite != null && Renderer.sprite != sprite) Renderer.sprite = sprite;
        }
    }
}
