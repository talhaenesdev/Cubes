using System;
using System.Collections.Generic;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // What carries over between runs and sessions: cash, finished contracts, bought upgrades.
    [Serializable]
    public class ProgressSVO
    {
        public int Cash;
        public List<string> Completed = new();
        public List<UpgradeLevelSVO> Upgrades = new();
    }

    [Serializable]
    public class UpgradeLevelSVO
    {
        public string Id;
        public int Level;
    }
}
