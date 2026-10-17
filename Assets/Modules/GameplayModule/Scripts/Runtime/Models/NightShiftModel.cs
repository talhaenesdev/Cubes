using FlowIoC.BaseModule.Adapters;
using FlowIoC.BaseModule.Constructables;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.RootsContexts;
using UnityEngine;
namespace Modules.GameplayModule.Models
{
    public class NightShiftModel : INightShiftModel, IConstructable
    {
        [Inject(nameof(GameplaySystemContext))] private GameObject _root { get; set; }
        public bool IsPostConstructed { get; set; }
        public bool IsDeconstructed { get; set; }
        public CD_Weapons Weapons { get; private set; }
        public CD_Characters Characters { get; private set; }
        public CD_Families Families { get; private set; }
        public CD_Missions Missions { get; private set; }
        public CD_Upgrades Upgrades { get; private set; }
        public CD_Music Music { get; private set; }
        public CD_Outfits Outfits { get; private set; }
        public CD_Gameplay Tuning { get; private set; }
        public bool Wear(string outfit)
        {
            OutfitCVO chosen = Outfits != null ? Outfits.Find(outfit) : null;
            if (chosen == null) return false;
            if (!Owns(outfit))
            {
                int cost = Tuning.UseMoney && Tuning.OutfitsCost ? chosen.Price : 0;
                if (Progress.Cash < cost) return false;
                Progress.Cash -= cost;
                Progress.OwnedOutfits.Add(outfit);
                Run.Cash = Progress.Cash;
            }
            Progress.Outfit = outfit;
            Run.Outfit = outfit;
            SaveProgress();
            return true;
        }
        public bool Owns(string outfit)
        {
            OutfitCVO found = Outfits != null ? Outfits.Find(outfit) : null;
            return found != null && (found.Price <= 0 || !Tuning.UseMoney || !Tuning.OutfitsCost || Progress.OwnedOutfits.Contains(outfit));
        }
        public bool MenuOpen { get; set; }
        public bool PausedBeforeMenu { get; set; }
        public void StoreWeapon(int slot)
        {
            string weapon = slot == 0 ? Run.Weapon : Run.Holster;
            if (Weapon(weapon) == null) return;
            // The locker keeps one of each; a second of a kind already in it is simply left behind.
            if (!Progress.Locker.Contains(weapon)) Progress.Locker.Add(weapon);
            if (slot == 0)
            {
                // The pocket weapon comes up to the hand.
                Run.Weapon = Run.Holster;
                Run.Ammo = Run.HolsterAmmo;
                Run.Reserve = Run.HolsterReserve;
                Run.Holster = null;
            }
            else Run.Holster = null;
            KeepLoadout();
        }
        public void TakeWeapon(string weapon)
        {
            WeaponCVO taken = Weapon(weapon);
            if (taken == null || !Progress.Locker.Remove(weapon)) return;
            if (Weapon(Run.Weapon) != null)
            {
                if (string.IsNullOrEmpty(Run.Holster))
                {
                    Run.Holster = Run.Weapon;
                    Run.HolsterAmmo = Run.Ammo;
                    Run.HolsterReserve = Run.Reserve;
                }
                else if (!Progress.Locker.Contains(Run.Weapon)) Progress.Locker.Add(Run.Weapon);
            }
            Run.Weapon = taken.Id;
            Run.Ammo = taken.Magazine;
            Run.Reserve = taken.Reserve;
            KeepLoadout();
        }
        // What the player carries is what the next run starts with.
        private void KeepLoadout()
        {
            Progress.Hand = Run.Weapon;
            Progress.Pocket = Run.Holster;
            SaveProgress();
        }
        public string PlayingTrack { get; set; }
        public float PlayerZoom { get; set; }
        public bool CallHeard
        {
            get => Progress.CallHeard;
            set
            {
                if (Progress.CallHeard == value) return;
                Progress.CallHeard = value;
                SaveProgress();
            }
        }
        public string QueuedTrack { get; set; }
        public bool TapePaused { get; set; }
        public MusicTrackCVO Tape(int step)
        {
            if (Music == null) return null;
            var tapes = Music.Tracks.FindAll(track => track.Category == MusicCategory.Hideout && track.Clip != null);
            if (tapes.Count == 0) return null;
            int playing = tapes.FindIndex(track => track.Id == PlayingTrack);
            if (playing < 0)
            {
                // The first tape is the chosen HideoutTrack when there is one.
                int chosen = tapes.FindIndex(track => track.Id == Music.HideoutTrack);
                return tapes[chosen >= 0 ? chosen : 0];
            }
            return tapes[((playing + step) % tapes.Count + tapes.Count) % tapes.Count];
        }
        public MusicTrackCVO TrackFor(string mission)
        {
            if (Music == null) return null;
            if (Missions == null || Missions.Find(mission) == null)
                return Music.Find(Music.HideoutTrack) ?? Music.Pick(MusicCategory.Hideout);
            string own = Missions.Find(mission).Music;
            return Music.Find(own) ?? Music.Find(Music.ContractTrack) ?? Music.Pick(MusicCategory.Contract);
        }
        public ProgressSVO Progress { get; private set; } = new();
        public string Mission { get; set; }
        public ITerritoryModel Territory { get; private set; }
        public float MapRefreshAt { get; set; }
        public bool TerritoryCaughtUp { get; set; }
        public CD_Territory TerritoryConfig { get; private set; }
        public Entities.LevelAuthoring RequestedLevel
        {
            get
            {
                if (Missions == null) return null;
                MissionCVO mission = Missions.Find(Mission);
                return mission != null ? mission.Level : Missions.Hideout;
            }
        }
        private const string ProgressKey = "nightshift.progress";
        public ContractRVO Run { get; private set; } = new();
        public ContractInputVO Input { get; private set; }
        public ContractLevelVO Level { get; private set; } = new();
        public bool IsTicking { get; set; }
        public bool IsHooked { get; set; }
        public int Remaining
        {
            get
            {
                int count = 0;
                foreach (GuardRVO guard in Run.Guards) if (guard.Alive && guard.Faction == Faction.Enemy) count++;
                return count;
            }
        }
        // The weapons and cast come off GameplaySystemRoot's RootAdapter; without them the shipped defaults play.
        public void PostConstruct()
        {
            var adapter = _root != null ? _root.GetComponent<RootAdapter>() : null;
            if (adapter != null)
            {
                Weapons = adapter.GetScriptable<CD_Weapons>();
                Characters = adapter.GetScriptable<CD_Characters>();
                Families = adapter.GetScriptable<CD_Families>();
                Missions = adapter.GetScriptable<CD_Missions>();
                Upgrades = adapter.GetScriptable<CD_Upgrades>();
                Music = adapter.GetScriptable<CD_Music>();
                Outfits = adapter.GetScriptable<CD_Outfits>();
                Tuning = adapter.GetScriptable<CD_Gameplay>();
                TerritoryConfig = adapter.GetScriptable<CD_Territory>();
            }
            else FlowLogger.LogWarning("GameplaySystemRoot has no RootAdapter; the default weapons and characters are used.");
            if (Weapons == null) { Weapons = ScriptableObject.CreateInstance<CD_Weapons>(); Weapons.FillDefaults(); }
            if (Characters == null) { Characters = ScriptableObject.CreateInstance<CD_Characters>(); Characters.FillDefaults(); }
            if (Families == null) Families = DefaultFamilies();
            if (Upgrades == null) { Upgrades = ScriptableObject.CreateInstance<CD_Upgrades>(); Upgrades.FillDefaults(); }
            if (Outfits == null) { Outfits = ScriptableObject.CreateInstance<CD_Outfits>(); Outfits.FillDefaults(); }
            if (Tuning == null) Tuning = ScriptableObject.CreateInstance<CD_Gameplay>();
            Progress = LoadProgress();
            Territory = new TerritoryModel(() => Progress, Missions, Families, TerritoryConfig, Tuning.UseMoney, SaveProgress, cash => Run.Cash = cash);
            // An unresolved raid is announced once again after every launch, even if its first banner was missed.
            foreach (WorkingLocationSVO location in Progress.WorkingLocations)
                if (location.Status == WorkingLocationStatus.UnderAttack) location.AttackNoticeSent = false;
        }
        public void Deconstruct() { }
        public int UpgradeLevel(string id)
        {
            foreach (UpgradeLevelSVO bought in Progress.Upgrades)
                if (bought.Id == id) return bought.Level;
            return 0;
        }
        public bool Buy(string id)
        {
            UpgradeCVO upgrade = null;
            foreach (UpgradeCVO candidate in Upgrades.Upgrades) if (candidate.Id == id) upgrade = candidate;
            if (upgrade == null) return false;
            int level = UpgradeLevel(id);
            int price = Tuning.UseMoney ? upgrade.Cost * (level + 1) : 0;
            if (level >= upgrade.MaxLevel || Progress.Cash < price) return false;
            Progress.Cash -= price;
            UpgradeLevelSVO entry = Progress.Upgrades.Find(bought => bought.Id == id);
            if (entry == null) Progress.Upgrades.Add(new UpgradeLevelSVO {Id = id, Level = 1});
            else entry.Level++;
            Run.Cash = Progress.Cash;
            SaveProgress();
            return true;
        }
        public void CompleteMission()
        {
            if (Tuning.UseMoney && Run.CashCarried > 0)
            {
                Progress.Cash += Run.CashCarried;
                Run.Cash = Progress.Cash;
                Run.CashCarried = 0;
                SaveProgress();
            }
            MissionCVO mission = Missions != null ? Missions.Find(Mission) : null;
            if (mission == null) return;
            bool first = !Progress.Completed.Contains(mission.Id);
            Territory.Claim(mission);
            Progress.CurrentMission = null;
            ClearRunSave();
            // A weapon found on the job comes home with the player.
            Progress.Hand = Run.Weapon;
            Progress.Pocket = Run.Holster;
            if (first && Tuning.UseMoney && Tuning.MissionsPay) Progress.Cash += mission.Reward;
            Run.Cash = Progress.Cash;
            SaveProgress();
        }
        // The save is a JSON file beside the game's other data; a save from before it, kept in PlayerPrefs, is read once.
        private static string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

