using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    public class PlayerSpawn : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 0.83f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.9f, new Vector3(0.6f, 1.8f, 0f));
        }
    }
}
