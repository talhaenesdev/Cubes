using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    public class VentView : MonoBehaviour
    {
        [Tooltip("The vent this one leads to. Link both ends to each other.")]
        public VentView Link;

        private void OnDrawGizmos()
        {
            if (Link == null) return;
            Gizmos.color = new Color(0.3f, 0.9f, 0.83f, 0.6f);
            Gizmos.DrawLine(transform.position + Vector3.up * 0.3f, Link.transform.position + Vector3.up * 0.3f);
        }
    }
}
