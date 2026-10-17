using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    [CreateAssetMenu(fileName = "CD_Family", menuName = "Night Shift/CD_Family")]
    public class CD_Family : ScriptableObject
    {
        public string Id = "new_family";
        public string DisplayName = "NEW FAMILY";
        [Tooltip("The family's colour on the city map.")]
        public Color Color = new(0.55f, 0.2f, 0.22f);
        [Tooltip("Player is the Square family, Friendly families never raid it, Enemy families can attack owned locations.")]
        public FamilyRelation RelationToPlayer = FamilyRelation.Enemy;
        [Tooltip("This family's Friendly/Enemy relationship with every other family Id. Keep both sides filled for a readable config; CD_Families also accepts one-sided entries.")]
        public List<FamilyDiplomacyCVO> Relations = new();
        public FamilyRoleCVO Don = new() {CharacterType = "circle"};
        public FamilyRoleCVO Consigliere = new() {CharacterType = "diamond"};
        public FamilyRoleCVO Caporegime = new() {CharacterType = "pentagon"};
        public FamilyRoleCVO Soldiers = new() {CharacterType = "triangle"};
        public FamilyRoleCVO Associates = new() {CharacterType = "circle"};

        public FamilyRoleCVO Role(FamilyRank rank) => rank switch
        {
            FamilyRank.Don => Don,
            FamilyRank.Consigliere => Consigliere,
            FamilyRank.Caporegime => Caporegime,
            FamilyRank.Soldier => Soldiers,
            _ => Associates,
        };
    }
}
