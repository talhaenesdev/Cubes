using System;
using System.Collections.Generic;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // What carries over between runs and sessions: cash, finished contracts, bought upgrades, owned outfits,
    // the weapons carried and stored, and the contract the player was on when they last left the game.
    [Serializable]
    public class ProgressSVO
    {
        public int Cash;
        public List<string> Completed = new();
        public List<UpgradeLevelSVO> Upgrades = new();
        // What the player wears, by Id in CD_Outfits.
        public string Outfit;
        // The weapons carried out of the hideout - hand and pocket - and the ones left in the locker.
        public string Hand;
        public string Pocket;
        public List<string> Locker = new();
        // Outfit Ids bought at the wardrobe; free ones are owned without being listed.
        public List<string> OwnedOutfits = new();
        public List<MissionGuardSVO> MissionGuards = new();
        // The contract in progress when the game was left; empty means home. Play picks it up again.
        public string CurrentMission;
        // The broker's first call has been answered and heard; it never rings again.
        public bool CallHeard;
    }

    [Serializable]
    public class MissionGuardSVO
    {
        public string Mission;
        public int Count;
    }

    [Serializable]
    public class UpgradeLevelSVO
    {
        public string Id;
        public int Level;
    }
}
