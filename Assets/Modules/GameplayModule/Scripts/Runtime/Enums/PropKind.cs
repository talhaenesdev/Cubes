namespace Modules.GameplayModule.Enums
{
    // Things in a level a bullet breaks.
    public enum PropKind
    {
        // Hangs from the ceiling; shot, it falls and crushes whoever stands under it.
        CeilingLamp = 0,
        // Glass that bursts into shards.
        Bottles = 1,
        // Sparks, and the screen goes dark.
        Monitor = 2,
        // Explodes: kills in a radius, blows doors open, sets off other barrels.
        Barrel = 3,
        // The pot bursts and the soil flies.
        Plant = 4,
    }
}
