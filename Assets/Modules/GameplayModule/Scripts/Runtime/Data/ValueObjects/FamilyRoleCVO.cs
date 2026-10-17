using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    [Serializable]
    public class FamilyRoleCVO
    {
        [Tooltip("The CD_Characters Id that supplies combat stats and equipment defaults.")]
        public string CharacterType = "triangle";
        [Tooltip("The directly-instantiated character prefab. Characters are not served by the pool system.")]
        public GameObject Prefab;
        [Tooltip("Names drawn for members of this rank. Empty uses the rank name.")]
        public List<string> Names = new();
    }
}
