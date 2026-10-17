using System;
using System.Collections.Generic;
using Modules.GameplayModule.Enums;

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
        // One UTC payment boundary per completed location; this is what lets income continue while the game is closed.
        public List<WorkingLocationPaymentSVO> WorkingLocationPayments = new();
        public List<WorkingLocationSVO> WorkingLocations = new();
        // Config relations are the starting diplomacy. Attacking a friendly family adds its Id here permanently.
        public List<string> EnemyFamilies = new();
        // The contract in progress when the game was left; empty means home. Play picks it up again.
        public string CurrentMission;
        // The broker's first call has been answered and heard; it never rings again.
        public bool CallHeard;
        // When the game was last open, UTC: the time since is what CatchUp settles on the next launch.
        public long LastSeenUnixMilliseconds;
    }

    [Serializable]
    public class MissionGuardSVO
    {
        public string Mission;
        public int Count;
    }

    [Serializable]
    public class WorkingLocationPaymentSVO
    {
        public string Mission;
        public long LastPaymentUnixMilliseconds;
    }

    [Serializable]
    public class WorkingLocationSVO
    {
        public string Mission;
        public WorkingLocationStatus Status;
        public long NextAttackRollUnixMilliseconds;
        public long AttackExpiresUnixMilliseconds;
        public string OwnerFamily;
        // The enemy family currently raiding it; kept through offline time and the defence run.
        public string AttackingFamily;
        public bool AttackNoticeSent;
    }

    public class WorkingLocationUpdateVO
    {
        public string AttackedMission;
        public string LostMission;
        // A raid the posted men beat off on their own.
        public string HeldMission;
    }

    [Serializable]
    public class UpgradeLevelSVO
    {
        public string Id;
        public int Level;
    }
}
