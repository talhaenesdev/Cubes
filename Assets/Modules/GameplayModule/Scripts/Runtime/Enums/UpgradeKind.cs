namespace Modules.GameplayModule.Enums
{
    public enum UpgradeKind
    {
        // +Amount hits the player survives.
        Health = 0,
        // Reloads take Amount percent less time per level.
        ReloadSpeed = 1,
        // The player moves Amount percent faster per level.
        MoveSpeed = 2,
        // +Amount spare rounds for the starting weapon per level.
        Ammo = 3,
    }
}
