namespace Modules.GameplayModule.Constants
{
    // The loading set CD_LoadingSets declares for the trip home after a contract, and the steps it waits for.
    public static class NightShiftLoading
    {
        public const string RETURN_HOME_SET = "ReturnHome";
        // The trip home after giving a job up: no payout, so only the hideout to wait for.
        public const string GIVE_UP_SET = "GiveUp";
        public const string DEBRIEF_STEP = "Debrief";
        public const string CONTINUE_STEP = "Continue";
        public const string HIDEOUT_STEP = "Hideout";
        // A step key belongs to one set, so the give-up trip home has a hideout step of its own.
        public const string GIVE_UP_HIDEOUT_STEP = "GiveUpHideout";
    }
}
