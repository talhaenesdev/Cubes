using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // Footsteps, the rooms' light switches and lamp hum, the cat, and the ambience under it all.
    // Swap any clip for your own; an empty slot is simply silent.
    [CreateAssetMenu(fileName = "CD_Sounds", menuName = "Night Shift/CD_Sounds")]
    public class CD_Sounds : ScriptableObject
    {
        [Header("Footsteps")]
        [Tooltip("One is picked at random for every step.")]
        public AudioClip[] Footsteps = new AudioClip[0];
        [Tooltip("Seconds between the player's steps when walking, and when crouched.")]
        [Min(0.1f)] public float StepInterval = 0.36f;
        [Min(0.1f)] public float CrouchStepInterval = 0.6f;
        [Range(0f, 1f)] public float StepVolume = 0.45f;
        [Range(0f, 1f)] public float CrouchStepVolume = 0.15f;
        [Tooltip("A guard's steps at arm's length; they fade out over GuardHearRange.")]
        [Range(0f, 1f)] public float GuardStepVolume = 0.35f;
        [Min(1f)] public float GuardHearRange = 9f;

        [Header("Lights")]
        public AudioClip SwitchClick;
        [Range(0f, 1f)] public float SwitchVolume = 0.7f;
        [Tooltip("Loops while the player stands in a lit room.")]
        public AudioClip LampHum;
        [Range(0f, 1f)] public float HumVolume = 0.08f;

        [Header("Cat")]
        public AudioClip[] CatMeows = new AudioClip[0];
        [Range(0f, 1f)] public float MeowVolume = 0.55f;
        [Tooltip("Seconds between meows, picked between these two.")]
        public Vector2 MeowInterval = new(9f, 22f);
        [Tooltip("Loops while the player stands next to the cat.")]
        public AudioClip CatPurr;
        [Range(0f, 1f)] public float PurrVolume = 0.35f;
        [Min(0.2f)] public float PurrRange = 1.4f;

        [Header("Cash")]
        public AudioClip CashPickup;
        [Range(0f, 1f)] public float CashVolume = 0.6f;

        [Header("Phone")]
        [Tooltip("Loops while the broker's call waits to be answered.")]
        public AudioClip PhoneRing;
        [Range(0f, 1f)] public float RingVolume = 0.5f;

        [Header("Ambience")]
        public AudioClip ContractAmbience;
        public AudioClip HideoutAmbience;
        [Range(0f, 1f)] public float AmbienceVolume = 0.3f;
    }
}
