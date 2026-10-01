using System;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Update;
using Modules.GameplayModule.Constants;
using Modules.LoadingModule.Services;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Modules.GameplayModule.Controllers
{
    // Holds the loading screen on its full bar until the player presses a key or a mouse button. The first
    // moments are ignored, so the E that opened the exit does not skip the payout.
    internal class WaitForKeyCommand : Command
    {
        private const float Grace = 0.35f;
        [Inject] private ILoadingService _loading { get; set; }
        [Inject] private IUpdateProvider _updates { get; set; }
        public override void Execute()
        {
            Retain();
            ILoadingStep step = _loading.Report(NightShiftLoading.CONTINUE_STEP);
            step.Start();
            float opened = Time.unscaledTime;
            IUpdateProvider updates = _updates;
            Action onFrame = null;
            onFrame = () =>
            {
                if (Time.unscaledTime - opened < Grace || !Pressed()) return;
                updates.RemoveUpdate(onFrame);
                step.Complete();
                Release();
            };
            updates.AddUpdate(onFrame);
        }
        private bool Pressed()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            return (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
                   || (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame));
        }
    }
}
