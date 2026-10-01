using FlowIoC.BaseModule.Function.ReturnableFunctions;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
namespace Modules.GameplayModule.Controllers
{
    // The lines a hideout station shows: one per contract or upgrade, with the player's cash on top.
    internal class BuildMenuFunction : FunctionReturn<MenuVO, StationKind>
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override MenuVO Execute(StationKind kind)
        {
            var menu = new MenuVO {Subtitle = "CASH  $" + _model.Progress.Cash};
            if (kind == StationKind.MissionBoard)
            {
                menu.Title = "MISSION BOARD";
                var missions = _model.Missions != null ? _model.Missions.Missions : new System.Collections.Generic.List<MissionCVO>();
                menu.Entries = new string[missions.Count];
                for (int i = 0; i < missions.Count; i++)
                {
                    MissionCVO mission = missions[i];
                    bool done = _model.Progress.Completed.Contains(mission.Id);
                    menu.Entries[i] = mission.Title + "   " + (done ? "DONE" : "$" + mission.Reward) + "\n" + mission.Briefing;
                }
                return menu;
            }
            menu.Title = "UPGRADE BENCH";
            var upgrades = _model.Upgrades.Upgrades;
            menu.Entries = new string[upgrades.Count];
            for (int i = 0; i < upgrades.Count; i++)
            {
                UpgradeCVO upgrade = upgrades[i];
                int level = _model.UpgradeLevel(upgrade.Id);
                string price = level >= upgrade.MaxLevel ? "MAXED" : "$" + upgrade.Cost * (level + 1);
                menu.Entries[i] = upgrade.Title + "   LV " + level + "/" + upgrade.MaxLevel + "   " + price + "\n" + upgrade.Description;
            }
            return menu;
        }
    }
}