        private ProgressSVO LoadProgress()
        {
            string saved = null;
            try
            {
                if (System.IO.File.Exists(SavePath)) saved = System.IO.File.ReadAllText(SavePath);
            }
            catch (System.Exception exception)
            {
                FlowLogger.LogWarning("The save file could not be read: " + exception.Message);
            }
            if (string.IsNullOrEmpty(saved)) saved = PlayerPrefs.GetString(ProgressKey, "");
            ProgressSVO progress = string.IsNullOrEmpty(saved) ? null : JsonUtility.FromJson<ProgressSVO>(saved);
            progress ??= new ProgressSVO();
            progress.Completed ??= new System.Collections.Generic.List<string>();
            progress.Upgrades ??= new System.Collections.Generic.List<UpgradeLevelSVO>();
            progress.Locker ??= new System.Collections.Generic.List<string>();
            // Saves from before the locker kept one of each lose their duplicates.
            progress.Locker = new System.Collections.Generic.List<string>(new System.Collections.Generic.HashSet<string>(progress.Locker));
            progress.OwnedOutfits ??= new System.Collections.Generic.List<string>();
            progress.MissionGuards ??= new System.Collections.Generic.List<MissionGuardSVO>();
            progress.WorkingLocationPayments ??= new System.Collections.Generic.List<WorkingLocationPaymentSVO>();
            progress.WorkingLocations ??= new System.Collections.Generic.List<WorkingLocationSVO>();
            return progress;
        }

