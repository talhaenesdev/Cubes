using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // What a character rig shows this frame; the view fills it from the run and the rig only draws it.
    public struct CharacterPoseVO
    {
        public Vector2 Position;
        public Vector2 Aim;
        public bool Alive;
        public float FallDirection;
        public bool Crouching;
        public bool Moving;
        public bool Hidden;
        public bool InVent;
        public float Alert;
        public bool Spotted;
        public bool Investigating;
        public bool Stunned;
        public bool Reloading;
        public float ReloadProgress;
        public int Ammo;
        public int Magazine;
        // A whole-magazine reload shows the magazine swap; a round-by-round one the cylinder.
        public bool MagazineReload;
        // Through the contract intro, 0 to 1: phone out, tap, phone away. Below 0 when there is no intro.
        public float Phone;
        // Below 0 no headphones; 0 to 1 while they are put on; 1 worn.
        public float Headphones;
        // Changing the song: 0 to 1 while the hand goes to the headphones and back; below 0 when not.
        public float TrackSwitch;
    }
}
