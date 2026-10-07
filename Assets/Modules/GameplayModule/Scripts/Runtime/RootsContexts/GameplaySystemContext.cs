using FlowIoC.BaseModule.Contexts;
using FlowIoC.BaseModule.Controller.Commands;
using Modules.GameplayModule.Constants;
using Modules.GameplayModule.Controllers;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using Modules.GameplayModule.ViewsMediators;
using Modules.LoadingModule.Services;
namespace Modules.GameplayModule.RootsContexts
{
    public class GameplaySystemContext : Context
    {
        private GameplaySignals _signals;
        private GameplayInternalSignals _internalSignals;
        public override void SignalBindings()
        {
            base.SignalBindings();
            _signals = InjectionBinderCrossContext.Bind<GameplaySignals>();
            _internalSignals = InjectionBinder.Bind<GameplayInternalSignals>();
        }
        public override void InjectionBindings()
        {
            base.InjectionBindings();
            InjectionBinder.Bind<INightShiftModel, NightShiftModel>();
        }
        public override void MediationBindings()
        {
            base.MediationBindings();
            MediationBinder.Bind<NightShiftView>().To<NightShiftMediator>();
        }
        public override void CommandBindings()
        {
            base.CommandBindings();
            CommandBinder.Bind(_signals.Incoming.StartContract)
                .ToSequence<RequestLevelCommand>()
                .ToSequence<ResumeFromMenuCommand>()
                .ToSequence<StartContractCommand>()
                .ToSequence<QueueMusicCommand>()
                .ToSequence<RestoreRunCommand>()
                .ToSequence<ApplyMusicVolumeCommand>()
                .ToSequence<RingPhoneCommand>()
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.OpenHud)
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.TerminalClosed)
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.MenuClosed)
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.DeckClosed)
                .ToSequence<SharePoolCommand>()
                .ToSequence<ShareRosterCommand>()
                .ToSequence<StartContractTickCommand>();
            CommandBinder.Bind(_signals.Incoming.LeaveContract)
                .ToSequence<PauseForMenuCommand>()
                .ToSequence<SaveRunCommand>()
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.ReturnedToMenu);
            CommandBinder.Bind(_signals.Incoming.Play).ToSequence<PlayCommand>();
            CommandBinder.Bind(_internalSignals.Resume)
                .ToSequence<ResumeFromMenuCommand>()
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.OpenHud);
            CommandBinder.Bind(_signals.Incoming.SaveTerminal).ToSequence<SaveTerminalCommand>();
            CommandBinder.Bind(_signals.Incoming.ChooseMenu).ToSequence<ChooseMenuCommand>();
            CommandBinder.Bind(_signals.Incoming.ControlTape).ToSequence<ControlTapeCommand>();
            CommandBinder.Bind(_signals.Incoming.SetMusicVolume)
                .ToSequence<SetMusicVolumeCommand>()
                .ToSequence<ApplyMusicVolumeCommand>();
            CommandBinder.Bind(_signals.Incoming.CloseDeck)
                .ToSequence<CloseDeckCommand>()
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.DeckClosed);
            CommandBinder.Bind(_signals.Incoming.CloseMenu)
                .ToSequence<CloseMenuCommand>()
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.MenuClosed);
            CommandBinder.Bind(_signals.Incoming.CloseTerminal)
                .ToSequence<CloseTerminalCommand>()
                .ToSequence<SignalDispatchCommand>(_signals.Outgoing.TerminalClosed);
            // Out of the exit: the loading screen shows the payout, waits for a key, and the hideout loads behind it.
            CommandBinder.Bind(_internalSignals.Extracted)
                .ToSequence<ILoadingService.Commands.Begin>(NightShiftLoading.RETURN_HOME_SET)
                .ToSequence<FadeMusicCommand>()
                .ToSequence<DebriefCommand>()
                .ToSequence<ReturnHomeCommand>(NightShiftLoading.HIDEOUT_STEP)
                .ToSequence<ILoadingService.Commands.Await>(NightShiftLoading.RETURN_HOME_SET);
            // Out of the door they came in by, the job given up: no payout, straight home behind the loading screen.
            CommandBinder.Bind(_internalSignals.Abandoned)
                .ToSequence<ILoadingService.Commands.Begin>(NightShiftLoading.GIVE_UP_SET)
                .ToSequence<FadeMusicCommand>()
                .ToSequence<ReturnHomeCommand>(NightShiftLoading.GIVE_UP_HIDEOUT_STEP)
                .ToSequence<ILoadingService.Commands.Await>(NightShiftLoading.GIVE_UP_SET);
            CommandBinder.Bind(_internalSignals.LevelAuthored).ToSequence<StoreLevelCommand>();
            CommandBinder.Bind(_internalSignals.InputSampled)
                .ToSequence<ZoomCameraCommand>()
                .ToSequence<CaptureContractInputCommand>();
            CommandBinder.Bind(_internalSignals.Tick)
                .ToSequence<PhoneCallCommand>()
                .ToSequence<IntroCommand>()
                .ToSequence<SwitchTrackCommand>()
                .ToSequence<MoveAgentCommand>()
                .ToSequence<InteractContractCommand>()
                .ToSequence<FireWeaponCommand>()
                .ToSequence<SimulateGuardsCommand>()
                .ToSequence<MoveBulletsCommand>()
                .ToSequence<ResolvePropsCommand>()
                .ToSequence<DropWeaponsCommand>()
                .ToSequence<CollectLootCommand>()
                .ToSequence<JudgeContractCommand>()
                .ToSequence<SaveRunCommand>()
                .ToSequence<DrawContractCommand>();
        }
        public override void DestroyContext()
        {
            InjectionBinder.GetInstance<INightShiftModel>().IsTicking = false;
            base.DestroyContext();
        }
    }
}
