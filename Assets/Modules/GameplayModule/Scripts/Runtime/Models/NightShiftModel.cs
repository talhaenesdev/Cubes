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
        public CD_Missions Missions { get; private set; }
        public CD_Upgrades Upgrades { get; private set; }
        public ProgressSVO Progress { get; private set; } = new();
        public string Mission { get; set; }
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
                Missions = adapter.GetScriptable<CD_Missions>();
                Upgrades = adapter.GetScriptable<CD_Upgrades>();
            }
            else FlowLogger.LogWarning("GameplaySystemRoot has no RootAdapter; the default weapons and characters are used.");
            if (Weapons == null) { Weapons = ScriptableObject.CreateInstance<CD_Weapons>(); Weapons.FillDefaults(); }
            if (Characters == null) { Characters = ScriptableObject.CreateInstance<CD_Characters>(); Characters.FillDefaults(); }
            if (Upgrades == null) { Upgrades = ScriptableObject.CreateInstance<CD_Upgrades>(); Upgrades.FillDefaults(); }
            string saved = PlayerPrefs.GetString(ProgressKey, "");
            Progress = string.IsNullOrEmpty(saved) ? new ProgressSVO() : JsonUtility.FromJson<ProgressSVO>(saved) ?? new ProgressSVO();
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
            int price = upgrade.Cost * (level + 1);
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
            MissionCVO mission = Missions != null ? Missions.Find(Mission) : null;
            if (mission == null || Progress.Completed.Contains(mission.Id)) return;
            Progress.Completed.Add(mission.Id);
            Progress.Cash += mission.Reward;
            Run.Cash = Progress.Cash;
            SaveProgress();
        }
        private void SaveProgress()
        {
            PlayerPrefs.SetString(ProgressKey, JsonUtility.ToJson(Progress));
            PlayerPrefs.Save();
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
            var guards = new GuardRVO[level.Guards.Length];
            for (int i = 0; i < guards.Length; i++)
            {
                GuardSpawnVO spawn = level.Guards[i];
                CharacterTypeCVO type = CharacterType(spawn.Type);
                guards[i] = new GuardRVO
                {
                    Position = spawn.Position, Left = spawn.Left, Right = spawn.Right, Facing = spawn.Facing >= 0 ? 1 : -1,
                    Type = type.Id, Faction = type.Faction, Weapon = string.IsNullOrEmpty(spawn.Weapon) ? type.Weapon : spawn.Weapon,
                    Health = type.Health, WalkSpeed = type.WalkSpeed, AlertSpeed = type.AlertSpeed,
                    SightRange = type.SightRange, ReactionTime = type.ReactionTime,
                };
                ApplyDifficulty(guards[i], level.Difficulty, spawn);
            }
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
            };
            CharacterTypeCVO player = CharacterType(Characters != null ? Characters.PlayerType : null);
            Run.PlayerType = player.Id;
            Run.Health = player.Health + Mathf.RoundToInt(Bonus(UpgradeKind.Health));
            Run.ReloadScale = Mathf.Max(0.2f, 1f - Bonus(UpgradeKind.ReloadSpeed) / 100f);
            Run.MoveScale = 1f + Bonus(UpgradeKind.MoveSpeed) / 100f;
            Run.InHideout = Missions != null && Missions.Find(Mission) == null;
            Run.Cash = Progress.Cash;
            var stations = new StationRVO[level.Stations.Length];
            for (int i = 0; i < stations.Length; i++) stations[i] = new StationRVO {Position = level.Stations[i].Position, Kind = level.Stations[i].Kind};
            Run.Stations = stations;
            var props = new PropRVO[level.Props.Length];
            for (int i = 0; i < props.Length; i++)
                props[i] = new PropRVO {Position = level.Props[i].Position, Kind = level.Props[i].Kind, Hit = level.Props[i].Hit, Size = level.Props[i].Size};
            Run.Props = props;
            WeaponCVO start = Weapon(Weapons != null ? Weapons.PlayerStartWeapon : null) ?? Weapon(player.Weapon);
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
            guard.BulletSpeedScale = bullet;
            guard.AlertSpeed *= speed;
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
