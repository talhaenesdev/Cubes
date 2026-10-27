using System;
using System.Threading.Tasks;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.GameplayModule.Constants;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.LoadingModule.Services;
using Modules.LocalizationModule.Services;
namespace Modules.GameplayModule.Controllers
{
    // The payout, on the loading bar: long enough to read what the contract paid before the hideout appears.
    internal class DebriefCommand : Command
    {
        private const int Frames = 12;
        private const int FrameMilliseconds = 100;
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILoadingService _loading { get; set; }
        [Inject] private ILocalizationService _loc { get; set; }
        public override async void Execute()
        {
            Retain();
            ILoadingStep step = _loading.Report(NightShiftLoading.DEBRIEF_STEP);
            try
            {
                step.Start();
                MissionCVO mission = _model.Missions != null ? _model.Missions.Find(_model.Mission) : null;
                ContractRVO run = _model.Run;
                step.Detail((mission != null ? _loc.Get(mission.Title) + "  ·  " : "") + _loc.Format("{0:0.0}s  ·  {1} shots", run.Elapsed, run.Shots)
                            + (_model.Tuning.UseMoney ? "  ·  " + _loc.Format("cash ${0}", _model.Progress.Cash) : ""));
                for (int i = 1; i <= Frames; i++)
                {
                    await Task.Delay(FrameMilliseconds);
                    step.Progress(i / (float) Frames);
                }
                step.Complete();
                Release();
            }
            catch (Exception exception)
            {
                FlowLogger.LogError($"DebriefCommand threw while showing the payout: {exception}");
                step.Fail(exception.Message);
                Stop();
            }
        }
    }
}
