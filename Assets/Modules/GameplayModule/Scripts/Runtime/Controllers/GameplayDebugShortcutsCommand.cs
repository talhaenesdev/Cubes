#if UNITY_EDITOR
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modules.GameplayModule.Controllers
{
    internal class GameplayDebugShortcutsCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !ControlAndShift(keyboard)) return;
            if (keyboard.digit1Key.wasPressedThisFrame) KillEnemies();
            else if (keyboard.digit2Key.wasPressedThisFrame) KillPlayer();
            else if (keyboard.digit3Key.wasPressedThisFrame) WinMission();
            else if (keyboard.digit4Key.wasPressedThisFrame) AbandonMission();
            else if (keyboard.digit5Key.wasPressedThisFrame) AttackLocation();
            else if (keyboard.digit6Key.wasPressedThisFrame) ExpireAttack();
            else if (keyboard.digit7Key.wasPressedThisFrame) DefendLocation();
            else if (keyboard.digit8Key.wasPressedThisFrame) PayLocations();
            else if (keyboard.digit9Key.wasPressedThisFrame) SkipDay();
        }

        private bool ControlAndShift(Keyboard keyboard) =>
            (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
            && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

        private void KillEnemies()
        {
            int killed = 0;
            foreach (GuardRVO guard in _model.Run.Guards)
            {
                if (!guard.Alive || guard.Faction != Faction.Enemy) continue;
                _model.DamageGuard(guard, int.MaxValue);
                killed++;
            }
            Announce($"DEBUG: {killed} ENEMIES DOWN");
        }

        private void KillPlayer()
        {
            _model.DamagePlayer(int.MaxValue, "debug shortcut");
            Announce("DEBUG: PLAYER DOWN");
        }

        private void WinMission()
        {
            if (!_model.Run.Active || _model.Run.InHideout) { Announce("DEBUG: NO ACTIVE MISSION"); return; }
            KillEnemies();
            _model.Run.Won = true;
            Announce("DEBUG: MISSION WON");
        }

        private void AbandonMission()
        {
            if (!_model.Run.Active || _model.Run.InHideout) { Announce("DEBUG: NO ACTIVE MISSION"); return; }
            _model.Run.Abandoned = true;
            Announce("DEBUG: MISSION ABANDONED");
        }

        private void AttackLocation()
        {
            string mission = _model.Territory.DebugRaid();
            Announce(mission == null ? "DEBUG: NO OWNED LOCATION" : $"DEBUG: {mission} UNDER ATTACK");
        }

        private void ExpireAttack()
        {
            string mission = _model.Territory.DebugExpireRaid();
            Announce(mission == null ? "DEBUG: NO ACTIVE ATTACK" : $"DEBUG: {mission} RAID RUNS OUT");
        }

        private void DefendLocation()
        {
            string mission = _model.Territory.DebugHoldRaid();
            Announce(mission == null ? "DEBUG: NO ACTIVE ATTACK" : $"DEBUG: {mission} DEFENDED");
        }

        private void PayLocations()
        {
            int payment = _model.Territory.DebugPayOnce();
            Announce(payment <= 0 ? "DEBUG: NO LOCATION PAYMENT" : $"DEBUG: LOCATION PAYMENT +${payment}");
        }

        private void SkipDay()
        {
            TerritoryReportVO report = _model.Territory.DebugSkipDays(1);
            Announce($"DEBUG: A DAY AWAY  +${report.Income}  LOST {report.Lost.Count}");
        }

        private void Announce(string message)
        {
            Debug.Log($"[Gameplay Shortcuts] {message}");
            _signals.Outgoing.BannerShown.Dispatch(message);
        }
    }
}
#endif
