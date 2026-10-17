using System;
using Modules.GameplayModule.Enums;

namespace Modules.GameplayModule.Data.ValueObjects
{
    [Serializable]
    public class FamilyDiplomacyCVO
    {
        public string Family;
        public FamilyDiplomacy Relation = FamilyDiplomacy.Enemy;
    }
}
