using System;
using Modules.GameplayModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One contract on the hideout's mission board.
    [Serializable]
    public class MissionCVO
    {
        public string Id = "new_mission";
        public string Title = "NEW CONTRACT";
        [TextArea(2, 6)] public string Briefing = "";
        [Tooltip("The level this contract is played in.")]
        public LevelAuthoring Level;
        [Tooltip("Cash paid the first time the contract is completed.")]
        [Min(0)] public int Reward = 300;
    }
}
