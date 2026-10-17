using System;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One bubble of a conversation: who says it and what, translated through CD_Localization.
    [Serializable]
    public class DialogueLineCVO
    {
        public DialogueSpeaker Speaker = DialogueSpeaker.Boss;
        [TextArea(1, 3)] public string Text = "";
    }

    // What happened to the territory while the game was closed, told once on the way home.
    public class TerritoryReportVO
    {
        public long AwayMilliseconds;
        public int Income;
        public System.Collections.Generic.List<string> Lost = new();
        public System.Collections.Generic.List<string> Held = new();
        public bool IsEmpty => Income <= 0 && Lost.Count == 0 && Held.Count == 0;
    }

    // A raid's force, worked out from how rich the shop is.
    public class RaidVO
    {
        public string Family;
        public int Attackers;
        public float Aggression = 1f;
        public int Toughness;
        public int Defenders;
    }
}
