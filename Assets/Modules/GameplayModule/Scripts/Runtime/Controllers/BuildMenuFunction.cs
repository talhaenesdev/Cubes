using FlowIoC.BaseModule.Function.ReturnableFunctions;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.LocalizationModule.Services;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // The lines a hideout station shows: one per contract or upgrade, with the player's cash on top.
    internal class BuildMenuFunction : FunctionReturn<MenuVO, StationKind>
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        private string Name(string weapon) => _model.Weapon(weapon) != null ? _loc.Get(_model.Weapon(weapon).DisplayName) : null;
        // One line per shop, its fields joined by MapFieldSeparator in the order the screen's map reads them:
        // name, owner, owner colour, state, income, status, guards, max guards, risk, danger, x, y, main action.
        public const char MapFieldSeparator = '\u001f';

        private MenuVO CityMap(MenuVO menu)
        {
            ITerritoryModel territory = _model.Territory;
            menu.IsMap = true;
            menu.Title = _loc.Get("CITY MAP");
            menu.Subtitle = _loc.Format("{0} / {1} SHOPS  ·  ${2} / HOUR", territory.OwnedCount, territory.LocationCount, territory.IncomePerHour)
                            + (_model.Tuning.UseMoney ? "  ·  " + _loc.Format("CASH  ${0}", _model.Progress.Cash) : "");
            var missions = _model.Missions != null ? _model.Missions.Missions : new System.Collections.Generic.List<MissionCVO>();
            menu.Entries = new string[missions.Count];
            long now = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            for (int i = 0; i < missions.Count; i++)
            {
                MissionCVO mission = missions[i];
                WorkingLocationSVO location = territory.Location(mission.Id);
                bool ours = territory.IsOurs(mission.Id);
                bool attacked = territory.IsUnderAttack(mission.Id);
                bool lost = !ours && location != null && location.Status == WorkingLocationStatus.Lost;
                int state = attacked ? 2 : ours ? 1 : lost ? 3 : 0;
                string owner = territory.OwnerOf(mission);
                string status = "";
                if (attacked)
                {
                    long left = System.Math.Max(0L, location.AttackExpiresUnixMilliseconds - now);
                    status = _loc.Format("UNDER ATTACK BY {0} {1}:{2:00}", _loc.Get(territory.FamilyName(location.AttackingFamily)), left / 60000, left / 1000 % 60);
                }
                else if (ours) status = _loc.Get("WORKS FOR US");
                else status = _loc.Format("WORKS FOR {0}", _loc.Get(territory.FamilyName(owner)));
                float perMinute = territory.IncomePerMinute(mission);
                string income = _model.Tuning.UseMoney && perMinute > 0f ? _loc.Format("${0} / HOUR", Mathf.RoundToInt(perMinute * 60f)) : "—";
                float danger = territory.Wealth(mission);
                RaidVO raid = territory.RaidOn(mission.Id);
                string risk = _loc.Format("RAIDS: {0}  ·  {1} ATTACKERS", _loc.Get(danger < 0.34f ? "LOW" : danger < 0.67f ? "MEDIUM" : "HIGH"), raid.Attackers);
                string action = attacked ? _loc.Get("GO AND HELP") : ours ? "" : _loc.Get("TAKE IT OVER");
                string translated = _loc.Get(mission.Title);
                string name = translated.Contains("/") ? translated[(translated.IndexOf('/') + 1)..].Trim() : translated;
                Color color = territory.FamilyColor(owner);
                menu.Entries[i] = string.Join(MapFieldSeparator.ToString(), name, _loc.Get(territory.FamilyName(owner)), "#" + ColorUtility.ToHtmlStringRGB(color),
                    state.ToString(), income, status, territory.Guards(mission.Id).ToString(), territory.MaxGuards.ToString(), risk,
                    danger.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                    mission.MapPosition.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture),
                    mission.MapPosition.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture), action);
            }
            return menu;
        }

        public override MenuVO Execute(StationKind kind)
        {
            bool money = _model.Tuning.UseMoney;
            var menu = new MenuVO {Subtitle = money ? _loc.Format("CASH  ${0}", _model.Progress.Cash) : ""};
            if (kind == StationKind.FamilyBoard)
            {
                menu.Title = _loc.Get("FAMILY BOARD");
                menu.Subtitle = _loc.Get("INTER-FAMILY RELATIONS");
                var families = _model.Families != null ? _model.Families.Families : new System.Collections.Generic.List<CD_Family>();
                menu.Entries = new string[families.Count];
                for (int i = 0; i < families.Count; i++)
                {
                    CD_Family family = families[i];
                    var friends = new System.Collections.Generic.List<string>();
                    var enemies = new System.Collections.Generic.List<string>();
                    if (family != null)
                    {
                        foreach (CD_Family other in families)
                        {
                            if (other == null || other.Id == family.Id) continue;
                            string name = _loc.Get(other.DisplayName);
                            if (_model.Territory.RelationBetween(family.Id, other.Id) == FamilyDiplomacy.Friendly) friends.Add(name);
                            else enemies.Add(name);
                        }
                    }
                    menu.Entries[i] = _loc.Format("{0} / FRIENDS: {1} / ENEMIES: {2}",
                        family != null ? _loc.Get(family.DisplayName) : "—",
                        friends.Count > 0 ? string.Join(", ", friends) : "—",
                        enemies.Count > 0 ? string.Join(", ", enemies) : "—");
                }
                return menu;
            }
            if (kind == StationKind.MissionBoard) return CityMap(menu);
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
