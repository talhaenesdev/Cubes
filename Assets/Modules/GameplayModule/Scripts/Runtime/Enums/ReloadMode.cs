namespace Modules.GameplayModule.Enums
{
    public enum ReloadMode
    {
        // One round per ReloadTime; holding R keeps loading, firing interrupts. Revolvers, shotguns.
        PerRound = 0,
        // The whole magazine after ReloadTime. Pistols, SMGs, rifles.
        Magazine = 1,
    }
}
