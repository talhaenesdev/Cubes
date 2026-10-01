namespace Modules.GameplayModule.Data.ValueObjects
{
    // A list the HUD shows at a hideout station: what it is, the player's cash, and one line per choice.
    public class MenuVO
    {
        public string Title;
        public string Subtitle;
        public string[] Entries;
    }
}