        private static string RunPath => System.IO.Path.Combine(Application.persistentDataPath, "run.json");
        public bool RestoreNextRun { get; set; }
        public float RunSaveTimer { get; set; }

        public void SaveRun()
        {
            RunSaveTimer = 0f;
            if (Run == null || !Run.Active || Run.InHideout || Run.IsLocationDefense || !Run.Alive || Run.Won || string.IsNullOrEmpty(Mission)) return;
            try
            {
                System.IO.File.WriteAllText(RunPath, JsonUtility.ToJson(new RunSaveSVO {Mission = Mission, Run = Run}));
            }
            catch (System.Exception exception)
            {
                FlowLogger.LogError("The contract could not be saved: " + exception.Message);
            }
        }

        public bool RestoreRun()
        {
            try
            {
                if (!System.IO.File.Exists(RunPath)) return false;
                RunSaveSVO saved = JsonUtility.FromJson<RunSaveSVO>(System.IO.File.ReadAllText(RunPath));
                if (saved?.Run == null || saved.Mission != Mission || !saved.Run.Alive || saved.Run.Won) return false;
                ContractRVO run = saved.Run;
                // Runs saved before location takeovers had documents and no owner to talk to. Keep the fresh Reset
                // result instead of restoring a contract that can no longer satisfy its objective.
                bool hasBoss = false;
                bool hasFamily = false;
                foreach (GuardRVO guard in run.Guards)
                {
                    hasBoss |= guard.Boss;
                    hasFamily |= !string.IsNullOrEmpty(guard.Family);
                }
                if (Missions != null && Missions.Find(Mission) != null && (!run.LocationTakeover || !hasBoss || !hasFamily)) return false;
                // Back in exactly as it was, minus what only lasts a moment.
                run.Active = true;
                run.Paused = false;
                run.Call = -1;
                run.Menu = -1;
                run.Terminal = -1;
                run.DeckOpen = false;
                run.IntroRemaining = 0f;
                run.CameraZoom = PlayerZoom > 0f ? PlayerZoom : Tuning.CameraZoom;
                Run = run;
                return true;
            }
            catch (System.Exception exception)
            {
                FlowLogger.LogWarning("The saved contract could not be read: " + exception.Message);
                return false;
            }
        }

