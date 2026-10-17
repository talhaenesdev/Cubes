using System.Collections.Generic;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LocalizationModule.Services;

namespace Modules.GameplayModule.Controllers
{
    // Home after time away: what the shops earned, and which were lost or held while the player was gone.
    internal class ReportAbsenceCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            TerritoryReportVO report = _model.Territory.PendingReport;
            if (report == null || !_model.Run.InHideout) return;
            _model.Territory.PendingReport = null;
            long hours = report.AwayMilliseconds / 3600000L;
            long minutes = report.AwayMilliseconds / 60000L % 60L;
            var lines = new List<string> {_loc.Format("AWAY {0}h {1:00}m  ·  +${2}", hours, minutes, report.Income)};
            if (report.Lost.Count > 0) lines.Add(_loc.Format("LOST: {0}", Names(report.Lost)));
            if (report.Held.Count > 0) lines.Add(_loc.Format("HELD: {0}", Names(report.Held)));
            _signals.Outgoing.BannerShown.Dispatch(string.Join("\n", lines));
        }

        private string Names(List<string> ids)
        {
            var names = new List<string>();
            foreach (string id in ids)
            {
                MissionCVO mission = _model.Missions.Find(id);
                string title = mission != null ? _loc.Get(mission.Title) : id;
                names.Add(title.Contains("/") ? title[(title.IndexOf('/') + 1)..].Trim() : title);
            }
            return string.Join(", ", names);
        }
    }
}
