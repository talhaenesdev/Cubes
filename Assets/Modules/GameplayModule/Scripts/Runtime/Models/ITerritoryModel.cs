using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Models
{
    // The city: which shops work for which family, what the player's pay, who guards them, and the raids on them -
    // live, and while the game is closed. Its state lives in the save the NightShiftModel owns.
    public interface ITerritoryModel
    {
        CD_Territory Config { get; }

        // ---- shops
        WorkingLocationSVO Location(string mission);
        bool IsOurs(string mission);
        bool IsUnderAttack(string mission);
        int OwnedCount { get; }
        int LocationCount { get; }
        bool AllOurs { get; }
        // The family a shop works for right now: the player's, the one that took it, or its owner in CD_Missions.
        string OwnerOf(MissionCVO mission);
        // Cash a shop makes per minute; 0 when it pays nothing.
        float IncomePerMinute(MissionCVO mission);
        // 0 the poorest shop on the map, 1 the richest: raid chance and strength both follow it.
        float Wealth(MissionCVO mission);
        int IncomePerHour { get; }
        // The owner has agreed to work for the player: the shop is theirs from now.
        void Claim(MissionCVO mission);

        // ---- guards
        int MaxGuards { get; }
        int Guards(string mission);
        int NetPayment(MissionCVO mission);
        bool TryHireGuard(string mission);
        void SetGuards(string mission, int count);

        // ---- families
        string FamilyName(string id);
        Color FamilyColor(string id);
        FamilyRelation RelationToPlayer(string family);
        FamilyDiplomacy RelationBetween(string firstFamily, string secondFamily);
        // True only when a friendly family has just become an enemy.
        bool DeclareEnemy(string family);

        // ---- time
        // Settles everything since the game was last open: income, raids that ran out, a shop lost per day away.
        TerritoryReportVO CatchUp(long utcMilliseconds);
        // The last CatchUp's report, until it has been told.
        TerritoryReportVO PendingReport { get; set; }
        // Marks the game as open now; saved now and then so a crash still counts the time.
        void Heartbeat(long utcMilliseconds);
        int CollectPayments(long utcMilliseconds);
        WorkingLocationUpdateVO ProcessRaids(long utcMilliseconds);

        // ---- defence
        string DefenseMission { get; }
        bool StartDefense(string mission);
        RaidVO RaidOn(string mission);
        // The raid beaten off with the player: the shop stays, the men who fell are gone.
        void DefenseWon(int defendersLost);
        void DefenseLost();

#if UNITY_EDITOR
        string DebugRaid();
        string DebugExpireRaid();
        string DebugHoldRaid();
        int DebugPayOnce();
        TerritoryReportVO DebugSkipDays(int days);
#endif
    }
}