        public void ClearRunSave()
        {
            try
            {
                if (System.IO.File.Exists(RunPath)) System.IO.File.Delete(RunPath);
            }
            catch (System.Exception exception)
            {
                FlowLogger.LogWarning("The saved contract could not be removed: " + exception.Message);
            }
        }

        public bool DamagePlayer(int amount, string cause)
        {
            if (!Run.Alive || amount <= 0) return false;
            Run.Health -= amount;
            if (Run.Health > 0) return false;
            Run.Alive = false;
            Run.Hint = cause;
            return true;
        }

        public bool DamageGuard(GuardRVO guard, int amount)
        {
            if (!guard.Alive || amount <= 0) return false;
            guard.Health -= amount;
            if (guard.Health <= 0)
            {
                guard.Alive = false;
                return true;
            }
            // A hit that does not kill still knocks them off their feet for a moment.
            guard.StunRemaining = Mathf.Max(guard.StunRemaining, 0.35f);
            return false;
        }

        public void SaveProgress()
        {
            try
            {
                System.IO.File.WriteAllText(SavePath, JsonUtility.ToJson(Progress, true));
            }
            catch (System.Exception exception)
            {
                FlowLogger.LogError("The save file could not be written: " + exception.Message);
            }
        }
        // The sum of an upgrade kind's bought levels times each upgrade's Amount.
        private float Bonus(UpgradeKind kind)
        {
            float total = 0f;
            if (Upgrades == null) return total;
            foreach (UpgradeCVO upgrade in Upgrades.Upgrades)
                if (upgrade.Kind == kind) total += upgrade.Amount * UpgradeLevel(upgrade.Id);
            return total;
        }
        public WeaponCVO Weapon(string id) => Weapons != null ? Weapons.Find(id) : null;
        public void Drop(Vector2 position, string weapon, int ammo, int reserve)
        {
            if (Weapon(weapon) == null) return;
            Run.Pickups.Add(new PickupRVO {Id = ++Run.PickupCount, Position = position, Weapon = weapon, Ammo = ammo, Reserve = reserve});
        }
        public void SetLevel(ContractLevelVO level) => Level = level ?? new ContractLevelVO();
        public void Reset()
        {
            ContractLevelVO level = Level;
            bool isDefense = !string.IsNullOrEmpty(Territory.DefenseMission) && Territory.DefenseMission == Mission;
            MissionCVO activeMission = Missions != null ? Missions.Find(Mission) : null;
            GuardSpawnVO[] spawns = !isDefense && activeMission != null ? LocationCast(level, activeMission) : level.Guards;
            CD_Family activeFamily = activeMission != null && Families != null ? Families.Find(activeMission.OwnerFamily) : null;
            var guards = new GuardRVO[spawns.Length];
            int armedIndex = 0;
            for (int i = 0; i < guards.Length; i++)
            {
                GuardSpawnVO spawn = spawns[i];
                FamilyRank rank = spawn.Boss ? FamilyRank.Don : spawn.Civilian ? FamilyRank.Associate
                    : armedIndex++ == 0 ? FamilyRank.Consigliere : armedIndex % 4 == 0 ? FamilyRank.Caporegime : FamilyRank.Soldier;
                FamilyRoleCVO role = activeFamily != null ? activeFamily.Role(rank) : null;
                CharacterTypeCVO type = CharacterType(role != null && !string.IsNullOrEmpty(role.CharacterType) ? role.CharacterType : spawn.Type);
                guards[i] = new GuardRVO
                {
                    Position = spawn.Position, Left = spawn.Left, Right = spawn.Right, Facing = spawn.Facing >= 0 ? 1 : -1,
                    Type = type.Id, Faction = type.Faction, Weapon = string.IsNullOrEmpty(spawn.Weapon) ? DrawWeapon(type) : spawn.Weapon,
                    SuspicionRate = type.SuspicionRate, SuspicionDecay = type.SuspicionDecay, CrouchSight = type.CrouchSight,
                    DarkSight = type.DarkSight, CloseSense = type.CloseSense, Hearing = type.Hearing, ChaseMemory = type.ChaseMemory,
                    InvestigateTime = type.InvestigateTime, Curiosity = type.Curiosity, FirePace = type.FirePace, DropChance = type.DropChance,
                    Health = type.Health, WalkSpeed = type.WalkSpeed, AlertSpeed = type.AlertSpeed,
                    SightRange = type.SightRange, ReactionTime = type.ReactionTime,
                    UsesCover = type.UsesCover, CoverSearch = type.CoverSearch, PeekTime = type.PeekTime, HideTime = type.HideTime,
                    RaisesAlarm = type.RaisesAlarm, AlarmRadius = type.AlarmRadius, CashDrop = type.CashDrop,
                    Civilian = spawn.Civilian || spawn.Boss,
                    Boss = spawn.Boss,
                    Family = activeFamily != null ? activeFamily.Id : null,
                    FamilyRank = rank,
                    Name = FamilyMemberName(activeFamily, rank),
                    WorkTimer = i * 0.37f,
                };
                if (guards[i].Civilian)
                {
                    guards[i].Faction = Faction.Friendly;
                    guards[i].Weapon = "";
                    guards[i].AlertSpeed = Mathf.Max(guards[i].AlertSpeed, 2.6f);
                    guards[i].WalkSpeed = Mathf.Max(guards[i].WalkSpeed, 0.75f);
                    guards[i].CashDrop = Vector2Int.zero;
                    guards[i].DropChance = 0f;
                    guards[i].UsesCover = false;
                    guards[i].RaisesAlarm = false;
                }
                ApplyDifficulty(guards[i], level.Difficulty, spawn);
            }
            if (isDefense) guards = PrepareLocationDefense(guards, level);
            var doors = new DoorRVO[level.Doors.Length];
            for (int i = 0; i < doors.Length; i++) doors[i] = new DoorRVO {Position = level.Doors[i]};
            var lights = new LightRVO[level.Lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                LightRVO light = level.Lights[i];
                lights[i] = new LightRVO {Floor = light.Floor, Left = light.Left, Right = light.Right, Switch = light.Switch, On = light.On};
            }
            var vents = new VentRVO[level.Vents.Length];
            for (int i = 0; i < vents.Length; i++) vents[i] = new VentRVO {Position = level.Vents[i].Position, Link = level.Vents[i].Link};
            var stairs = new StairRVO[level.Stairs.Length];
            for (int i = 0; i < stairs.Length; i++) stairs[i] = new StairRVO {Position = level.Stairs[i].Position, Link = level.Stairs[i].Link};
            var terminals = new TerminalRVO[level.Terminals.Length];
            for (int i = 0; i < terminals.Length; i++)
                terminals[i] = new TerminalRVO {Position = level.Terminals[i].Position, Title = level.Terminals[i].Title, Text = level.Terminals[i].Text};
            Run = new ContractRVO
            {
                Active = true,
                FloorHeight = level.FloorHeight,
                HalfWidth = level.HalfWidth,
                ExitX = level.ExitX,
                Player = level.PlayerSpawn,
                Guards = guards,
                Doors = doors,
                Lights = lights,
                Vents = vents,
                Covers = (Vector2[]) level.Covers.Clone(),
                Stairs = stairs,
                Terminals = terminals,
                HasDocument = false,
                Document = level.Document,
                LocationTakeover = !isDefense && activeMission != null,
                IsLocationDefense = isDefense,
            };
            CharacterTypeCVO player = CharacterType(Characters != null ? Characters.PlayerType : null);
            Run.PlayerType = player.Id;
            Run.Health = player.Health + Mathf.RoundToInt(Bonus(UpgradeKind.Health));
            Run.ReloadScale = Mathf.Max(0.2f, 1f - Bonus(UpgradeKind.ReloadSpeed) / 100f);
            Run.MoveScale = 1f + Bonus(UpgradeKind.MoveSpeed) / 100f;
            Run.CameraZoom = PlayerZoom > 0f ? PlayerZoom : Tuning.CameraZoom;
            Run.InHideout = Missions != null && Missions.Find(Mission) == null;
            string currentMission = Run.InHideout || isDefense ? null : Mission;
            if (Progress.CurrentMission != currentMission)
            {
                Progress.CurrentMission = currentMission;
                SaveProgress();
                if (Run.InHideout) ClearRunSave();
            }
            Run.Cash = Progress.Cash;
            var stations = new StationRVO[level.Stations.Length];
            for (int i = 0; i < stations.Length; i++) stations[i] = new StationRVO {Position = level.Stations[i].Position, Kind = level.Stations[i].Kind};
            Run.Stations = stations;
            var props = new PropRVO[level.Props.Length];
            for (int i = 0; i < props.Length; i++)
                props[i] = new PropRVO {Position = level.Props[i].Position, Kind = level.Props[i].Kind, Hit = level.Props[i].Hit, Size = level.Props[i].Size};
            Run.Props = props;
            Run.Outfit = Outfits.Find(Progress.Outfit) != null ? Progress.Outfit : Outfits.Default;
            // The run starts with what the player carried out last time, else the starting weapon.
            WeaponCVO start = Weapon(Progress.Hand) ?? Weapon(Weapons != null ? Weapons.PlayerStartWeapon : null) ?? Weapon(player.Weapon);
            WeaponCVO pocket = Weapon(Progress.Pocket);
            if (pocket != null && (start == null || pocket.Id != start.Id))
            {
                Run.Holster = pocket.Id;
                Run.HolsterAmmo = pocket.Magazine;
                Run.HolsterReserve = pocket.Reserve;
            }
            Run.Weapon = start != null ? start.Id : null;
            Run.Ammo = start != null ? start.Magazine : 0;
            Run.Reserve = start != null ? start.Reserve + Mathf.RoundToInt(Bonus(UpgradeKind.Ammo)) : 0;
            foreach (WeaponSpawnVO spawn in level.Weapons)
            {
                WeaponCVO weapon = Weapon(spawn.Weapon);
                if (weapon != null) Drop(spawn.Position, weapon.Id, weapon.Magazine, weapon.Reserve);
            }
            Input = new ContractInputVO {Aim = Run.Aim};
        }

