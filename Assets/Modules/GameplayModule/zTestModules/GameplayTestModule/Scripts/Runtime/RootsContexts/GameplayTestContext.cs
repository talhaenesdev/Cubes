#if UNITY_EDITOR
using FlowIoC.BaseModule.Contexts;

namespace Modules.GameplayModule.GameplayTestModule.RootsContexts
{

    public class GameplayTestContext : Context
    {
        public override void SignalBindings()
        {
            base.SignalBindings();
        }

        public override void InjectionBindings()
        {
            base.InjectionBindings();
        }

        public override void MediationBindings()
        {
            base.MediationBindings();
        }

        public override void CommandBindings()
        {
            base.CommandBindings();
        }

        public override void Setup()
        {
            base.Setup();
        }

        public override void Launch()
        {
            base.Launch();
        }
    }
}
#endif
