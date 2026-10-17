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

        [Header("Phone call")]
        [Tooltip("Who rings the first time the player is home. Translated through CD_Localization.")]
        public string PhoneCaller = "THE BROKER";
        [Tooltip("What they say, one line per page. The player moves on with E, Space or a click.")]
        [TextArea(2, 4)] public List<string> PhoneCall = new()
        {
            "It's me. Don't talk, just listen.",
            "Every place on that board makes money for a family.",
            "Clear their armed men, but leave the civilians alone.",
            "Find the boss and make an offer they cannot refuse.",
            "Once the boss agrees to work for us, the place is ours.",
        };
        [Tooltip("Seconds a line stays before the next one comes on its own.")]
        [Min(1f)] public float PhoneLineTime = 6f;

        public MissionCVO Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (MissionCVO mission in Missions)
                if (mission.Id == id) return mission;
            return null;
        }
    }
}