        // A raid: the attacking family's men across the building - more and harder the richer the shop - and the
        // player's posted men by the door, of their own type and weaker than the player.
        private GuardRVO[] PrepareLocationDefense(GuardRVO[] authored, ContractLevelVO level)
        {
            RaidVO raid = Territory.RaidOn(Mission);
            CD_Family attackingFamily = Families != null ? Families.Find(raid.Family) : null;
            var enemies = new System.Collections.Generic.List<GuardRVO>();
            foreach (GuardRVO guard in authored)
                if (guard.Faction == Faction.Enemy && !guard.Civilian) enemies.Add(guard);
            if (enemies.Count == 0) enemies.Add(DefenseGuard(CharacterType(null), level.PlayerSpawn + Vector2.right * 5f, Faction.Enemy));
            var defense = new System.Collections.Generic.List<GuardRVO>();
            int attackers = Mathf.Max(1, raid.Attackers);
            for (int i = 0; i < attackers; i++)
            {
                GuardRVO source = enemies[i % enemies.Count];
                GuardRVO enemy = i < enemies.Count ? source : CopyDefenseGuard(source);
                enemy.Faction = Faction.Enemy;
                enemy.Civilian = false;
                enemy.Boss = false;
                enemy.Family = attackingFamily != null ? attackingFamily.Id : raid.Family;
                enemy.FamilyRank = i == 0 ? FamilyRank.Caporegime : FamilyRank.Soldier;
                FamilyRoleCVO role = attackingFamily != null ? attackingFamily.Role(enemy.FamilyRank) : null;
                if (role != null && !string.IsNullOrEmpty(role.CharacterType) && Characters.Find(role.CharacterType) != null) enemy.Type = role.CharacterType;
                enemy.Name = FamilyMemberName(attackingFamily, enemy.FamilyRank);
                enemy.Health += raid.Toughness;
                enemy.ReactionTime /= raid.Aggression;
                enemy.FireRateScale /= raid.Aggression;
                enemy.SuspicionRate *= raid.Aggression;
                // They come in hunting: they know someone is there.
                enemy.Suspicion = 1f;
                enemy.Alerted = true;
                enemy.Chase = enemy.ChaseMemory;
                enemy.LastSeen = level.PlayerSpawn;
                enemy.Position = new Vector2(Mathf.Lerp(-level.HalfWidth * 0.2f, level.HalfWidth - 2f, (i + 1f) / (attackers + 1f)), source.Position.y);
                enemy.Left = enemy.Position.x - 2f;
                enemy.Right = enemy.Position.x + 2f;
                defense.Add(enemy);
            }
            CD_Territory config = Territory.Config;
            CharacterTypeCVO defenderType = Characters.Find(config.DefenderType) ?? CharacterType(null);
            CD_Family playerFamily = Families != null ? Families.PlayerFamily : null;
            for (int i = 0; i < raid.Defenders; i++)
            {
                Vector2 position = level.PlayerSpawn + Vector2.right * (2f + i * 1.1f);
                GuardRVO friendly = DefenseGuard(defenderType, position, Faction.Friendly);
                friendly.Family = playerFamily != null ? playerFamily.Id : null;
                friendly.FamilyRank = i == 0 ? FamilyRank.Caporegime : FamilyRank.Soldier;
                friendly.Name = FamilyMemberName(playerFamily, friendly.FamilyRank);
                // Posted men are not the player: fewer hits, slower hands, shorter sight, slower bullets.
                friendly.Health = Mathf.Max(1, config.DefenderHealth);
                friendly.FirePace *= config.DefenderFirePace;
                friendly.ReactionTime *= config.DefenderReaction;
                friendly.SightRange *= config.DefenderSight;
                friendly.BulletSpeedScale *= config.DefenderBulletSpeed;
                defense.Add(friendly);
            }
            return defense.ToArray();
        }

