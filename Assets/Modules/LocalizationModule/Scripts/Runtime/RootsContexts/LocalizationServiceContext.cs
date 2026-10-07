using FlowIoC.BaseModule.Contexts;
using Modules.LocalizationModule.Models;
using Modules.LocalizationModule.Services;

namespace Modules.LocalizationModule.RootsContexts
{
    public class LocalizationServiceContext : Context
    {
        public override void InjectionBindings()
        {
            base.InjectionBindings();
            InjectionBinder.Bind<ILocalizationModel, LocalizationModel>();
            // The one type other modules reference directly, which is what makes this a Service.
            InjectionBinderCrossContext.Bind<ILocalizationService, LocalizationService>();
        }
    }
}
