using FlowIoC.BaseModule.Function.ReturnableFunctions;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.LocalizationModule.Services;
namespace Modules.GameplayModule.Controllers
{
    // The lines a hideout station shows: one per contract or upgrade, with the player's cash on top.
    internal class BuildMenuFunction : FunctionReturn<MenuVO, StationKind>
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        private string Name(string weapon) => _model.Weapon(weapon) != null ? _loc.Get(_model.Weapon(weapon).DisplayName) : null;
        public override MenuVO Execute(StationKind kind)
        {
            bool money = _model.Tuning.UseMoney;
            var menu = new MenuVO {Subtitle = money ? _loc.Format("CASH  ${0}", _model.Progress.Cash) : ""};
            if (kind == StationKind.MissionBoard)
            {
                menu.Title = _loc.Get("MISSION BOARD");
                var missions = _model.Missions != null ? _model.Missions.Missions : new System.Collections.Generic.List<MissionCVO>();
                menu.Entries = new string[missions.Count];
                for (int i = 0; i < missions.Count; i++)
                {
                    MissionCVO mission = missions[i];
                    bool done = _model.Progress.Completed.Contains(mission.Id);
                    menu.Entries[i] = _loc.Get(mission.Title) + "   " + (done ? _loc.Get("DONE") : money && _model.Tuning.MissionsPay ? "$" + mission.Reward : "") + "\n" + _loc.Get(mission.Briefing);
                }
                return menu;
            }
            if (kind == StationKind.Wardrobe)
            {
                menu.Title = _loc.Get("WARDROBE");
                var outfits = _model.Outfits.Outfits;
                menu.Entries = new string[outfits.Count];
                for (int i = 0; i < outfits.Count; i++)
                    menu.Entries[i] = _loc.Get(outfits[i].Name) + (outfits[i].Id == _model.Run.Outfit ? _loc.Get("   (wearing)")
                        : _model.Owns(outfits[i].Id) ? "" : "   $" + outfits[i].Price);
                // Owns() already treats every outfit as owned when outfits are free.
                return menu;
            }
            if (kind == StationKind.WeaponLocker)
            {
                menu.Title = _loc.Get("WEAPON LOCKER");
                var lines = new System.Collections.Generic.List<string>();
                lines.Add(Name(_model.Run.Weapon) != null ? _loc.Format("PUT AWAY  {0}   (hand)", Name(_model.Run.Weapon)) : _loc.Get("HAND EMPTY"));
                lines.Add(Name(_model.Run.Holster) != null ? _loc.Format("PUT AWAY  {0}   (pocket)", Name(_model.Run.Holster)) : _loc.Get("POCKET EMPTY"));
                foreach (string stored in _model.Progress.Locker)
                    if (Name(stored) != null) lines.Add(_loc.Format("TAKE  {0}", Name(stored)));
                menu.Entries = lines.ToArray();
                return menu;
            }
            menu.Title = _loc.Get("UPGRADE BENCH");
            var upgrades = _model.Upgrades.Upgrades;
            menu.Entries = new string[upgrades.Count];
            for (int i = 0; i < upgrades.Count; i++)
            {
                UpgradeCVO upgrade = upgrades[i];
                int level = _model.UpgradeLevel(upgrade.Id);
                string price = level >= upgrade.MaxLevel ? _loc.Get("MAXED") : money ? "$" + upgrade.Cost * (level + 1) : "";
                menu.Entries[i] = _loc.Get(upgrade.Title) + "   " + _loc.Format("LV {0}/{1}", level, upgrade.MaxLevel) + "   " + price + "\n" + _loc.Get(upgrade.Description);
            }
            return menu;
        }
    }
}
