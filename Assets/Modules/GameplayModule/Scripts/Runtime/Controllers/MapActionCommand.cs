using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Function.Provider;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.LocalizationModule.Services;
namespace Modules.GameplayModule.Controllers
{
    // A button on the city map's shop card: take the shop over, go and help it, or post one man more or fewer there.
    internal class MapActionCommand : Command
    {
        public const int Main = 0;
        public const int FewerGuards = 1;
        public const int MoreGuards = 2;
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [SignalParam] private int _location { get; set; }
        [SignalParam] private int _action { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (run.Menu < 0 || run.Menu >= run.Stations.Length || run.Stations[run.Menu].Kind != StationKind.MissionBoard) return;
            if (_model.Missions == null || _location < 0 || _location >= _model.Missions.Missions.Count) return;
            MissionCVO mission = _model.Missions.Missions[_location];
            ITerritoryModel territory = _model.Territory;
            if (_action == FewerGuards || _action == MoreGuards)
            {
                if (!territory.IsOurs(mission.Id)) return;
                if (_action == MoreGuards)
                {
                    if (territory.Guards(mission.Id) >= territory.MaxGuards)
                        _signals.Outgoing.BannerShown.Dispatch(_loc.Get("THIS SHOP HAS ENOUGH MEN"));
                    else if (!territory.TryHireGuard(mission.Id))
                        _signals.Outgoing.BannerShown.Dispatch(_loc.Format("NEED ${0} TO POST A MAN", territory.Config.GuardHirePrice));
                }
                else territory.SetGuards(mission.Id, territory.Guards(mission.Id) - 1);
                Refresh();
                return;
            }
            if (territory.IsUnderAttack(mission.Id))
            {
                if (!territory.StartDefense(mission.Id)) return;
                _model.Mission = mission.Id;
                _signals.Incoming.StartContract.Dispatch();
                return;
            }
            if (territory.IsOurs(mission.Id)) return;
            // Going after a friendly family's shop makes them an enemy.
            string owner = territory.OwnerOf(mission);
            if (territory.DeclareEnemy(owner))
                _signals.Outgoing.BannerShown.Dispatch(_loc.Format("{0} IS NOW AN ENEMY", _loc.Get(territory.FamilyName(owner))));
            _model.Mission = mission.Id;
            _signals.Incoming.StartContract.Dispatch();
        }

        private void Refresh()
        {
            MenuVO menu = _functions.Call<BuildMenuFunction>().AddParams(StationKind.MissionBoard).ExecuteAndGetResult<MenuVO>();
            _signals.Outgoing.MenuOpened.Dispatch(menu.Title, menu.Subtitle, menu.Entries, menu.IsMap);
        }
    }
}
