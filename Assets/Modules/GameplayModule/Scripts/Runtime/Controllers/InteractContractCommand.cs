using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Function.Provider;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LocalizationModule.Services;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // E does one thing per press: the first interaction in reach, in this order, owns both the hint and the press.
    internal class InteractContractCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        private bool _press;
        private string _hint;
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            // A boss conversation intentionally pauses the world, while this command must keep
            // receiving E so each line can advance and the conversation can finish.
            if (!run.Alive || run.Won || run.Abandoned || (run.Paused && !run.BossDialogueOpen)) return;
            bool documentNear = run.HasDocument && !run.DocumentTaken && !run.InVent
                && Mathf.Abs(run.Player.y - run.Document.y) < 0.5f && Mathf.Abs(run.Player.x - run.Document.x) < 1f;
            _press = _model.Input.Interact;
            _hint = null;
            if (run.LocationTakeover && OfferBossTalk(run))
            {
                run.Hint = _hint ?? "";
                return;
            }
            CollectRounds(run);
            if (_model.Input.Swap && !run.InVent) SwapWeapons(run);
            // Crouching over a weapon you do not carry makes E pick it up before anything else in reach.
            if (run.Crouching && !run.InVent) OfferPickup(run);
            if (documentNear && Offer(_loc.Get("E  —  take the documents")))
            {
                run.DocumentTaken = true;
                _signals.Outgoing.BannerShown.Dispatch(_loc.Get("DOCUMENTS SECURED"));
            }
            // A vent is a place to hide: in, out of sight, and out again where you went in. It leads nowhere.
            if (run.InVent && Offer(_loc.Get("E  —  climb out of the vent")))
            {
                run.InVent = false;
                run.Hidden = false;
                run.Vent = -1;
            }
            else if (!run.InVent)
                for (int i = 0; i < run.Vents.Length; i++)
                {
                    if (Vector2.Distance(run.Player, run.Vents[i].Position) > 0.8f) continue;
                    if (!Offer(_loc.Get("E  —  hide in the vent"))) continue;
                    run.InVent = true;
                    run.Hidden = true;
                    run.Vent = i;
                    run.Player = run.Vents[i].Position;
                    run.Floor = run.Player.y > run.FloorHeight * 0.5f ? 1 : 0;
                    run.VerticalSpeed = 0;
                    break;
                }
            foreach (StairRVO stair in run.Stairs)
            {
                if (run.InVent || Vector2.Distance(run.Player, stair.Position) > 0.8f) continue;
                StairRVO exit = run.Stairs[stair.Link];
                if (!Offer(_loc.Get(exit.Position.y > stair.Position.y ? "E  —  stairs up" : "E  —  stairs down"))) continue;
                run.Player = exit.Position;
                run.Floor = run.Player.y > run.FloorHeight * 0.5f ? 1 : 0;
                run.VerticalSpeed = 0;
                break;
            }
            foreach (LightRVO light in run.Lights)
            {
                if (run.InVent || Mathf.Abs(run.Player.y - light.Floor) > 1.8f || Mathf.Abs(run.Player.x - light.Switch) > 0.7f) continue;
                if (Offer(_loc.Get(light.On ? "E  —  kill the lights" : "E  —  turn the lights on"))) light.On = !light.On;
            }
            for (int i = 0; i < run.Stations.Length; i++)
            {
                StationRVO station = run.Stations[i];
                if (run.InVent || Mathf.Abs(run.Player.y - station.Position.y) > 0.5f || Mathf.Abs(run.Player.x - station.Position.x) > 1f) continue;
                if (station.Kind == Enums.StationKind.CassettePlayer)
                {
                    if (Offer("E")) OpenDeck(run);
                    continue;
                }
                // At home the icon over the station says what it is; the prompt is only the key.
                if (!Offer("E")) continue;
                run.Menu = i;
                run.Paused = true;
                MenuVO menu = _functions.Call<BuildMenuFunction>().AddParams(station.Kind).ExecuteAndGetResult<MenuVO>();
                _signals.Outgoing.MenuOpened.Dispatch(menu.Title, menu.Subtitle, menu.Entries, menu.IsMap);
            }
            for (int i = 0; i < run.Terminals.Length; i++)
            {
                TerminalRVO terminal = run.Terminals[i];
                if (run.InVent || Mathf.Abs(run.Player.y - terminal.Position.y) > 0.5f || Mathf.Abs(run.Player.x - terminal.Position.x) > 1f) continue;
                if (!Offer(_loc.Format("E  —  use the computer: {0}", _loc.Get(terminal.Title)))) continue;
                run.Terminal = i;
                run.Paused = true;
                _signals.Outgoing.TerminalOpened.Dispatch(_loc.Get(terminal.Title), _loc.Get(terminal.Text));
            }
            foreach (DoorRVO door in run.Doors)
            {
                if (run.InVent || Vector2.Distance(run.Player, door.Position) > 1.6f) continue;
                if (!Offer(_loc.Get(door.Open ? "E  —  close the door" : "E  —  kick the door"))) continue;
                door.Open = !door.Open;
                if (!door.Open) continue;
                run.NoiseRemaining = 0.3f;
                run.NoiseRadius = _model.Tuning.KickNoise;
                run.NoisePosition = door.Position;
                foreach (GuardRVO guard in run.Guards)
                    if (guard.Alive && Mathf.Abs(guard.Position.y - door.Position.y) < 0.1f && Mathf.Abs(guard.Position.x - door.Position.x) < _model.Tuning.KickReach)
                        guard.StunRemaining = _model.Tuning.KickStun;
            }
            OfferPickup(run);
            WeaponCVO held = _model.Weapon(run.Weapon);
            if (_model.Input.Reload && held != null && run.Ammo < held.Magazine && run.Reserve > 0 && run.ReloadRemaining == 0)
                run.ReloadRemaining = held.ReloadTime * run.ReloadScale;
            bool exitOpen = !run.LocationTakeover && !run.IsLocationDefense && (run.HasDocument ? run.DocumentTaken : _model.Remaining == 0);
            if (!run.InHideout && exitOpen && !run.InVent)
            {
                // With the file in hand, stepping through the exit takes the player straight home.
                if (Mathf.Abs(run.Player.x - run.ExitX) < _model.Tuning.ExitReach && run.Player.y < 0.5f) run.Won = true;
                else if (run.HasDocument)
                    _hint ??= _loc.Format("Documents secured - get to the exit  {0}",
                        (run.Player.y > 0.5f ? _loc.Get("downstairs") + "  " : "") + (run.ExitX < run.Player.x ? "<<" : ">>"));
                else _hint ??= _loc.Get("Clear - back to the exit");
            }
            // Back at the door they came in by, before the job is done: E gives it up and goes home.
            if (!run.InHideout && !exitOpen && !run.InVent && run.Player.y < 0.5f
                && Mathf.Abs(run.Player.x - run.ExitX) < _model.Tuning.ExitReach
                && Offer(_loc.Get("E  —  give up the job and go home")))
                run.Abandoned = true;
            if (!run.InHideout && run.HasDocument && !run.DocumentTaken) _hint ??= _loc.Get("Find the documents");
            if (!run.InHideout && run.LocationTakeover && _model.Remaining == 0 && !run.BossRecruited)
                _hint ??= _loc.Get("Talk to the boss");
            // Only what the player can do right here; the rest of the time the line stays empty.
            run.Hint = _hint ?? "";
        }

        // The owner, once their men are down: E starts the talk, and the lines come as bubbles over whoever says them -
        // on E, or on their own after a while. The world holds still meanwhile; the player keeps the gun on them.
        private bool OfferBossTalk(ContractRVO run)
        {
            int bossIndex = -1;
            for (int i = 0; i < run.Guards.Length; i++)
                if (run.Guards[i].Boss && run.Guards[i].Alive && !run.Guards[i].Escaped) { bossIndex = i; break; }
            if (bossIndex < 0)
            {
                _hint ??= _loc.Get("The boss is gone");
                return false;
            }
            GuardRVO boss = run.Guards[bossIndex];
            bool near = !run.InVent && Mathf.Abs(run.Player.y - boss.Position.y) < 0.8f && Mathf.Abs(run.Player.x - boss.Position.x) < 2.2f;
            if (!near && !run.BossDialogueOpen) return false;
            // The boss owns this spot. Before the guards are clear, nothing else should show an E prompt here.
            if (_model.Remaining > 0) return near;
            var lines = _model.Territory.Config.Persuasion;
            if (lines == null || lines.Count == 0)
            {
                Recruit(run);
                return true;
            }
            if (!run.BossDialogueOpen)
            {
                _hint = _loc.Get("E  —  talk to the boss");
                if (!_press) return true;
                _press = false;
                run.BossDialogueOpen = true;
                run.BossDialogue = 0;
                run.Paused = true;
                Say(run, lines[0], bossIndex);
                return true;
            }
            // Facing them down: the boss cowers, the gun stays on them.
            boss.Crouching = true;
            boss.Facing = run.Player.x < boss.Position.x ? -1 : 1;
            run.Aim = boss.Position + Vector2.up * 1.1f;
            run.SpeechTimer += Mathf.Min(Time.deltaTime, 0.05f);
            _hint = _loc.Get("E  —  next");
            // A press only counts once the line has been up a moment, so the E that opened the talk does not skip its first line.
            bool next = (_press && run.SpeechTimer > 0.4f) || run.SpeechTimer >= _model.Territory.Config.BubbleTime;
            if (!next) return true;
            _press = false;
            run.BossDialogue++;
            if (run.BossDialogue < lines.Count)
            {
                Say(run, lines[run.BossDialogue], bossIndex);
                return true;
            }
            Recruit(run);
            return true;
        }

        private void Say(ContractRVO run, DialogueLineCVO line, int bossIndex)
        {
            run.SpeechSpeaker = line.Speaker == DialogueSpeaker.Player ? -1 : bossIndex;
            string family = _loc.Get(_model.Territory.FamilyName(_model.Missions?.Find(_model.Mission)?.OwnerFamily));
            run.SpeechText = _loc.Format(line.Text, family);
            run.SpeechTimer = 0f;
        }

        private void Recruit(ContractRVO run)
        {
            run.BossRecruited = true;
            run.BossDialogueOpen = false;
            run.SpeechSpeaker = -2;
            run.SpeechText = "";
            run.Paused = false;
            run.Won = true;
            _signals.Outgoing.BannerShown.Dispatch(_loc.Get("THE BOSS WORKS FOR US NOW"));
        }
        // Walking over a weapon you already carry, in hand or in the pocket, empties its rounds into yours.
        private void CollectRounds(ContractRVO run)
        {
            for (int i = run.Pickups.Count - 1; i >= 0; i--)
            {
                PickupRVO pickup = run.Pickups[i];
                if (run.InVent || !InReach(run, pickup)) continue;
                if (pickup.Weapon == run.Weapon) run.Reserve += pickup.Ammo + pickup.Reserve;
                else if (pickup.Weapon == run.Holster) run.HolsterReserve += pickup.Ammo + pickup.Reserve;
                else continue;
                run.Pickups.RemoveAt(i);
            }
        }

        private void SwapWeapons(ContractRVO run)
        {
            if (string.IsNullOrEmpty(run.Holster)) return;
            (run.Weapon, run.Holster) = (run.Holster, run.Weapon);
            (run.Ammo, run.HolsterAmmo) = (run.HolsterAmmo, run.Ammo);
            (run.Reserve, run.HolsterReserve) = (run.HolsterReserve, run.Reserve);
            run.ReloadRemaining = 0;
            run.ShotCooldown = 0.3f;
        }

        // A weapon you do not carry: into the hand, with the one you held going to an empty pocket, or to the floor.
        private void OfferPickup(ContractRVO run)
        {
            for (int i = 0; i < run.Pickups.Count; i++)
            {
                PickupRVO pickup = run.Pickups[i];
                if (run.InVent || !InReach(run, pickup) || pickup.Weapon == run.Weapon || pickup.Weapon == run.Holster) continue;
                WeaponCVO found = _model.Weapon(pickup.Weapon);
                if (found == null) continue;
                WeaponCVO held = _model.Weapon(run.Weapon);
                bool pocket = string.IsNullOrEmpty(run.Holster) && held != null;
                string then = held == null ? "" : _loc.Format(pocket ? "  ({0} to the pocket)" : "  (drops the {0})", _loc.Get(held.DisplayName));
                if (!Offer(_loc.Format("E  —  pick up the {0}", _loc.Get(found.DisplayName)) + then)) continue;
                run.Pickups.RemoveAt(i);
                if (pocket)
                {
                    run.Holster = run.Weapon;
                    run.HolsterAmmo = run.Ammo;
                    run.HolsterReserve = run.Reserve;
                }
                else if (held != null) _model.Drop(run.Player, run.Weapon, run.Ammo, run.Reserve);
                run.Weapon = pickup.Weapon;
                run.Ammo = pickup.Ammo;
                run.Reserve = pickup.Reserve;
                run.ReloadRemaining = 0;
                run.ShotCooldown = 0.25f;
                return;
            }
        }

        private bool InReach(ContractRVO run, PickupRVO pickup) =>
            Mathf.Abs(run.Player.y - pickup.Position.y) < 0.6f && Mathf.Abs(run.Player.x - pickup.Position.x) < 1f;

        // E on the deck opens its panel, showing the tape loaded and whether it plays.
        private void OpenDeck(ContractRVO run)
        {
            run.DeckOpen = true;
            run.Paused = true;
            MusicTrackCVO loaded = _model.Music != null ? _model.Music.Find(_model.PlayingTrack) : null;
            bool tape = loaded != null && loaded.Category == Enums.MusicCategory.Hideout;
            _signals.Outgoing.DeckShown.Dispatch(tape ? loaded.Title : _loc.Get("NO TAPE"), tape ? loaded.Artist : _loc.Get("Press PLAY"), run.TapePlaying);
        }

        private bool Offer(string text)
        {
            if (_hint != null) return false;
            _hint = text;
            if (!_press) return false;
            _press = false;
            return true;
        }
    }
}
