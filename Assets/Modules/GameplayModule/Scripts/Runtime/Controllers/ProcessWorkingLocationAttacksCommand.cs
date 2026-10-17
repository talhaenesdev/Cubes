using System;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LocalizationModule.Services;

namespace Modules.GameplayModule.Controllers
{
    // Raids while the game is open: rolled, run out and settled by the territory; this step only tells the player.
    internal class ProcessWorkingLocationAttacksCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            WorkingLocationUpdateVO update = _model.Territory.ProcessRaids(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            if (!string.IsNullOrEmpty(update.AttackedMission))
                _signals.Outgoing.BannerShown.Dispatch(_loc.Format("UNDER ATTACK: {0}", ShopName(update.AttackedMission)) + "\n" + _loc.Get("Open the city map to go and help"));
            if (!string.IsNullOrEmpty(update.HeldMission))
                _signals.Outgoing.BannerShown.Dispatch(_loc.Format("OUR MEN HELD {0}", ShopName(update.HeldMission)));
            if (!string.IsNullOrEmpty(update.LostMission))
                _signals.Outgoing.BannerShown.Dispatch(_loc.Format("LOCATION LOST: {0}", ShopName(update.LostMission)));
        }

        private string ShopName(string id)
        {
            MissionCVO mission = _model.Missions.Find(id);
            if (mission == null) return id;
            string title = _loc.Get(mission.Title);
            return title.Contains("/") ? title[(title.IndexOf('/') + 1)..].Trim() : title;
        }
    }
}
