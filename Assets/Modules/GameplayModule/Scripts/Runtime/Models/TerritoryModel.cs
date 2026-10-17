using System;
using System.Collections.Generic;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Models
{
    // The city's shops and the families that run them. One UTC clock covers the game open and closed: a shop's
    // payments are counted from its last paid boundary, a raid has an expiry time, and the time since the game was
    // last open loses the player one shop for every HoursPerLostShop.
    //
    // It works on the ProgressSVO the NightShiftModel loads and saves, so the city and the rest of the player's
    // progress stay in one save file; saving goes back through the owner.
    public class TerritoryModel : ITerritoryModel
    {
        private const long Minute = 60000L;
        private const long HeartbeatSave = 30000L;
        private readonly Func<ProgressSVO> _progress;
        private readonly CD_Missions _missions;
        private readonly CD_Families _families;
        private readonly Action _save;
        private readonly Action<int> _cashChanged;
        private readonly bool _useMoney;
        private long _lastHeartbeatSave;

        public CD_Territory Config { get; }
        public TerritoryReportVO PendingReport { get; set; }
        public string DefenseMission { get; private set; }

        public TerritoryModel(Func<ProgressSVO> progress, CD_Missions missions, CD_Families families, CD_Territory config,
            bool useMoney, Action save, Action<int> cashChanged)
        {
            _progress = progress;
            _missions = missions;
            _families = families;
            Config = config != null ? config : ScriptableObject.CreateInstance<CD_Territory>();
            _useMoney = useMoney;
            _save = save;
            _cashChanged = cashChanged;
        }

        private ProgressSVO Progress => _progress();
        private IEnumerable<MissionCVO> Missions => _missions != null ? _missions.Missions : Array.Empty<MissionCVO>();
        private string PlayerFamily => _families != null && _families.PlayerFamily != null ? _families.PlayerFamily.Id : "player";
        private long RaidRoll => Math.Max(1L, (long) (Config.RaidRollMinutes * Minute));
        private long RaidResponse => Math.Max(1L, (long) (Config.RaidResponseMinutes * Minute));
        private long LostShopPeriod => Math.Max(Minute, (long) (Config.HoursPerLostShop * 60d * Minute));

        // ---------------------------------------------------------------- shops

        public WorkingLocationSVO Location(string mission) => Progress.WorkingLocations.Find(item => item.Mission == mission);

        public bool IsOurs(string mission)
        {
            WorkingLocationSVO location = Location(mission);
            return location != null && location.Status != WorkingLocationStatus.Lost && Progress.Completed.Contains(mission);
        }

        public bool IsUnderAttack(string mission) => Location(mission)?.Status == WorkingLocationStatus.UnderAttack;

        public int OwnedCount
        {
            get
            {
                int count = 0;
                foreach (MissionCVO mission in Missions) if (IsOurs(mission.Id)) count++;
                return count;
            }
        }

        public int LocationCount => _missions != null ? _missions.Missions.Count : 0;
        public bool AllOurs => LocationCount > 0 && OwnedCount == LocationCount;

        public string OwnerOf(MissionCVO mission)
        {
            if (mission == null) return null;
            if (IsOurs(mission.Id)) return PlayerFamily;
            WorkingLocationSVO location = Location(mission.Id);
            if (location != null && location.Status == WorkingLocationStatus.Lost && !string.IsNullOrEmpty(location.OwnerFamily))
                return location.OwnerFamily;
            return mission.OwnerFamily;
        }

        public float IncomePerMinute(MissionCVO mission) =>
            mission == null || mission.WorkPayment <= 0 || mission.WorkPaymentInterval <= 0f ? 0f : mission.WorkPayment * 60f / mission.WorkPaymentInterval;

        public float Wealth(MissionCVO mission)
        {
            float low = float.MaxValue, high = 0f;
            foreach (MissionCVO other in Missions)
            {
                float income = IncomePerMinute(other);
                low = Mathf.Min(low, income);
                high = Mathf.Max(high, income);
            }
            if (high <= low) return 0.5f;
            float wealth = Mathf.InverseLerp(low, high, IncomePerMinute(mission));
            // A location marked harder in CD_Missions is treated as richer still.
            return Mathf.Clamp01(wealth + (int) mission.LocationDifficulty * 0.15f);
        }

        public int IncomePerHour
        {
            get
            {
                float total = 0f;
                foreach (MissionCVO mission in Missions)
                    if (IsOurs(mission.Id) && !IsUnderAttack(mission.Id)) total += IncomePerMinute(mission) * 60f;
                return Mathf.RoundToInt(total);
            }
        }

        public void Claim(MissionCVO mission)
        {
            if (mission == null) return;
            long now = Now;
            if (!Progress.Completed.Contains(mission.Id)) Progress.Completed.Add(mission.Id);
            WorkingLocationSVO location = Location(mission.Id);
            if (location == null)
            {
                location = new WorkingLocationSVO {Mission = mission.Id};
                Progress.WorkingLocations.Add(location);
            }
            Settle(location, WorkingLocationStatus.Owned, now);
            WorkingLocationPaymentSVO paid = Progress.WorkingLocationPayments.Find(item => item.Mission == mission.Id);
            if (paid == null) Progress.WorkingLocationPayments.Add(new WorkingLocationPaymentSVO {Mission = mission.Id, LastPaymentUnixMilliseconds = now});
            else paid.LastPaymentUnixMilliseconds = now;
            _save();
        }

        private void Settle(WorkingLocationSVO location, WorkingLocationStatus status, long now)
        {
            location.Status = status;
            if (status == WorkingLocationStatus.Owned) location.OwnerFamily = null;
            location.AttackingFamily = null;
            location.AttackExpiresUnixMilliseconds = 0;
            location.AttackNoticeSent = false;
            location.NextAttackRollUnixMilliseconds = now + RaidRoll;
        }

        private void Lose(WorkingLocationSVO location, MissionCVO mission, string family)
        {
            location.Status = WorkingLocationStatus.Lost;
            location.OwnerFamily = !string.IsNullOrEmpty(family) ? family
                : !string.IsNullOrEmpty(location.AttackingFamily) ? location.AttackingFamily
                : mission != null ? mission.AttackingFamily : "rival";
            location.AttackingFamily = null;
            location.AttackExpiresUnixMilliseconds = 0;
            location.AttackNoticeSent = false;
            Progress.Completed.Remove(location.Mission);
            Progress.WorkingLocationPayments.RemoveAll(item => item.Mission == location.Mission);
            // The men posted there went down with it.
            SetGuardsQuietly(location.Mission, 0);
        }

        // ---------------------------------------------------------------- guards

        public int MaxGuards => Mathf.Max(1, Config.MaxGuardsPerLocation);

        public int Guards(string mission)
        {
            MissionGuardSVO entry = Progress.MissionGuards.Find(item => item.Mission == mission);
            return entry != null ? Mathf.Clamp(entry.Count, 0, MaxGuards) : 0;
        }

        public void SetGuards(string mission, int count)
        {
            SetGuardsQuietly(mission, count);
            _save();
        }

        private void SetGuardsQuietly(string mission, int count)
        {
            MissionGuardSVO entry = Progress.MissionGuards.Find(item => item.Mission == mission);
            if (entry == null)
            {
                entry = new MissionGuardSVO {Mission = mission};
                Progress.MissionGuards.Add(entry);
            }
            entry.Count = Mathf.Clamp(count, 0, MaxGuards);
        }

        // ---------------------------------------------------------------- families

        private CD_Family Family(string id) => _families != null ? _families.Find(id) : null;

        public string FamilyName(string id)
        {
            CD_Family family = Family(id);
            return family != null && !string.IsNullOrEmpty(family.DisplayName) ? family.DisplayName : id;
        }

        public Color FamilyColor(string id)
        {
            CD_Family family = Family(id);
            return family != null ? family.Color : new Color(0.5f, 0.5f, 0.5f);
        }

        public FamilyRelation RelationToPlayer(string family)
        {
            if (_families == null || _families.PlayerFamily == null) return FamilyRelation.Enemy;
            if (family == _families.PlayerFamily.Id) return FamilyRelation.Player;
            return RelationBetween(_families.PlayerFamily.Id, family) == FamilyDiplomacy.Friendly ? FamilyRelation.Friendly : FamilyRelation.Enemy;
        }

        public FamilyDiplomacy RelationBetween(string firstFamily, string secondFamily)
        {
            if (_families == null) return FamilyDiplomacy.Enemy;
            string other = firstFamily == PlayerFamily ? secondFamily : secondFamily == PlayerFamily ? firstFamily : null;
            if (!string.IsNullOrEmpty(other) && Progress.EnemyFamilies != null && Progress.EnemyFamilies.Contains(other)) return FamilyDiplomacy.Enemy;
            return _families.Relation(firstFamily, secondFamily);
        }

        public bool DeclareEnemy(string family)
        {
            if (string.IsNullOrEmpty(family) || RelationToPlayer(family) != FamilyRelation.Friendly) return false;
            Progress.EnemyFamilies ??= new List<string>();
            Progress.EnemyFamilies.Add(family);
            _save();
            return true;
        }

        // A family at war with the player that this shop allows to raid it; null when nobody can.
        private string Raider(MissionCVO mission)
        {
            var candidates = new List<string>();
            if (_families != null)
                foreach (CD_Family family in _families.Families)
                {
                    if (family == null || family.Id == PlayerFamily || RelationBetween(family.Id, PlayerFamily) != FamilyDiplomacy.Enemy) continue;
                    if (mission.AttackingFamilies != null && mission.AttackingFamilies.Count > 0 && !mission.AttackingFamilies.Contains(family.Id)) continue;
                    candidates.Add(family.Id);
                }
            if (candidates.Count > 0) return candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return RelationToPlayer(mission.AttackingFamily) == FamilyRelation.Enemy ? mission.AttackingFamily : null;
        }

        // ---------------------------------------------------------------- time

        private static long Now => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public void Heartbeat(long utcMilliseconds)
        {
            Progress.LastSeenUnixMilliseconds = utcMilliseconds;
            if (utcMilliseconds - _lastHeartbeatSave < HeartbeatSave) return;
            _lastHeartbeatSave = utcMilliseconds;
            _save();
        }

        public TerritoryReportVO CatchUp(long now)
        {
            var report = new TerritoryReportVO();
            long lastSeen = Progress.LastSeenUnixMilliseconds;
            Progress.LastSeenUnixMilliseconds = now;
            if (_missions == null || lastSeen <= 0 || now <= lastSeen)
            {
                ResetRaidRolls(now);
                _save();
                return report;
            }
            report.AwayMilliseconds = now - lastSeen;
            int cashBefore = Progress.Cash;

            // A raid that was on when the game closed ran its course without the player.
            foreach (MissionCVO mission in Missions)
            {
                WorkingLocationSVO location = Location(mission.Id);
                if (location == null || location.Status != WorkingLocationStatus.UnderAttack || location.AttackExpiresUnixMilliseconds > now) continue;
                Pay(mission, location.AttackExpiresUnixMilliseconds);
                if (HoldWithoutPlayer(location, mission)) report.Held.Add(mission.Id);
                else report.Lost.Add(mission.Id);
            }

            // One shop lost for every full period away, the next at each period's end. A shop earns until it goes.
            int periods = (int) Math.Min(LocationCount, (now - lastSeen) / LostShopPeriod);
            for (int i = 1; i <= periods; i++)
            {
                long at = lastSeen + i * LostShopPeriod;
                MissionCVO victim = PickLostShop();
                if (victim == null) break;
                Pay(victim, at);
                WorkingLocationSVO location = Location(victim.Id);
                Lose(location, victim, Raider(victim));
                report.Lost.Add(victim.Id);
            }

            // What still works earned all the way to now. Raids are not rolled for the time away: that cost is the lost shops.
            CollectPaymentsQuietly(now);
            ResetRaidRolls(now);
            report.Income = Math.Max(0, Progress.Cash - cashBefore);
            _cashChanged(Progress.Cash);
            _save();
            PendingReport = report.IsEmpty ? null : report;
            return report;
        }

        // The shop lost while away: one of ours, the fewer men guard it the likelier.
        private MissionCVO PickLostShop()
        {
            var owned = new List<MissionCVO>();
            var weights = new List<float>();
            float total = 0f;
            foreach (MissionCVO mission in Missions)
            {
                if (!IsOurs(mission.Id)) continue;
                float weight = 1f / (1f + Guards(mission.Id) * Config.GuardKeepWeight);
                owned.Add(mission);
                weights.Add(weight);
                total += weight;
            }
            if (owned.Count == 0) return null;
            float roll = UnityEngine.Random.value * total;
            for (int i = 0; i < owned.Count; i++)
                if ((roll -= weights[i]) <= 0f) return owned[i];
            return owned[owned.Count - 1];
        }

        private void ResetRaidRolls(long now)
        {
            foreach (WorkingLocationSVO location in Progress.WorkingLocations)
                if (location.Status == WorkingLocationStatus.Owned) location.NextAttackRollUnixMilliseconds = now + RaidRoll;
        }

        public int CollectPayments(long now)
        {
            int paid = CollectPaymentsQuietly(now);
            if (paid > 0) _save();
            return paid;
        }

        private int CollectPaymentsQuietly(long now)
        {
            int total = 0;
            foreach (MissionCVO mission in Missions) total += Pay(mission, now);
            return total;
        }

        // Every whole payment a shop has earned up to `until`; a shop under attack earns nothing.
        private int Pay(MissionCVO mission, long until)
        {
            if (!_useMoney || !Config.LocationsPay || mission == null || !IsOurs(mission.Id) || IsUnderAttack(mission.Id)) return 0;
            if (mission.WorkPayment <= 0 || mission.WorkPaymentInterval <= 0f) return 0;
            WorkingLocationPaymentSVO state = Progress.WorkingLocationPayments.Find(item => item.Mission == mission.Id);
            if (state == null)
            {
                Progress.WorkingLocationPayments.Add(new WorkingLocationPaymentSVO {Mission = mission.Id, LastPaymentUnixMilliseconds = until});
                return 0;
            }
            long interval = Math.Max(1L, (long) (mission.WorkPaymentInterval * 1000d));
            long elapsed = until - state.LastPaymentUnixMilliseconds;
            if (elapsed < interval) return 0;
            long due = elapsed / interval;
            long earned = Math.Min(int.MaxValue, due * mission.WorkPayment);
            state.LastPaymentUnixMilliseconds += due * interval;
            Progress.Cash = (int) Math.Min(int.MaxValue, (long) Progress.Cash + earned);
            _cashChanged(Progress.Cash);
            return (int) earned;
        }

        public WorkingLocationUpdateVO ProcessRaids(long now)
        {
            var update = new WorkingLocationUpdateVO();
            if (_missions == null) return update;
            bool save = false;
            foreach (MissionCVO mission in Missions)
            {
                if (!IsOurs(mission.Id)) continue;
                WorkingLocationSVO location = Location(mission.Id);
                if (location.Status == WorkingLocationStatus.UnderAttack)
                {
                    if (mission.Id == DefenseMission) continue;
                    if (now >= location.AttackExpiresUnixMilliseconds)
                    {
                        // Nobody came: the posted men hold it, or they don't.
                        if (HoldWithoutPlayer(location, mission)) update.HeldMission ??= mission.Id;
                        else update.LostMission ??= mission.Id;
                        save = true;
                    }
                    else if (!location.AttackNoticeSent)
                    {
                        location.AttackNoticeSent = true;
                        update.AttackedMission ??= mission.Id;
                        save = true;
                    }
                    continue;
                }
                if (!Config.Raids || mission.AttackChance <= 0f) continue;
                if (location.NextAttackRollUnixMilliseconds <= 0)
                {
                    location.NextAttackRollUnixMilliseconds = now + RaidRoll;
                    save = true;
                    continue;
                }
                if (location.NextAttackRollUnixMilliseconds > now) continue;
                location.NextAttackRollUnixMilliseconds = now + RaidRoll;
                save = true;
                // Richer shops draw more raids.
                float chance = mission.AttackChance * (1f + Config.RichShopRaidBonus * Wealth(mission));
                if (UnityEngine.Random.value * 100f >= chance) continue;
                string raider = Raider(mission);
                if (raider == null) continue;
                Pay(mission, now);
                location.Status = WorkingLocationStatus.UnderAttack;
                location.AttackingFamily = raider;
                location.AttackExpiresUnixMilliseconds = now + RaidResponse;
                location.AttackNoticeSent = true;
                update.AttackedMission ??= mission.Id;
            }
            if (save) _save();
            return update;
        }

        // The posted men against the raiders, without the player: the more of them, the better their chance.
        private bool HoldWithoutPlayer(WorkingLocationSVO location, MissionCVO mission)
        {
            RaidVO raid = RaidOn(mission.Id);
            float guards = Guards(mission.Id) * Config.GuardStrength;
            float attackers = raid.Attackers * raid.Aggression * (1f + raid.Toughness * 0.25f);
            bool held = guards > 0f && UnityEngine.Random.value < guards / (guards + attackers);
            if (held)
            {
                int lost = Mathf.CeilToInt(Guards(mission.Id) * Config.GuardLosses);
                SetGuardsQuietly(mission.Id, Guards(mission.Id) - lost);
                Settle(location, WorkingLocationStatus.Owned, Now);
                WorkingLocationPaymentSVO paid = Progress.WorkingLocationPayments.Find(item => item.Mission == mission.Id);
                if (paid != null) paid.LastPaymentUnixMilliseconds = Now;
            }
            else Lose(location, mission, location.AttackingFamily);
            return held;
        }

        // ---------------------------------------------------------------- defence

        public RaidVO RaidOn(string missionId)
        {
            MissionCVO mission = _missions != null ? _missions.Find(missionId) : null;
            WorkingLocationSVO location = Location(missionId);
            float wealth = mission != null ? Wealth(mission) : 0f;
            return new RaidVO
            {
                Family = location != null && !string.IsNullOrEmpty(location.AttackingFamily) ? location.AttackingFamily
                    : mission != null ? mission.AttackingFamily : "rival",
                Attackers = Mathf.RoundToInt(Mathf.Lerp(Mathf.Min(Config.MinAttackers, Config.MaxAttackers), Config.MaxAttackers, wealth)),
                Aggression = Mathf.Lerp(Config.AttackerAggression.x, Config.AttackerAggression.y, wealth),
                Toughness = Mathf.FloorToInt(Config.RichShopToughness * wealth + 0.0001f),
                Defenders = Guards(missionId),
            };
        }

        public bool StartDefense(string mission)
        {
            WorkingLocationSVO location = Location(mission);
            if (location == null || location.Status != WorkingLocationStatus.UnderAttack || Now >= location.AttackExpiresUnixMilliseconds) return false;
            DefenseMission = mission;
            return true;
        }

        public void DefenseWon(int defendersLost)
        {
            WorkingLocationSVO location = Location(DefenseMission);
            if (location != null)
            {
                Settle(location, WorkingLocationStatus.Owned, Now);
                SetGuardsQuietly(DefenseMission, Guards(DefenseMission) - Mathf.Max(0, defendersLost));
                WorkingLocationPaymentSVO paid = Progress.WorkingLocationPayments.Find(item => item.Mission == DefenseMission);
                if (paid != null) paid.LastPaymentUnixMilliseconds = Now;
            }
            DefenseMission = null;
            _save();
        }

        public void DefenseLost()
        {
            MissionCVO mission = _missions != null ? _missions.Find(DefenseMission) : null;
            WorkingLocationSVO location = Location(DefenseMission);
            if (location != null) Lose(location, mission, location.AttackingFamily);
            DefenseMission = null;
            _save();
        }

#if UNITY_EDITOR
        private MissionCVO FirstWith(Predicate<string> test)
        {
            foreach (MissionCVO mission in Missions) if (test(mission.Id)) return mission;
            return null;
        }

        public string DebugRaid()
        {
            MissionCVO mission = FirstWith(id => IsOurs(id) && !IsUnderAttack(id));
            if (mission == null) return null;
            WorkingLocationSVO location = Location(mission.Id);
            location.Status = WorkingLocationStatus.UnderAttack;
            location.AttackingFamily = Raider(mission) ?? mission.AttackingFamily;
            location.AttackExpiresUnixMilliseconds = Now + RaidResponse;
            location.AttackNoticeSent = true;
            _save();
            return mission.Id;
        }

        public string DebugExpireRaid()
        {
            MissionCVO mission = FirstWith(IsUnderAttack);
            if (mission == null) return null;
            Location(mission.Id).AttackExpiresUnixMilliseconds = Now - 1;
            return mission.Id;
        }

        public string DebugHoldRaid()
        {
            MissionCVO mission = FirstWith(IsUnderAttack);
            if (mission == null) return null;
            Settle(Location(mission.Id), WorkingLocationStatus.Owned, Now);
            _save();
            return mission.Id;
        }

        public int DebugPayOnce()
        {
            int total = 0;
            foreach (MissionCVO mission in Missions)
                if (_useMoney && IsOurs(mission.Id) && !IsUnderAttack(mission.Id)) total += mission.WorkPayment;
            Progress.Cash = (int) Math.Min(int.MaxValue, (long) Progress.Cash + total);
            _cashChanged(Progress.Cash);
            _save();
            return total;
        }

        // Pretends the game was closed for `days` periods: moves every clock back and catches up.
        public TerritoryReportVO DebugSkipDays(int days)
        {
            long shift = days * LostShopPeriod;
            Progress.LastSeenUnixMilliseconds = Now - shift;
            foreach (WorkingLocationPaymentSVO paid in Progress.WorkingLocationPayments) paid.LastPaymentUnixMilliseconds -= shift;
            return CatchUp(Now);
        }
#endif
    }
}
