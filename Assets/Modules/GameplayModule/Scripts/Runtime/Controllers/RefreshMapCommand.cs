using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Function.Provider;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // The open city map is redrawn once a second, so raid countdowns and income run live on it.
    internal class RefreshMapCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (run.Menu < 0 || run.Menu >= run.Stations.Length || run.Stations[run.Menu].Kind != StationKind.MissionBoard) return;
            if (Time.unscaledTime < _model.MapRefreshAt) return;
            _model.MapRefreshAt = Time.unscaledTime + _model.Tuning.MapRefreshInterval;
            MenuVO menu = _functions.Call<BuildMenuFunction>().AddParams(StationKind.MissionBoard).ExecuteAndGetResult<MenuVO>();
            _signals.Outgoing.MenuOpened.Dispatch(menu.Title, menu.Subtitle, menu.Entries, menu.IsMap);
        }
    }
}
