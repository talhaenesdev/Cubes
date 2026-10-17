using System;
using System.Collections.Generic;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One contract on the hideout's mission board.
    [Serializable]
    public class MissionCVO
    {
        public string Id = "new_mission";
        public string Title = "NEW CONTRACT";
        [TextArea(2, 6)] public string Briefing = "";
        [Tooltip("The level this contract is played in.")]
        public LevelAuthoring Level;
        [Tooltip("Cash paid the first time the contract is completed.")]
        [Min(0)] public int Reward = 300;
        [Tooltip("Cash this location pays every WorkPaymentInterval seconds after it starts working for the player. Zero disables its recurring income.")]
        [Min(0)] public int WorkPayment = 30;
        [Tooltip("Real-time seconds between this working location's payments. Zero disables its recurring income.")]
        [Min(0f)] public float WorkPaymentInterval = 60f;
        [Header("City map")]
        [Tooltip("Where the shop stands on the city map, 0-1 across and up. (0, 0) lays it out in order.")]
        public Vector2 MapPosition;
        [Header("Location takeover")]
        [Tooltip("CD_Families Id of this location's owner before the player takes it.")]
        public string OwnerFamily = "rival";
        [Tooltip("Non-combatants added to this location beside its owner. The owner is added separately.")]
        [Min(1)] public int CivilianCount = 3;
        [Header("Working location defence")]
        [Tooltip("How strong the attack force is when this location is raided.")]
        public LocationDifficulty LocationDifficulty = LocationDifficulty.Easy;
        [Tooltip("Chance from 0 to 100 on every configured attack roll. Zero makes this location safe.")]
        [Range(0f, 100f)] public float AttackChance = 10f;
        [Tooltip("CD_Families Id of the family that raids and owns this location if it is lost.")]
        public string AttackingFamily = "rival";
        [Tooltip("Enemy family Ids allowed to raid this location. Empty lets every family marked Enemy attack it.")]
        public List<string> AttackingFamilies = new();
        [Tooltip("Track Id in CD_Music. Empty plays CD_Music's ContractTrack.")]
        public string Music = "";
    }
}
