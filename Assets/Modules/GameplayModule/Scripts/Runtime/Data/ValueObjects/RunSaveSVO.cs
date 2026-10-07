using System;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // A contract as it stood when it was last saved: which one, and the whole run - the player, every guard,
    // door, light, pickup and prop - so the next launch carries on from there.
    [Serializable]
    public class RunSaveSVO
    {
        public string Mission;
        public ContractRVO Run;
    }
}
