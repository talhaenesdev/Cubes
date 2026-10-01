using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.ViewsMediators.Mediator;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Signals;
namespace Modules.GameplayModule.ViewsMediators
{
    public class NightShiftMediator : IMediator
    {
        [Inject] private NightShiftView _view { get; set; }
        [InjectSignal] private GameplayInternalSignals _signals { get; set; }
        public void OnRegister()
        {
            _view.InputSampled += OnInput;
            _signals.Draw.AddListener(_view.Draw);
            _signals.PoolShared.AddListener(_view.SetPool);
            _signals.RosterShared.AddListener(_view.SetRoster);
            _signals.LevelRequested.AddListener(OnLevelRequested);
            ContractLevelVO level = _view.LoadLevel();
            if (level != null) _signals.LevelAuthored.Dispatch(level);
        }
        public void OnRemove()
        {
            _view.InputSampled -= OnInput;
            _signals.Draw.RemoveListener(_view.Draw);
            _signals.PoolShared.RemoveListener(_view.SetPool);
            _signals.RosterShared.RemoveListener(_view.SetRoster);
            _signals.LevelRequested.RemoveListener(OnLevelRequested);
        }
        private void OnInput(ContractInputVO input) => _signals.InputSampled.Dispatch(input);
        private void OnLevelRequested(Entities.LevelAuthoring prefab)
        {
            ContractLevelVO level = _view.LoadLevel(prefab);
            if (level != null) _signals.LevelAuthored.Dispatch(level);
        }
    }
}
