using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // The hideout the player starts in and the contracts its mission board offers.
    [CreateAssetMenu(fileName = "CD_Missions", menuName = "Night Shift/CD_Missions")]
    public class CD_Missions : ScriptableObject
    {
        [Tooltip("The player's room: where Play starts, and where a finished contract returns to.")]
        public LevelAuthoring Hideout;
        public List<MissionCVO> Missions = new();

        public MissionCVO Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (MissionCVO mission in Missions)
                if (mission.Id == id) return mission;
            return null;
        }
    }
}
