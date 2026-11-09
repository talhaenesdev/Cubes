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
    // A line picked at a hideout station: a contract starts it, an upgrade buys its next level.
    internal class ChooseMenuCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private IFunctionProvider _functions { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        [SignalParam] private int _index { get; set; }
        public override void Execute()
        {
            ContractRVO run = _model.Run;
            if (run.Menu < 0 || run.Menu >= run.Stations.Length) return;
            StationKind kind = run.Stations[run.Menu].Kind;
            // The map is worked through its own buttons (MapActionCommand); a plain pick on it does nothing.
            if (kind == StationKind.MissionBoard) return;
            if (kind == StationKind.FamilyBoard)
            {
                // An enemy family's line buys a truce with it; any other line changes nothing.
                var families = _model.Families != null ? _model.Families.Families : null;
                if (families != null && _index >= 0 && _index < families.Count && families[_index] != null)
                {
                    ITerritoryModel territory = _model.Territory;
                    string family = families[_index].Id;
                    if (territory.RelationToPlayer(family) == FamilyRelation.Enemy)
                    {
                        if (territory.TryMakeTruce(family))
                            _signals.Outgoing.BannerShown.Dispatch(_loc.Format("TRUCE WITH {0}", _loc.Get(territory.FamilyName(family))));
                        else _signals.Outgoing.BannerShown.Dispatch(_loc.Format("NEED ${0} FOR A TRUCE", territory.Config.TrucePrice));
                    }
                }
            }
            else if (kind == StationKind.Wardrobe)
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
            _signals.Outgoing.MenuOpened.Dispatch(menu.Title, menu.Subtitle, menu.Entries, menu.IsMap);
        }
    }
}
