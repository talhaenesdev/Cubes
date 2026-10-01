#if UNITY_EDITOR
using System.Collections.Generic;
using Modules.GameplayModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    public partial class NightShiftAssetBuilder
    {
        // The reload cylinder every character carries over its head.
        private RevolverCylinder Cylinder(Transform parent)
        {
            GameObject go = Node("Reload Cylinder", parent, new Vector2(0f, 2.4f));
            var cylinder = go.AddComponent<RevolverCylinder>();
            Part("Backing", go.transform, Vector2.zero, new Vector2(0.62f, 0.62f), new Color(0.02f, 0.03f, 0.05f, 0.75f), 30, 45f);
            Transform wheel = Node("Wheel", go.transform, Vector2.zero).transform;
            var chambers = new List<Object>();
            for (int i = 0; i < 6; i++)
            {
                float a = (90f - i * 60f) * Mathf.Deg2Rad;
                Vector2 at = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 0.2f;
                chambers.Add(Part("Chamber " + i, wheel, at, new Vector2(0.12f, 0.12f), _amber, 31));
            }
            SpriteRenderer shell = Part("Shell", go.transform, new Vector2(0.45f, 0.5f), new Vector2(0.08f, 0.2f), _amber, 33);
            Assign(cylinder, "_wheel", wheel);
            AssignArray(cylinder, "_chambers", chambers);
            Assign(cylinder, "_shell", shell.transform);
            go.SetActive(false);
            return cylinder;
        }
    }
}
#endif
