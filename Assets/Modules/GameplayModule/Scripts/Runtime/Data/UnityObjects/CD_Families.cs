using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    [CreateAssetMenu(fileName = "CD_Families", menuName = "Night Shift/CD_Families")]
    public class CD_Families : ScriptableObject
    {
        [Tooltip("The player's own family, used for owned locations and assigned defenders.")]
        public CD_Family PlayerFamily;
        [Tooltip("Every family available to mission and raid configs. Each entry is its own CD_Family asset.")]
        public List<CD_Family> Families = new();

        public CD_Family Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (CD_Family family in Families)
                if (family != null && family.Id == id) return family;
            return null;
        }

        public FamilyDiplomacy Relation(string first, string second)
        {
            if (!string.IsNullOrEmpty(first) && first == second) return FamilyDiplomacy.Friendly;
            FamilyDiplomacyCVO direct = Find(first)?.Relations?.Find(item => item != null && item.Family == second);
            FamilyDiplomacyCVO reverse = Find(second)?.Relations?.Find(item => item != null && item.Family == first);
            // Diplomacy is mutual. If two configs conflict, hostility wins so query order can never change the answer.
            if (direct?.Relation == FamilyDiplomacy.Enemy || reverse?.Relation == FamilyDiplomacy.Enemy) return FamilyDiplomacy.Enemy;
            if (direct?.Relation == FamilyDiplomacy.Friendly || reverse?.Relation == FamilyDiplomacy.Friendly) return FamilyDiplomacy.Friendly;
            if (PlayerFamily != null)
            {
                CD_Family other = first == PlayerFamily.Id ? Find(second) : second == PlayerFamily.Id ? Find(first) : null;
                if (other != null) return other.RelationToPlayer == FamilyRelation.Friendly
                    ? FamilyDiplomacy.Friendly : FamilyDiplomacy.Enemy;
            }
            return FamilyDiplomacy.Enemy;
        }
    }
}
