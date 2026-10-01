using FlowIoC.BaseModule.Signals;
using FlowIoC.PoolModule.Services;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
namespace Modules.GameplayModule.Signals
{
    internal class GameplayInternalSignals : ISignalHolder
    {
        public Signal Tick = new(hideCommandLog: true);
        public Signal<ContractInputVO> InputSampled = new(hideCommandLog: true);
        public Signal<ContractRVO> Draw = new(hideCommandLog: true);
        public Signal<ContractLevelVO> LevelAuthored = new();
        public Signal<IPoolService> PoolShared = new();
        public Signal<CD_Characters, CD_Weapons> RosterShared = new();
        public Signal<LevelAuthoring> LevelRequested = new();
        public Signal Extracted = new();
    }
}
