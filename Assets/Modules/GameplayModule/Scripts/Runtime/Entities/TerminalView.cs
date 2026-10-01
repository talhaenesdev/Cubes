using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A computer the player can use. What is written here is on its screen when the level starts;
    // what the player types replaces it for the rest of the session.
    public class TerminalView : MonoBehaviour
    {
        public string Title = "TERMINAL";
        [TextArea(3, 10)] public string Text = "";
    }
}
