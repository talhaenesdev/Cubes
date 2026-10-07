using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    public struct ContractInputVO
    {
        public float Move;
        public bool Jump;
        public bool Crouch;
        public bool Fire;
        public bool FireHeld;
        public bool Melee;
        public bool Interact;
        public bool Reload;
        public bool Swap;
        // Skip to the next song on the job: the player taps the headphones.
        public bool NextTrack;
        public bool ReloadHeld;
        public bool Pause;
        public Vector2 Aim;
        // The mouse wheel this frame: up is positive. Zero while the pointer is over the HUD.
        public float Zoom;
    }
}
