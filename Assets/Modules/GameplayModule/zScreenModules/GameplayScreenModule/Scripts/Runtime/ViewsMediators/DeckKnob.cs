using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    // A rotary volume knob: drag up or right to turn it up, or roll the mouse wheel over it.
    // It sweeps 270 degrees, from seven o'clock to five o'clock, like an old deck's pot.
    public class DeckKnob : MonoBehaviour, IDragHandler, IScrollHandler
    {
        private const float Sweep = 270f;
        public RectTransform Dial;
        public Action<float> Changed;
        public float Value { get; private set; }

        public void SetWithoutNotify(float value)
        {
            Value = Mathf.Clamp01(value);
            if (Dial != null) Dial.localEulerAngles = new Vector3(0f, 0f, Sweep * 0.5f - Value * Sweep);
        }

        public void OnDrag(PointerEventData eventData) => Turn((eventData.delta.y + eventData.delta.x) * 0.006f);
        public void OnScroll(PointerEventData eventData) => Turn(eventData.scrollDelta.y * 0.05f);

        private void Turn(float amount)
        {
            float before = Value;
            SetWithoutNotify(Value + amount);
            if (!Mathf.Approximately(before, Value)) Changed?.Invoke(Value);
        }
    }
}
