using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Function.Provider;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.Controllers
{
    // A line picked at a hideout station: a contract starts it, an upgrade buys its next level.
    internal class ChooseMenuCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [SignalParam] private int _index { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (run.Menu < 0 || run.Menu >= run.Stations.Length) return;
            StationKind kind = run.Stations[run.Menu].Kind;
            if (kind == StationKind.MissionBoard)
            {
                if (_model.Missions == null || _index < 0 || _index >= _model.Missions.Missions.Count) return;
                _model.Mission = _model.Missions.Missions[_index].Id;
                _signals.Incoming.StartContract.Dispatch();
                return;
            }
            if (kind == StationKind.Wardrobe)
            {
                if (_index >= 0 && _index < _model.Outfits.Outfits.Count) _model.Wear(_model.Outfits.Outfits[_index].Id);
            }
            else if (kind == StationKind.WeaponLocker)
            {
                // Lines 0 and 1 put away the hand and pocket weapons; the rest take stored ones, in locker order.
                if (_index <= 1) _model.StoreWeapon(_index);
                else
                {
                    int stored = 0;
                    foreach (string weapon in _model.Progress.Locker)
                    {
                        if (_model.Weapon(weapon) == null) continue;
                        if (stored++ != _index - 2) continue;
                        _model.TakeWeapon(weapon);
                        break;
                    }
                }
            }
            else
            {
                if (_index < 0 || _index >= _model.Upgrades.Upgrades.Count) return;
                _model.Buy(_model.Upgrades.Upgrades[_index].Id);
            }
            MenuVO menu = _functions.Call<BuildMenuFunction>().AddParams(kind).ExecuteAndGetResult<MenuVO>();
            _signals.Outgoing.MenuOpened.Dispatch(menu.Title, menu.Subtitle, menu.Entries);
        }
    }
}
