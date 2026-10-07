using FlowIoC.BaseModule.Contexts;
using Modules.AudioSettingsModule.Services;

namespace Modules.AudioSettingsModule.RootsContexts
{

    public class AudioSettingsServiceContext : Context
    {
        public override void SignalBindings()
        {
            base.SignalBindings();
        }

        public override void InjectionBindings()
        {
            base.InjectionBindings();
            InjectionBinderCrossContext.Bind<IAudioSettingsService, AudioSettingsService>();
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
