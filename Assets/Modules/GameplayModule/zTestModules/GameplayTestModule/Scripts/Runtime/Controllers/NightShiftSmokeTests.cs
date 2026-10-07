#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using FlowIoC.BaseModule.Signals;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.RootsContexts;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.GameplayTestModule.Controllers
{
    // Runs against the real bound tick sequence; all setup is confined to this editor test module.
    public class NightShiftSmokeTests
    {
        public string Execute(GameplaySystemRoot root)
        {
            var context = root.GetContext();
            var model = context.InjectionBinder.GetInstance<INightShiftModel>();
            var signals = context.InjectionBinderCrossContext.GetInstance<GameplaySignals>();
            object holder = typeof(GameplaySystemContext).GetField("_internalSignals", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(context);
            var tick = (Signal) holder.GetType().GetField("Tick").GetValue(holder);
            var passed = new List<string>();

            Reset(model);
            Vector2 before = model.Run.Player;
            model.Capture(new ContractInputVO {Move = 1, Aim = Vector2.right});
            tick.Dispatch();
            Check(model.Run.Player.x > before.x, "movement", passed);

            Reset(model);
            model.Run.Player = new Vector2(-9, 0);
            var watcher = model.Run.Guards[0];
            watcher.StunRemaining = 0;
            watcher.Position = new Vector2(-12.5f, 0);
            watcher.Left = -13f;
            watcher.Right = -12f;
            watcher.Facing = 1;
            for (int i = 0; i < 5; i++) { model.Capture(new ContractInputVO {Crouch = true}); tick.Dispatch(); }
            Check(model.Run.InCover && !model.Run.Hidden && watcher.Suspicion == 0, "cover blocks sight from the far side", passed);
            watcher.Position = new Vector2(-6.5f, 0);
            watcher.Left = -7f;
            watcher.Right = -6f;
            watcher.Facing = -1;
            model.Capture(new ContractInputVO {Crouch = true});
            tick.Dispatch();
            Check(watcher.Suspicion > 0, "cover does not hide from the near side", passed);

            Reset(model);
            model.Run.Player = new Vector2(-9, 0);
            model.Run.Guards[0].Position = new Vector2(-13, 0);
            model.Capture(new ContractInputVO {Crouch = true, Fire = true, Aim = new Vector2(-13, 0.8f)});
            Fly(tick, model);
            Check(model.Run.Guards[0].Alive && model.Run.Impacts.Count == 1 && model.Run.Impacts[0].Position.x > -10.2f, "crouched shot hits the cover", passed);

            Reset(model);
            model.Run.Player = new Vector2(-3.8f, 0);
            model.Capture(new ContractInputVO {Interact = true});
            tick.Dispatch();
            Check(model.Run.Doors[0].Open, "door interaction", passed);

            Reset(model);
            model.Run.Player = new Vector2(-4, 0);
            foreach (var guard in model.Run.Guards) guard.Alive = false;
            model.Run.Guards[0].Alive = true;
            model.Run.Guards[0].Position = new Vector2(-1, 0);
            model.Capture(new ContractInputVO {Fire = true, Aim = new Vector2(-1, 1)});
            Fly(tick, model);
            Check(model.Run.Guards[0].Alive && model.Run.Ammo == 5 && model.Run.Impacts.Count == 1, "closed door stops bullet", passed);
            model.Run.Doors[0].Open = true;
            model.Run.ShotCooldown = 0;
            model.Capture(new ContractInputVO {Fire = true, Aim = new Vector2(-1, 1)});
            Fly(tick, model);
            Check(!model.Run.Guards[0].Alive && model.Run.Ammo == 4, "open door permits hit", passed);

            Reset(model);
            model.Run.Player = new Vector2(-5.8f, 0);
            model.Capture(new ContractInputVO {Melee = true});
            tick.Dispatch();
            Check(!model.Run.Guards[0].Alive && model.Run.SilentKills == 1 && model.Run.Ammo == 6, "silent melee", passed);

            Reset(model);
            model.Run.Ammo = 2;
            model.Capture(new ContractInputVO {Reload = true});
            tick.Dispatch();
            Check(model.Run.ReloadRemaining > 0, "reload starts", passed);
            model.Run.ReloadRemaining = 0.000001f;
            tick.Dispatch();
            Check(model.Run.Ammo == 3 && model.Run.Reserve == 11 && model.Run.ReloadRemaining == 0, "reload loads one shell", passed);

            Reset(model);
            model.Run.Guards[0].StunRemaining = 0;
            model.Run.Guards[0].Suspicion = 1;
            model.Run.Guards[0].Facing = -1;
            model.Run.Player = new Vector2(-5.8f, 0);
            model.Capture(new ContractInputVO {Melee = true});
            tick.Dispatch();
            Check(model.Run.Guards[0].Alive && model.Run.Guards[0].StunRemaining > 0, "aware guard is only shoved", passed);

            Reset(model);
            model.Run.Player = new Vector2(-8f, 0);
            model.Capture(new ContractInputVO {Interact = true});
            tick.Dispatch();
            Check(model.Run.Player == new Vector2(-8f, 4.2f) && model.Run.Floor == 1, "stairwell door up", passed);
            model.Capture(new ContractInputVO {Interact = true});
            tick.Dispatch();
            Check(model.Run.Player == new Vector2(-8f, 0f) && model.Run.Floor == 0, "stairwell door down", passed);

            Reset(model);
            var hunter = model.Run.Guards[3];
            hunter.StunRemaining = 0;
            hunter.Chase = 3f;
            model.Run.Player = new Vector2(5f, 0f);
            model.Run.Guards[0].Alive = false;
            model.Run.Guards[1].Alive = false;
            tick.Dispatch();
            Check(hunter.Investigating && hunter.Target == model.Run.Player, "spotted guard hunts across floors", passed);
            hunter.Position = new Vector2(-8f, 4.2f);
            tick.Dispatch();
            Check(hunter.Position.y == 0f, "guard takes the stairwell door", passed);

            Reset(model);
            model.Run.Player = new Vector2(7.8f, 0);
            model.Capture(new ContractInputVO {Interact = true});
            tick.Dispatch();
            Check(model.Run.InVent && model.Run.Hidden, "enter vent", passed);
            model.Capture(new ContractInputVO {Interact = true});
            tick.Dispatch();
            Check(model.Run.InVent && model.Run.Player == new Vector2(-12f, 4.2f), "crawl to linked vent", passed);
            model.Capture(new ContractInputVO {Move = 1});
            tick.Dispatch();
            Check(!model.Run.InVent, "leave vent", passed);

            Reset(model);
            model.Run.Player = new Vector2(-6.8f, 0);
            model.Capture(new ContractInputVO {Interact = true});
            tick.Dispatch();
            Check(!model.Run.Lights[0].On, "kill the lights", passed);
            model.Run.Guards[0].StunRemaining = 0;
            model.Run.Player = new Vector2(-12f, 0);
            tick.Dispatch();
            Check(model.Run.Guards[0].Investigating && model.Run.Guards[0].Target.x == -6.8f, "guard goes to the switch", passed);

            Reset(model);
            model.Run.Paused = true;
            before = model.Run.Player;
            model.Capture(new ContractInputVO {Move = 1, Fire = true});
            tick.Dispatch();
            Check(model.Run.Player == before && model.Run.Ammo == 6 && model.Run.Elapsed == 0, "pause freezes simulation", passed);

            Reset(model);
            model.Run.Player = new Vector2(-7, 0);
            var shooter = model.Run.Guards[0];
            shooter.StunRemaining = 0;
            shooter.Facing = -1;
            shooter.Alerted = true;
            shooter.Suspicion = 1;
            shooter.FireCooldown = 0;
            Fly(tick, model);
            Check(!model.Run.Alive && !model.IsTicking, "one hit death stops tick", passed);

            Reset(model);
            foreach (var guard in model.Run.Guards) guard.Alive = false;
            model.Capture(new ContractInputVO {Interact = true});
            tick.Dispatch();
            Check(model.Run.Won && !model.IsTicking, "clear and extract", passed);

            Reset(model);
            model.Run.Props = new[]
            {
                new PropRVO {Position = new Vector2(-5.5f, 0f), Kind = Enums.PropKind.Barrel, Hit = new Vector2(0f, 0.5f), Size = new Vector2(0.76f, 1f), Broken = true},
                new PropRVO {Position = new Vector2(-7.5f, 0f), Kind = Enums.PropKind.Barrel, Hit = new Vector2(0f, 0.5f), Size = new Vector2(0.76f, 1f)},
            };
            tick.Dispatch();
            Check(!model.Run.Guards[0].Alive && model.Run.Props[1].Broken && model.Run.Impacts.Exists(i => i.Kind == Enums.ImpactKind.Explosion),
                "a barrel explodes, kills beside it and sets off the next", passed);

            Reset(model);
            var carrier = model.Run.Guards[1];
            carrier.Alive = false;
            tick.Dispatch();
            Check(model.Run.Pickups.Exists(p => p.Weapon == carrier.Weapon && p.Position == carrier.Position), "dead character drops its weapon", passed);
            model.Run.Player = carrier.Position;
            model.Capture(new ContractInputVO {Interact = true});
            tick.Dispatch();
            Check(model.Run.Weapon == carrier.Weapon && model.Run.Holster == "revolver", "picked-up weapon goes to hand, old one to the pocket", passed);
            model.Capture(new ContractInputVO {Swap = true});
            tick.Dispatch();
            Check(model.Run.Weapon == "revolver" && model.Run.Holster == carrier.Weapon, "Q swaps hand and pocket", passed);
            int reserve = model.Run.Reserve;
            model.Drop(model.Run.Player, "revolver", 6, 0);
            tick.Dispatch();
            Check(model.Run.Reserve == reserve + 6 && !model.Run.Pickups.Exists(p => p.Weapon == "revolver"), "walking over your own weapon takes its rounds", passed);

            signals.Incoming.StartContract.Dispatch();
            Check(model.Run.Alive && !model.Run.Won && model.Remaining == 6 && model.Run.Ammo == 6 && model.IsTicking, "retry resets run", passed);
            model.Run.Paused = true;
            return passed.Count + " PASS: " + string.Join(", ", passed);
        }

        // Bullets travel; keep ticking until every one in flight has landed.
        private void Fly(Signal tick, INightShiftModel model)
        {
            tick.Dispatch();
            for (int i = 0; i < 200 && model.Run.Bullets.Count > 0; i++) tick.Dispatch();
        }

        private void Reset(INightShiftModel model)
        {
            // The shipped contract layout, not the hideout, and none of the upgrades bought on this machine.
            model.Mission = model.Missions != null && model.Missions.Missions.Count > 0 ? model.Missions.Missions[0].Id : null;
            model.SetLevel(new ContractLevelVO());
            model.Reset();
            model.Run.Health = 1;
            model.Run.ReloadScale = 1f;
            model.Run.MoveScale = 1f;
            model.Run.Reserve = 12;
            model.Run.Weapon = "revolver";
            model.Run.Ammo = 6;
            model.Run.Holster = null;
            foreach (var guard in model.Run.Guards) guard.StunRemaining = 100;
        }

        private void Check(bool condition, string name, List<string> passed)
        {
            if (!condition) throw new InvalidOperationException("NightShift test failed: " + name);
            passed.Add(name);
        }
    }
}
#endif
