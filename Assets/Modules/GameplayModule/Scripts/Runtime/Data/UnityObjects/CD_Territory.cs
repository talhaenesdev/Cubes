using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // Everything about holding the city: what working shops pay, how often the other families come for them, how
    // hard they hit, who the player leaves behind to guard them, what happens while the game is closed, and what the
    // owner says when they are talked round.
    [CreateAssetMenu(fileName = "CD_Territory", menuName = "Night Shift/CD_Territory")]
    public class CD_Territory : ScriptableObject
    {
        [Header("Income")]
        [Tooltip("Shops pay their CD_Missions WorkPayment every WorkPaymentInterval, in real time, open or closed.")]
        public bool LocationsPay = true;

        [Header("Guards")]
        [Tooltip("Most men the player can post at one shop.")]
        [Min(1)] public int MaxGuardsPerLocation = 10;
        [Tooltip("Cash paid once when one man is posted at a shop. Removing him gives no refund.")]
        [Min(0)] public int GuardHirePrice = 1000;
        [Tooltip("Cash one posted man takes from that shop every WorkPaymentInterval. Net income never goes below zero.")]
        [Min(0)] public int GuardUpkeepPerPayment = 20;
        [Tooltip("The character type posted men use - not the player's own, and weaker than the enemy's soldiers.")]
        public string DefenderType = "square";
        [Tooltip("Hits a posted man takes. The enemy's soldiers keep their own.")]
        [Min(1)] public int DefenderHealth = 1;
        [Tooltip("Multiplies the seconds between a posted man's shots; above 1 is slower.")]
        [Min(0.2f)] public float DefenderFirePace = 1.6f;
        [Tooltip("Multiplies the seconds before a posted man reacts; above 1 is slower.")]
        [Min(0.2f)] public float DefenderReaction = 1.5f;
        [Tooltip("Multiplies a posted man's sight range.")]
        [Min(0.2f)] public float DefenderSight = 0.85f;
        [Tooltip("Multiplies his bullets' speed.")]
        [Min(0.2f)] public float DefenderBulletSpeed = 0.8f;

        [Header("Raids while playing")]
        [Tooltip("Shops can be raided while the game is open.")]
        public bool Raids = true;
        [Tooltip("Real-time minutes between each shop's raid rolls.")]
        [Min(0.1f)] public float RaidRollMinutes = 8f;
        [Tooltip("Real-time minutes the player has to go and help before the raid is settled without them.")]
        [Min(0.1f)] public float RaidResponseMinutes = 5f;
        [Tooltip("How much a shop's income raises its raid chance: the richest shop rolls its AttackChance times (1 + this).")]
        [Min(0f)] public float RichShopRaidBonus = 1f;

        [Header("Raid strength")]
        [Tooltip("Attackers at the poorest shop, and at the richest.")]
        [Min(1)] public int MinAttackers = 3;
        [Min(1)] public int MaxAttackers = 10;
        [Tooltip("Attacker aggression at the poorest and the richest shop (1 is their type's own pace).")]
        public Vector2 AttackerAggression = new(1f, 1.6f);
        [Tooltip("Extra hits each attacker takes at the richest shop; scales down to 0 at the poorest.")]
        [Min(0)] public int RichShopToughness = 2;

        [Header("A raid nobody answers")]
        [Tooltip("How much one posted man counts against one attacker when the player does not come. Below 1: they are weaker.")]
        [Min(0f)] public float GuardStrength = 0.6f;
        [Tooltip("Share of the posted men lost when they hold a raid on their own.")]
        [Range(0f, 1f)] public float GuardLosses = 0.4f;

        [Header("While the game is closed")]
        [Tooltip("Hours away after which one shop is lost, and another every such period after.")]
        [Min(1f)] public float HoursPerLostShop = 24f;
        [Tooltip("Shops with more posted men are less likely to be the one lost: each man counts this much against it.")]
        [Min(0f)] public float GuardKeepWeight = 0.5f;

        [Header("Talking the owner round")]
        [Tooltip("The conversation with a shop's owner once their men are down, in bubbles. {0} is the family they paid until now.")]
        public List<DialogueLineCVO> Persuasion = new()
        {
            new() {Speaker = DialogueSpeaker.Boss, Text = "Wait - don't shoot! What do you want?"},
            new() {Speaker = DialogueSpeaker.Player, Text = "Your men are on the floor. This place pays me now."},
            new() {Speaker = DialogueSpeaker.Boss, Text = "The {0} will kill me."},
            new() {Speaker = DialogueSpeaker.Player, Text = "They couldn't protect you tonight. I can."},
            new() {Speaker = DialogueSpeaker.Player, Text = "A cut of every take. Miss one and I come back."},
            new() {Speaker = DialogueSpeaker.Boss, Text = "...All right. We work for you."},
        };
        [Tooltip("Seconds a bubble stays before the next one comes on its own.")]
        [Min(1f)] public float BubbleTime = 4.5f;
    }
}
