using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A stairwell door. Link it to its partner on the other floor; E at one comes out of the other,
    // and a guard hunting the player across floors walks to it.
    public class StairDoorView : MonoBehaviour
    {
        [Tooltip("The stairwell door on the other floor this one leads to. Link both ends to each other.")]
        public StairDoorView Link;

        private void OnDrawGizmos()
        {
            if (Link == null) return;
            Gizmos.color = new Color(1f, 0.65f, 0.3f, 0.6f);
            Gizmos.DrawLine(transform.position + Vector3.up * 1.2f, Link.transform.position + Vector3.up * 1.2f);
        }
    }
}
