using System;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    // Once per launch, before the first run: everything that happened to the city while the game was closed.
    internal class CatchUpTerritoryCommand : Command
    {
        [Inject] private INightShiftModel _model { get; set; }

        public override void Execute()
        {
            if (_model.TerritoryCaughtUp) return;
            _model.TerritoryCaughtUp = true;
            _model.Territory.CatchUp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
}