        private GuardRVO DefenseGuard(CharacterTypeCVO type, Vector2 position, Faction faction)
        {
            string weapon = DrawWeapon(type);
            if (Weapon(weapon) == null) weapon = Weapons != null ? Weapons.PlayerStartWeapon : null;
            return new GuardRVO
            {
                Position = position, Left = position.x - 2f, Right = position.x + 2f, Type = type.Id, Faction = faction,
                Weapon = weapon, Health = Mathf.Max(1, type.Health), WalkSpeed = type.WalkSpeed, AlertSpeed = type.AlertSpeed,
                SightRange = type.SightRange, ReactionTime = type.ReactionTime, SuspicionRate = type.SuspicionRate,
                SuspicionDecay = type.SuspicionDecay, CrouchSight = type.CrouchSight, DarkSight = type.DarkSight,
                CloseSense = type.CloseSense, Hearing = type.Hearing, ChaseMemory = type.ChaseMemory,
                InvestigateTime = type.InvestigateTime, Curiosity = type.Curiosity, FirePace = type.FirePace,
                BulletSpeedScale = Mathf.Max(0.5f, type.BulletSpeed), DropChance = 0f,
            };
        }

        private GuardRVO CopyDefenseGuard(GuardRVO source) => new()
        {
            Position = source.Position, Left = source.Left, Right = source.Right, Facing = source.Facing, Type = source.Type,
            Faction = source.Faction, Weapon = source.Weapon, Health = source.Health, WalkSpeed = source.WalkSpeed,
            AlertSpeed = source.AlertSpeed, SightRange = source.SightRange, ReactionTime = source.ReactionTime,
            FireRateScale = source.FireRateScale, BulletSpeedScale = source.BulletSpeedScale, SuspicionRate = source.SuspicionRate,
            SuspicionDecay = source.SuspicionDecay, CrouchSight = source.CrouchSight, DarkSight = source.DarkSight,
            CloseSense = source.CloseSense, Hearing = source.Hearing, ChaseMemory = source.ChaseMemory,
            InvestigateTime = source.InvestigateTime, Curiosity = source.Curiosity, FirePace = source.FirePace,
            UsesCover = source.UsesCover, CoverSearch = source.CoverSearch, PeekTime = source.PeekTime,
            HideTime = source.HideTime, RaisesAlarm = source.RaisesAlarm, AlarmRadius = source.AlarmRadius,
        };

