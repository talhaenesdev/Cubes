using System;
using System.Threading.Tasks;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.GameplayModule.Constants;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.LoadingModule.Services;
namespace Modules.GameplayModule.Controllers
{
    // The payout, on the loading bar: long enough to read what the contract paid before the hideout appears.
    internal class DebriefCommand : Command
    {
        private const int Frames = 12;
        private const int FrameMilliseconds = 100;
        [Inject] private INightShiftModel _model { get; set; }
        [Inject] private ILoadingService _loading { get; set; }
        public override async void Execute()
        {
            Retain();
            ILoadingStep step = _loading.Report(NightShiftLoading.DEBRIEF_STEP);
            try
            {
                step.Start();
                MissionCVO mission = _model.Missions != null ? _model.Missions.Find(_model.Mission) : null;
                ContractRVO run = _model.Run;
                step.Detail((mission != null ? mission.Title + "  -  " : "") + $"{run.Elapsed:0.0}s  -  {run.Shots} shots" + (_model.Tuning.UseMoney ? $"  -  cash ${_model.Progress.Cash}" : ""));
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
