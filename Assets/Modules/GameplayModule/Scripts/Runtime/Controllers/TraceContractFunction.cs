using FlowIoC.BaseModule.Function.ReturnableFunctions;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using UnityEngine;
namespace Modules.GameplayModule.Controllers
{
    // Returns the unobstructed fraction of a ray; doors, cover crates, the concrete floors and the outer walls stop bullets and vision.
    internal class TraceContractFunction : FunctionReturn<float, Vector2, Vector2>
    {
        [Inject] private INightShiftModel _model { get; set; }
        public override float Execute(Vector2 start, Vector2 end)
        {
            float fraction = 1f;
            foreach (DoorRVO door in _model.Run.Doors)
            {
                if (door.Open) continue;
                fraction = Mathf.Min(fraction, Intersect(start, end, new Rect(door.Position.x - 0.2f, door.Position.y, 0.4f, 2.7f)));
            }
            ContractRVO run = _model.Run;
            float h = run.FloorHeight;
            float w = run.HalfWidth;
            fraction = Mathf.Min(fraction, Intersect(start, end, new Rect(-w, h - 0.25f, w * 2f, 0.25f)));
            foreach (Vector2 cover in _model.Run.Covers)
                fraction = Mathf.Min(fraction, Intersect(start, end, new Rect(cover.x - 0.05f, cover.y, 0.1f, ContractRVO.CoverHeight)));
            fraction = Mathf.Min(fraction, Intersect(start, end, new Rect(-w - 0.5f, -0.4f, w * 2f + 1f, 0.4f)));
            fraction = Mathf.Min(fraction, Intersect(start, end, new Rect(-w - 0.5f, h * 2f - 0.05f, w * 2f + 1f, 0.4f)));
            fraction = Mathf.Min(fraction, Intersect(start, end, new Rect(-w - 0.5f, -0.4f, 0.4f, h * 2f + 1f)));
            fraction = Mathf.Min(fraction, Intersect(start, end, new Rect(w + 0.1f, -0.4f, 0.4f, h * 2f + 1f)));
            return fraction;
        }
        private float Intersect(Vector2 start, Vector2 end, Rect rect)
        {
            Vector2 delta = end - start;
            float near = 0f;
            float far = 1f;
            for (int axis = 0; axis < 2; axis++)
            {
                float min = axis == 0 ? rect.xMin : rect.yMin;
                float max = axis == 0 ? rect.xMax : rect.yMax;
                if (Mathf.Abs(delta[axis]) < 0.0001f)
                {
                    if (start[axis] < min || start[axis] > max) return 1f;
                    continue;
                }
                float a = (min - start[axis]) / delta[axis];
                float b = (max - start[axis]) / delta[axis];
                near = Mathf.Max(near, Mathf.Min(a, b));
                far = Mathf.Min(far, Mathf.Max(a, b));
                if (near > far) return 1f;
            }
            return near;
        }
    }
}
