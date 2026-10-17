#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Modules.GameplayModule.Data.UnityObjects;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The look every Night Shift config shares: a banner, coloured cards that fold, sections with a tinted rule,
    // and stat bars that compare one entry with the rest.
    public abstract class ConfigInspector : UnityEditor.Editor
    {
        protected static readonly Color Gold = new(0.85f, 0.68f, 0.3f);
        protected static readonly Color Crimson = new(0.85f, 0.2f, 0.25f);
        protected static readonly Color Teal = new(0.3f, 0.75f, 0.7f);
        protected static readonly Color Violet = new(0.6f, 0.45f, 0.9f);
        protected static readonly Color Sky = new(0.35f, 0.6f, 0.95f);
        protected static readonly Color Leaf = new(0.4f, 0.8f, 0.45f);
        protected static readonly Color Ember = new(0.95f, 0.55f, 0.2f);
        protected static readonly Color Slate = new(0.55f, 0.58f, 0.65f);

        private GUIStyle _title;
        private GUIStyle _subtitle;
        private GUIStyle _cardTitle;
        private GUIStyle _small;

        protected void Styles()
        {
            if (_title != null) return;
            _title = new GUIStyle(EditorStyles.boldLabel) {fontSize = 16, normal = {textColor = Color.white}};
            _subtitle = new GUIStyle(EditorStyles.miniLabel) {wordWrap = true, normal = {textColor = new Color(1f, 1f, 1f, 0.75f)}};
            _cardTitle = new GUIStyle(EditorStyles.boldLabel) {fontSize = 12, normal = {textColor = Color.white}};
            _small = new GUIStyle(EditorStyles.miniLabel) {normal = {textColor = new Color(1f, 1f, 1f, 0.8f)}};
        }

        protected void Banner(string title, string subtitle, Color color)
        {
            Styles();
            Rect rect = GUILayoutUtility.GetRect(0f, 46f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, color * 0.55f);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 3f, rect.width, 3f), color);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 4f, rect.width - 20f, 22f), title, _title);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 24f, rect.width - 20f, 20f), subtitle, _subtitle);
            GUILayout.Space(6f);
        }

        // A card that folds: a coloured header with a title and a one-line summary; the body draws when open.
        protected bool Card(string key, string title, string summary, Color color, Action<Rect> headerExtras = null)
        {
            Styles();
            bool open = SessionState.GetBool(key, false);
            Rect header = GUILayoutUtility.GetRect(0f, 26f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(header, color * 0.45f);
            EditorGUI.DrawRect(new Rect(header.x, header.y, 4f, header.height), color);
            GUI.Label(new Rect(header.x + 22f, header.y + 4f, 220f, 18f), title, _cardTitle);
            GUI.Label(new Rect(header.x + 230f, header.y + 6f, header.width - 330f, 16f), summary, _small);
            bool now = EditorGUI.Foldout(new Rect(header.x + 8f, header.y + 4f, 14f, 18f), open, GUIContent.none, true);
            headerExtras?.Invoke(header);
            if (Event.current.type == EventType.MouseDown && header.Contains(Event.current.mousePosition) && Event.current.mousePosition.x < header.xMax - 100f)
            {
                now = !open;
                Event.current.Use();
            }
            if (now != open) SessionState.SetBool(key, now);
            return now;
        }

        protected void Section(string title, Color color)
        {
            GUILayout.Space(4f);
            Rect rect = GUILayoutUtility.GetRect(0f, 18f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), color * 0.8f);
            GUI.Label(rect, title, new GUIStyle(EditorStyles.boldLabel) {normal = {textColor = color}});
        }

        // A bar of value against max, labelled, in the section's colour.
        protected void StatBar(string label, float value, float max, Color color, string text)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 16f, GUILayout.ExpandWidth(true));
            Rect labelRect = new(rect.x, rect.y, 110f, rect.height);
            Rect bar = new(rect.x + 112f, rect.y + 2f, rect.width - 180f, rect.height - 4f);
            GUI.Label(labelRect, label, EditorStyles.miniLabel);
            EditorGUI.DrawRect(bar, new Color(0f, 0f, 0f, 0.35f));
            EditorGUI.DrawRect(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(max <= 0f ? 0f : value / max), bar.height), color);
            GUI.Label(new Rect(bar.xMax + 6f, rect.y, 64f, rect.height), text, EditorStyles.miniLabel);
        }

        protected void Fields(SerializedProperty element, params string[] names)
        {
            foreach (string name in names)
            {
                SerializedProperty property = element.FindPropertyRelative(name);
                if (property != null) EditorGUILayout.PropertyField(property, true);
            }
        }

        protected void RootFields(params string[] names)
        {
            foreach (string name in names)
            {
                SerializedProperty property = serializedObject.FindProperty(name);
                if (property != null) EditorGUILayout.PropertyField(property, true);
            }
        }

        // Add, duplicate and remove for a list of cards, applied after the cards are drawn.
        protected void ListButtons(SerializedProperty list, ref int remove, ref int duplicate, string addLabel)
        {
            if (remove >= 0) { list.DeleteArrayElementAtIndex(remove); remove = -1; }
            if (duplicate >= 0) { list.InsertArrayElementAtIndex(duplicate); duplicate = -1; }
            GUILayout.Space(6f);
            if (GUILayout.Button(addLabel, GUILayout.Height(24f))) list.InsertArrayElementAtIndex(list.arraySize);
        }

        protected void CardButtons(Rect header, int index, ref int remove, ref int duplicate)
        {
            if (GUI.Button(new Rect(header.xMax - 96f, header.y + 4f, 44f, 18f), "Copy", EditorStyles.miniButton)) duplicate = index;
            if (GUI.Button(new Rect(header.xMax - 48f, header.y + 4f, 44f, 18f), "Delete", EditorStyles.miniButton)) remove = index;
        }
    }

    [CustomEditor(typeof(CD_Weapons))]
    public class WeaponsInspector : ConfigInspector
    {
        private int _remove = -1;
        private int _duplicate = -1;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Banner("ARSENAL", "Every weapon in the game. Characters carry them, they drop when their owner falls, the player picks them up.", Gold);
            var config = (CD_Weapons) target;
            RootFields("PlayerStartWeapon");
            SerializedProperty list = serializedObject.FindProperty("Weapons");
            float maxDamage = 1f, maxRate = 1f, maxMag = 1f, maxNoise = 1f, maxSpeed = 1f;
            foreach (var w in config.Weapons)
            {
                maxDamage = Mathf.Max(maxDamage, w.Damage * w.Pellets);
                maxRate = Mathf.Max(maxRate, 1f / Mathf.Max(0.02f, w.FireInterval));
                maxMag = Mathf.Max(maxMag, w.Magazine);
                maxNoise = Mathf.Max(maxNoise, w.NoiseRadius);
                maxSpeed = Mathf.Max(maxSpeed, w.BulletSpeed);
            }
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                var w = i < config.Weapons.Count ? config.Weapons[i] : null;
                if (w == null) continue;
                Color tint = w.Pellets > 1 ? Crimson : w.Automatic ? Ember : w.NoiseRadius < 4f ? Sky : w.Damage >= 3 ? Violet : Gold;
                string kind = w.Pellets > 1 ? "SPREAD" : w.Automatic ? "AUTOMATIC" : w.NoiseRadius < 4f ? "SILENT" : w.Damage >= 3 ? "HEAVY" : "STANDARD";
                int index = i;
                string summary = $"{kind}   dmg {w.Damage}x{w.Pellets}   mag {w.Magazine}   {1f / Mathf.Max(0.02f, w.FireInterval):0.#}/s   noise {w.NoiseRadius:0}";
                bool open = Card("ns.weapon." + i, w.DisplayName + "  (" + w.Id + ")", summary, tint, header => CardButtons(header, index, ref _remove, ref _duplicate));
                if (!open) continue;
                EditorGUI.indentLevel++;
                StatBar("Damage / shot", w.Damage * w.Pellets, maxDamage, Crimson, (w.Damage * w.Pellets).ToString());
                StatBar("Fire rate", 1f / Mathf.Max(0.02f, w.FireInterval), maxRate, Ember, $"{1f / Mathf.Max(0.02f, w.FireInterval):0.#}/s");
                StatBar("Magazine", w.Magazine, maxMag, Gold, w.Magazine.ToString());
                StatBar("Bullet speed", w.BulletSpeed, maxSpeed, Violet, w.BulletSpeed.ToString("0"));
                StatBar("Noise", w.NoiseRadius, maxNoise, Sky, w.NoiseRadius.ToString("0.#"));
                Section("Identity", Slate);
                Fields(element, "Id", "DisplayName", "Prefab");
                Section("Ammunition", Gold);
                Fields(element, "Magazine", "Reserve", "Reload", "ReloadTime");
                Section("Firing", Ember);
                Fields(element, "Automatic", "FireInterval", "Pellets", "Spread", "Damage");
                Section("Bullet", Violet);
                Fields(element, "BulletSpeed", "BulletLife");
                Section("Sound", Sky);
                Fields(element, "NoiseRadius");
                EditorGUI.indentLevel--;
                GUILayout.Space(6f);
            }
            ListButtons(list, ref _remove, ref _duplicate, "+  Add weapon");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(CD_Characters))]
    public class CharactersInspector : ConfigInspector
    {
        private int _remove = -1;
        private int _duplicate = -1;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Banner("CAST", "Every kind of character. Red cards hunt the player, green ones only patrol. Change Faction to switch sides.", Crimson);
            var config = (CD_Characters) target;
            RootFields("PlayerType");
            SerializedProperty list = serializedObject.FindProperty("Types");
            for (int i = 0; i < list.arraySize && i < config.Types.Count; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                var t = config.Types[i];
                bool enemy = t.Faction == Enums.Faction.Enemy;
                Color tint = t.Id == config.PlayerType ? Gold : enemy ? Crimson : Leaf;
                string side = t.Id == config.PlayerType ? "PLAYER" : enemy ? "ENEMY" : "FRIENDLY";
                int index = i;
                string summary = $"{side}   {t.Shape}   hp {t.Health}   sight {t.SightRange:0.#}   notice {t.SuspicionRate:0.#}/s   react {t.ReactionTime:0.##}s";
                bool open = Card("ns.character." + i, t.DisplayName + "  (" + t.Id + ")", summary, tint, header => CardButtons(header, index, ref _remove, ref _duplicate));
                if (!open) continue;
                EditorGUI.indentLevel++;
                StatBar("Health", t.Health, 5f, Crimson, t.Health.ToString());
                StatBar("Sight", t.SightRange, 10f, Sky, t.SightRange.ToString("0.#"));
                StatBar("Notices", t.SuspicionRate, 4f, Ember, $"{t.SuspicionRate:0.#}/s");
                StatBar("Hearing", t.Hearing, 2f, Violet, $"x{t.Hearing:0.#}");
                StatBar("Chase", t.ChaseMemory, 8f, Gold, $"{t.ChaseMemory:0.#}s");
                Section("Identity", Slate);
                Fields(element, "Id", "DisplayName", "Shape", "Faction", "Prefab");
                Section("Weapons", Gold);
                Fields(element, "Weapon", "WeaponPool", "DropChance");
                float total = 0f;
                foreach (var chance in t.WeaponPool) total += Mathf.Max(0f, chance.Weight);
                foreach (var chance in t.WeaponPool)
                    StatBar("  " + chance.Weapon, chance.Weight, total, Gold, total > 0f ? $"{chance.Weight / total:P0}" : "-");
                Section("Body", Leaf);
                Fields(element, "Health", "WalkSpeed", "AlertSpeed");
                Section("Senses  -  how fast it finds you", Sky);
                Fields(element, "SightRange", "CrouchSight", "DarkSight", "CloseSense", "Hearing", "Curiosity");
                Section("Temper  -  how fast it grows suspicious and how long it hunts", Ember);
                Fields(element, "SuspicionRate", "SuspicionDecay", "ReactionTime", "ChaseMemory", "InvestigateTime");
                Section("Shooting", Crimson);
                Fields(element, "BulletSpeed", "FirePace");
                Section("Loot", Gold);
                Fields(element, "CashDrop");
                Section("Tactics  -  cover, and warning the others", Teal);
                Fields(element, "UsesCover", "CoverSearch", "PeekTime", "HideTime", "RaisesAlarm", "AlarmRadius");
                EditorGUI.indentLevel--;
                GUILayout.Space(6f);
            }
            ListButtons(list, ref _remove, ref _duplicate, "+  Add character type");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(CD_Sounds))]
    public class SoundsInspector : ConfigInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Banner("SOUNDS", "Footsteps, light switches, lamp hum, the cat and the ambience. Drop your own clip in any slot; an empty slot is silent.", Sky);
            Group("Footsteps", Leaf, "Footsteps", "StepInterval", "CrouchStepInterval", "StepVolume", "CrouchStepVolume", "GuardStepVolume", "GuardHearRange");
            Group("Lights", Gold, "SwitchClick", "SwitchVolume", "LampHum", "HumVolume");
            Group("Cat", Violet, "CatMeows", "MeowVolume", "MeowInterval", "CatPurr", "PurrVolume", "PurrRange");
            Group("Cash", Gold, "CashPickup", "CashVolume");
            Group("Phone", Crimson, "PhoneRing", "RingVolume");
            Group("Ambience", Teal, "ContractAmbience", "HideoutAmbience", "AmbienceVolume");
            serializedObject.ApplyModifiedProperties();
        }

        private void Group(string title, Color color, params string[] names)
        {
            Section(title, color);
            EditorGUI.indentLevel++;
            RootFields(names);
            EditorGUI.indentLevel--;
        }
    }

    [CustomEditor(typeof(CD_Gameplay))]
    public class GameplayInspector : ConfigInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Banner("GAMEPLAY", "The numbers that belong to no single weapon or character: how the player moves, melee, doors, gunfire, props.", Teal);
            Group("Player movement", Leaf, "WalkSpeed", "CrouchSpeed", "JumpSpeed", "Gravity");
            Group("Melee", Crimson, "MeleeRange", "MeleeCooldown", "MeleeStun", "MeleeKnockback");
            Group("Doors", Gold, "KickReach", "KickStun", "KickNoise");
            Group("Gunfire  -  what guards make of it", Sky, "ShotNoiseTime", "ImpactCuriosityTime", "ImpactSuspicion");
            Group("Props", Ember, "BlastRadius", "ChainRadius", "CrushHalfWidth");
            Group("Money  -  enemies drop cash; upgrades cost it", Gold, "UseMoney", "MissionsPay", "OutfitsCost", "LootReach");
            Group("Damage  -  everything takes it off Health, nothing kills outright", Crimson, "BlastDamage", "CrushDamage", "MeleeDamage");
            Group("Contract", Violet, "ExitReach");
            Group("Camera  -  how big the game looks", Gold, "CameraZoom", "ZoomMin", "ZoomMax", "ZoomStep");
            serializedObject.ApplyModifiedProperties();
        }

        private void Group(string title, Color color, params string[] names)
        {
            Section(title, color);
            EditorGUI.indentLevel++;
            RootFields(names);
            EditorGUI.indentLevel--;
        }
    }

    [CustomEditor(typeof(CD_Families))]
    public class FamiliesInspector : ConfigInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Banner("FAMILIES", "Register every CD_Family here. Missions refer to a family's Id.", Crimson);
            RootFields("PlayerFamily", "Families");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(CD_Territory))]
    public class TerritoryInspector : ConfigInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Banner("TERRITORY", "Holding the city: shop income, the men posted there, raids live and while the game is closed, and talking an owner round.", Teal);
            Group("Income", Gold, "LocationsPay");
            Group("Our men at a shop  -  their own type, weaker than the player", Leaf, "MaxGuardsPerLocation", "DefenderType", "DefenderHealth",
                "DefenderFirePace", "DefenderReaction", "DefenderSight", "DefenderBulletSpeed");
            Group("Raids while playing", Ember, "Raids", "RaidRollMinutes", "RaidResponseMinutes", "RichShopRaidBonus");
            Group("Raid strength  -  richer shops are hit harder", Crimson, "MinAttackers", "MaxAttackers", "AttackerAggression", "RichShopToughness");
            Group("A raid nobody answers", Violet, "GuardStrength", "GuardLosses");
            Group("While the game is closed", Sky, "HoursPerLostShop", "GuardKeepWeight");
            Group("Talking the owner round", Gold, "Persuasion", "BubbleTime");
            serializedObject.ApplyModifiedProperties();
        }

        private void Group(string title, Color color, params string[] names)
        {
            Section(title, color);
            EditorGUI.indentLevel++;
            RootFields(names);
            EditorGUI.indentLevel--;
        }
    }

    [CustomEditor(typeof(CD_Family))]
    public class FamilyInspector : ConfigInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Banner("FAMILY", "Names, combat type and directly-instantiated prefab for every rank.", Ember);
            RootFields("Id", "DisplayName", "Color", "RelationToPlayer", "Relations", "Don", "Consigliere", "Caporegime", "Soldiers", "Associates");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(CD_Missions))]
    public class MissionsInspector : ConfigInspector
    {
        private int _remove = -1;
        private int _duplicate = -1;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Banner("CONTRACTS", "The hideout's mission board, easiest first. Each contract plays in its level and pays its reward once.", Violet);
            RootFields("Hideout");
            var config = (CD_Missions) target;
            SerializedProperty list = serializedObject.FindProperty("Missions");
            int top = 1;
            foreach (var m in config.Missions) top = Mathf.Max(top, m.Reward);
            for (int i = 0; i < list.arraySize && i < config.Missions.Count; i++)
            {
                var m = config.Missions[i];
                var level = m.Level;
                Color tint = level == null ? Slate : level.Difficulty switch
                {
                    Enums.LevelDifficulty.Easy => Leaf,
                    Enums.LevelDifficulty.Normal => Sky,
                    Enums.LevelDifficulty.Hard => Ember,
                    _ => Crimson,
                };
                int index = i;
                string summary = m.LocationDifficulty.ToString().ToUpperInvariant() + $"   ${m.Reward}   {m.AttackChance:0.#}% RAID   " + (level != null ? level.name : "NO LEVEL");
                bool open = Card("ns.mission." + i, (i + 1) + ".  " + m.Title, summary, tint, header => CardButtons(header, index, ref _remove, ref _duplicate));
                if (!open) continue;
                EditorGUI.indentLevel++;
                StatBar("Reward", m.Reward, top, Gold, "$" + m.Reward);
                Fields(list.GetArrayElementAtIndex(i), "Id", "Title", "Briefing", "Level", "Reward", "WorkPayment", "WorkPaymentInterval", "MapPosition",
                    "OwnerFamily", "CivilianCount", "LocationDifficulty", "AttackChance", "AttackingFamily", "AttackingFamilies", "Music");
                EditorGUI.indentLevel--;
                GUILayout.Space(6f);
            }
            ListButtons(list, ref _remove, ref _duplicate, "+  Add contract");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
