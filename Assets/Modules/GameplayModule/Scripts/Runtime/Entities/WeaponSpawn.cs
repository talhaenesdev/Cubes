using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A weapon lying in the level when the run starts, loaded.
    public class WeaponSpawn : MonoBehaviour
    {
        [Tooltip("Weapon Id in CD_Weapons.")]
        public string Weapon = "pistol";

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.3f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, new Vector3(0.7f, 0.25f, 0f));
        }
    }
}
