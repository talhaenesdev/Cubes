using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A hideout station: the mission board lists contracts, the upgrade bench sells upgrades. E opens its list.
    public class StationView : MonoBehaviour
    {
        public StationKind Kind = StationKind.MissionBoard;
    }
}
