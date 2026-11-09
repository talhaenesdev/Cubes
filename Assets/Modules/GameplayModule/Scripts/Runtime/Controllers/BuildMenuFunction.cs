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
                float paymentsPerHour = mission.WorkPaymentInterval > 0f ? 3600f / mission.WorkPaymentInterval : 0f;
                // The card's money in plain words: what the shop pays, what our men there cost, what is left for us.
                int grossPerHour = Mathf.RoundToInt(territory.GrossPayment(mission) * paymentsPerHour);
                int netPerHour = Mathf.RoundToInt(territory.NetPayment(mission) * paymentsPerHour);
                int guards = territory.Guards(mission.Id);
                int manPerHour = Mathf.RoundToInt(territory.Config.GuardUpkeepPerPayment * paymentsPerHour);
                string income = "—";
                if (_model.Tuning.UseMoney && mission.WorkPayment > 0)
                {
                    string cost = guards > 0
                        ? _loc.Format("Shop pays ${0}, your {1} men take ${2}", grossPerHour, guards, guards * manPerHour)
                        : _loc.Get("No men posted, you keep it all");
                    income = ours || attacked
                        ? _loc.Format("YOU GET ${0} / HOUR", netPerHour) + "\n<size=12>" + cost + "\n"
                          + _loc.Format("A new man: ${0} once, then ${1} / hour", territory.Config.GuardHirePrice, manPerHour) + "</size>"
                        : _loc.Format("PAYS ${0} / HOUR", grossPerHour) + "\n<size=12>" + _loc.Get("Yours once you take it over") + "</size>";
                }
                float danger = territory.Wealth(mission);
                RaidVO raid = territory.RaidOn(mission.Id);
                string level = _loc.Get(danger < 0.34f ? "LOW" : danger < 0.67f ? "MEDIUM" : "HIGH");
                string risk = ours || attacked ? _loc.Format("{0}  ·  a raid brings {1} men", level, raid.Attackers) : level;
                // On our own shop the big button puts money into it; at the top level there is nothing left to buy.
                int upgrade = territory.ShopUpgradePrice(mission.Id);
                int shopLevel = territory.ShopLevel(mission.Id);
                string action = attacked ? _loc.Get("GO AND HELP")
                    : ours ? shopLevel < territory.Config.MaxShopLevel
                        ? _loc.Format("UPGRADE SHOP  ${0}  (+{1}%)", upgrade, Mathf.RoundToInt(territory.Config.ShopIncomePerLevel * 100f)) : ""
                    : _loc.Get("TAKE IT OVER");
                string translated = _loc.Get(mission.Title);
                string name = translated.Contains("/") ? translated[(translated.IndexOf('/') + 1)..].Trim() : translated;
                // How hard the job is, at a glance, and how far the shop has been built up.
                int stars = (int) mission.LocationDifficulty + 1;
                name += "  " + new string('\u2605', stars) + new string('\u2606', 3 - stars);
                if (ours && shopLevel > 0) name += "  " + _loc.Format("LV {0}", shopLevel);
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
                ITerritoryModel territory = _model.Territory;
                PlayerStatsSVO stats = _model.Progress.Stats ?? new PlayerStatsSVO();
                menu.Subtitle = _loc.Format("TAKEN {0}  ·  LOST {1}  ·  RAIDS BEATEN {2}  ·  MEN DOWN {3}  ·  SHOPS EARNED ${4}",
                    stats.ShopsTaken, stats.ShopsLost, stats.RaidsRepelled, stats.MenDown, stats.ShopIncome)
                    + (money ? "  ·  " + _loc.Format("CASH  ${0}", _model.Progress.Cash) : "");
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
                    // An enemy of ours can be bought off: picking its line pays for a truce.
                    if (family != null && territory.RelationToPlayer(family.Id) == FamilyRelation.Enemy)
                        menu.Entries[i] += "\n" + _loc.Format("Pick to buy a truce: ${0}", territory.Config.TrucePrice);
                    else if (family != null && _model.Progress.TruceFamilies != null && _model.Progress.TruceFamilies.Contains(family.Id))
                        menu.Entries[i] += "\n" + _loc.Get("Truce: they leave our shops alone - unless we hit theirs");
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
