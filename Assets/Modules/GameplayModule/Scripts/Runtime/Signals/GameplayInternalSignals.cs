using FlowIoC.BaseModule.Signals;
using FlowIoC.PoolModule.Services;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using UnityEngine;
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
        public Signal<CD_Outfits> OutfitsShared = new();
        public Signal<LevelAuthoring> LevelRequested = new();
        public Signal Extracted = new();
        public Signal Abandoned = new();
        public Signal Resume = new();
        // A clip to fade in over whatever plays, or null to fade the music out.
        public Signal<AudioClip, float> MusicRequested = new();
        // True pauses the music where it is, fading it down; false fades it back up from the same place.
        public Signal<bool> MusicPaused = new();
        // The player's music volume, 0 to 1, that every track is played at.
        public Signal<float> MusicVolume = new();
    }
}