        private GuardSpawnVO[] LocationCast(ContractLevelVO level, MissionCVO mission)
        {
            var cast = new System.Collections.Generic.List<GuardSpawnVO>(level.Guards);
            bool hasBoss = false;
            int civilians = 0;
            foreach (GuardSpawnVO spawn in cast) hasBoss |= spawn.Boss;
            foreach (GuardSpawnVO spawn in cast) if (spawn.Civilian && !spawn.Boss) civilians++;
            float upperFloor = 0f;
            foreach (GuardSpawnVO spawn in level.Guards) upperFloor = Mathf.Max(upperFloor, spawn.Position.y);
            int needed = Mathf.Max(1, mission.CivilianCount) - civilians;
            for (int i = 0; i < needed; i++)
            {
                float t = (i + 1f) / (needed + 1f);
                float x = Mathf.Lerp(-level.HalfWidth + 3f, level.HalfWidth - 5f, t);
                float y = upperFloor > 0f && i % 2 == 1 ? upperFloor : 0f;
                cast.Add(new GuardSpawnVO {Position = new Vector2(x, y), Left = x - 0.8f, Right = x + 0.8f,
                    Facing = i % 2 == 0 ? 1 : -1, Type = "circle", Civilian = true});
            }
            if (!hasBoss)
            {
                float x = level.HalfWidth - 2.5f;
                cast.Add(new GuardSpawnVO {Position = new Vector2(x, upperFloor), Left = x - 0.7f, Right = x + 0.7f,
                    Facing = -1, Type = "circle", Civilian = true, Boss = true});
            }
            return cast.ToArray();
        }
        // Health added; reaction, sight and fire rate scaled; guard bullets sped up - by the level, then by the spawn.
        private void ApplyDifficulty(GuardRVO guard, LevelDifficulty difficulty, GuardSpawnVO spawn)
        {
            (int health, float reaction, float sight, float fire, float bullet, float speed) = difficulty switch
            {
                LevelDifficulty.Easy => (0, 1.4f, 0.8f, 1.3f, 0.85f, 0.9f),
                LevelDifficulty.Hard => (0, 0.75f, 1.15f, 0.85f, 1.05f, 1.1f),
                LevelDifficulty.Brutal => (1, 0.55f, 1.35f, 0.7f, 1.15f, 1.2f),
                _ => (0, 1f, 1f, 1f, 1f, 1f),
            };
            float aggression = Mathf.Max(0.1f, spawn.Aggression);
            guard.Health += health + Mathf.Max(0, spawn.Toughness);
            guard.ReactionTime *= reaction / aggression;
            guard.SightRange *= sight;
            guard.FireRateScale = fire / aggression;
            guard.BulletSpeedScale = bullet * CharacterType(guard.Type).BulletSpeed;
            guard.SuspicionRate *= aggression / reaction;
            guard.AlertSpeed *= speed;
        }
        // A weapon from the type's pool by weight; the type's single Weapon when the pool is empty.
        private string DrawWeapon(CharacterTypeCVO type)
        {
            float total = 0f;
            foreach (WeaponChanceCVO chance in type.WeaponPool) total += Mathf.Max(0f, chance.Weight);
            if (total <= 0f) return type.Weapon;
            float roll = Random.value * total;
            foreach (WeaponChanceCVO chance in type.WeaponPool)
            {
                roll -= Mathf.Max(0f, chance.Weight);
                if (roll <= 0f) return chance.Weapon;
            }
            return type.WeaponPool[type.WeaponPool.Count - 1].Weapon;
        }
        // An Id the cast does not list falls back to its first entry, so a typo in a marker still spawns someone.
        private CharacterTypeCVO CharacterType(string id)
        {
            if (Characters == null) PostConstruct();
            CharacterTypeCVO type = Characters.Find(id);
            if (type != null) return type;
            CharacterTypeCVO fallback = Characters.Types.Count > 0 ? Characters.Types[0] : new CharacterTypeCVO();
            if (!string.IsNullOrEmpty(id)) FlowLogger.LogWarning($"CD_Characters lists no character '{id}'; using '{fallback.Id}'.");
            return fallback;
        }

