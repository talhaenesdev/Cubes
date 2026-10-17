using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;
namespace Modules.GameplayModule.Models
{
    public interface INightShiftModel
    {
        ContractRVO Run { get; }
        ContractInputVO Input { get; }
        ContractLevelVO Level { get; }
        bool IsTicking { get; set; }
        bool IsHooked { get; set; }
        // Enemies still standing; friendlies never count.
        int Remaining { get; }
        CD_Weapons Weapons { get; }
        CD_Characters Characters { get; }
        CD_Families Families { get; }
        CD_Missions Missions { get; }
        CD_Upgrades Upgrades { get; }
        CD_Music Music { get; }
        CD_Outfits Outfits { get; }
        // Every gameplay number that is not one weapon's or one character's.
        CD_Gameplay Tuning { get; }
        // Wears an owned outfit, or buys it first when the cash is there. False when it could not.
        bool Wear(string outfit);
        bool Owns(string outfit);
        void SaveProgress();
        // The one way anyone is hurt - bullets, blasts, falling lamps, takedowns. True when the hit killed.
        bool DamagePlayer(int amount, string cause);
        bool DamageGuard(GuardRVO guard, int amount);
        // The contract in progress, written to its own file; read back by RestoreRun when it is the same contract.
        void SaveRun();
        bool RestoreRun();
        void ClearRunSave();
        // Set by Play on a fresh launch: the next run is restored from the save rather than started anew.
        bool RestoreNextRun { get; set; }
        float RunSaveTimer { get; set; }
        // True while the main menu is open over a paused run.
        bool MenuOpen { get; set; }
        bool PausedBeforeMenu { get; set; }
        // Puts the hand (0) or pocket (1) weapon in the locker.
        void StoreWeapon(int slot);
        // Takes a weapon from the locker into the hand; what the hand held goes to the pocket if it is empty, else the locker.
        void TakeWeapon(string weapon);
        // The track playing now, so a retry keeps it going instead of starting over.
        string PlayingTrack { get; set; }
        // The zoom the player last chose with the mouse wheel; 0 until they do.
        float PlayerZoom { get; set; }
        // Whether the opening phone call has been heard, ever; kept in the save.
        bool CallHeard { get; set; }
        // Chooses what plays for a contract, or for the hideout when the mission is null; null when there is nothing.
        MusicTrackCVO TrackFor(string mission);
        // The track the current intro will start.
        string QueuedTrack { get; set; }
        // The hideout tape `step` places from the one loaded, round the list: 0 the loaded one (or the first), 1 the
        // next, -1 the previous. Null when there are no hideout tracks.
        MusicTrackCVO Tape(int step);
        // The tape in the deck is stopped; it stays stopped across runs until played again.
        bool TapePaused { get; set; }
        ProgressSVO Progress { get; }
        // The contract being played, or null for the hideout.
        string Mission { get; set; }
        // The city - shops, families, raids and the time between sessions.
        ITerritoryModel Territory { get; }
        // When the open city map is next redrawn, in unscaled seconds.
        float MapRefreshAt { get; set; }
        // The time away has been settled for this launch.
        bool TerritoryCaughtUp { get; set; }
        // The level prefab the next run is played in: the mission's, or the hideout.
        Entities.LevelAuthoring RequestedLevel { get; }
        int UpgradeLevel(string id);
        // Buys the next level of an upgrade; false when it is maxed or the cash is short.
        bool Buy(string id);
        // Pays the current mission's reward the first time it is finished.
        void CompleteMission();
        // The weapon by Id, or null for unarmed or an Id CD_Weapons does not list.
        WeaponCVO Weapon(string id);
        void Drop(Vector2 position, string weapon, int ammo, int reserve);
        void SetLevel(ContractLevelVO level);
        void Reset();
        void Capture(ContractInputVO input);
        void ClearEdges();
        void WriteTerminal(int index, string text);
        LightRVO LightAt(Vector2 position);
    }
}
