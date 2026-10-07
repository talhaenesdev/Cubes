namespace Modules.GameplayModule.Enums
{
    // Something in the hideout the player walks up to and uses.
    public enum StationKind
    {
        MissionBoard = 0,
        UpgradeBench = 1,
        // E plays a hideout tape, and the next one on every press after.
        CassettePlayer = 2,
        // Lists CD_Outfits; picking one puts it on.
        Wardrobe = 3,
        // Puts the carried weapons away and hands back the stored ones.
        WeaponLocker = 4,
    }
}
