using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    public class GuardSpawn : MonoBehaviour
    {
        [Tooltip("Character type Id in CD_Characters - decides shape, friend or enemy, health and senses.")]
        public string CharacterType = "triangle";
        [Tooltip("Weapon Id in CD_Weapons. Empty uses the character type's own weapon.")]
        public string Weapon = "";
        [Tooltip("Extra hits this one takes, on top of its type and the level's difficulty.")]
        public int Toughness;
        [Tooltip("1 is its type's pace. 2 reacts and fires twice as fast; 0.5 half as fast.")]
        [Min(0.1f)] public float Aggression = 1f;
        [Tooltip("1 faces right, -1 faces left.")]
        public int Facing = 1;
        [Tooltip("World X the patrol turns back at.")]
        public float PatrolLeft = -1f;
        public float PatrolRight = 1f;
        [Tooltip("A non-combatant: works until frightened, then hides or flees.")]
        public bool Civilian;
        [Tooltip("The civilian the player can talk to after every enemy is gone.")]
        public bool Boss;

        private void OnDrawGizmos()
        {
            Vector3 p = transform.position;
            Gizmos.color = new Color(0.95f, 0.22f, 0.3f);
            Gizmos.DrawWireCube(p + Vector3.up * 0.9f, new Vector3(0.6f, 1.8f, 0f));
            Gizmos.DrawLine(new Vector3(PatrolLeft, p.y + 0.1f), new Vector3(PatrolRight, p.y + 0.1f));
            Gizmos.DrawLine(p + Vector3.up * 1.2f, p + new Vector3(Facing * 0.8f, 1.2f));
        }
    }
}