        private string FamilyMemberName(CD_Family family, FamilyRank rank)
        {
            FamilyRoleCVO role = family != null ? family.Role(rank) : null;
            if (role == null || role.Names == null || role.Names.Count == 0) return rank.ToString();
            return role.Names[UnityEngine.Random.Range(0, role.Names.Count)];
        }

        private CD_Families DefaultFamilies()
        {
            var rival = ScriptableObject.CreateInstance<CD_Family>();
            rival.Id = "rival";
            rival.DisplayName = "TRIANGLE FAMILY";
            rival.RelationToPlayer = FamilyRelation.Enemy;
            var player = ScriptableObject.CreateInstance<CD_Family>();
            player.Id = "player";
            player.DisplayName = "SQUARE FAMILY";
            player.RelationToPlayer = FamilyRelation.Player;
            var families = ScriptableObject.CreateInstance<CD_Families>();
            families.PlayerFamily = player;
            families.Families.Add(player);
            families.Families.Add(rival);
            return families;
        }
        public void Capture(ContractInputVO input)
        {
            input.Jump |= Input.Jump;
            input.Fire |= Input.Fire;
            input.Melee |= Input.Melee;
            input.Interact |= Input.Interact;
            input.Reload |= Input.Reload;
            input.Swap |= Input.Swap;
            input.Pause |= Input.Pause;
            Input = input;
        }
        public void ClearEdges()
        {
            ContractInputVO input = Input;
            input.Jump = input.Fire = input.Melee = input.Interact = input.Reload = input.Swap = input.Pause = false;
            Input = input;
        }
        // What is typed into a terminal outlives the run: a retry starts with the note still on the screen.
        public void WriteTerminal(int index, string text)
        {
            if (index < 0 || index >= Run.Terminals.Length) return;
            Run.Terminals[index].Text = text ?? string.Empty;
            if (index < Level.Terminals.Length) Level.Terminals[index].Text = Run.Terminals[index].Text;
        }
        public LightRVO LightAt(Vector2 position)
        {
            foreach (LightRVO light in Run.Lights)
                if (Mathf.Abs(position.y - light.Floor) < 1.8f && position.x >= light.Left && position.x < light.Right)
                    return light;
            return null;
        }
    }
}
