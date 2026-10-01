using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
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
        CD_Missions Missions { get; }
        CD_Upgrades Upgrades { get; }
        ProgressSVO Progress { get; }
        // The contract being played, or null for the hideout.
        string Mission { get; set; }
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
