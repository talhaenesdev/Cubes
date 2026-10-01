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
        public bool ReloadHeld;
        public bool Pause;
        public Vector2 Aim;
    }
}
