#if UNITY_EDITOR
using Modules.GameplayModule.Entities;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The cash a dead guard drops: a few green bills under a brass clip, with a glint.
    public partial class NightShiftAssetBuilder
    {
        public const string CashPrefab = Prefabs + "/Weapons/PB_CashPickup.prefab";

        private void BuildCash()
        {
            if (!ShouldWrite(CashPrefab)) return;
            GameObject root = Node("PB_CashPickup", null, Vector2.zero);
            var view = root.AddComponent<CashPickupView>();
            Part("Shadow", root.transform, new Vector2(0f, 0.02f), new Vector2(0.5f, 0.04f), new Color(0f, 0f, 0f, 0.45f), -4);
            SpriteRenderer glint = Part("Glint", root.transform, new Vector2(0f, 0.18f), new Vector2(0.8f, 0.8f), new Color(0.6f, 1f, 0.6f, 0.3f), -3);
            glint.sprite = _soft;
            Transform bundle = Node("Bundle", root.transform, new Vector2(0f, 0.14f)).transform;
            Color bill = new(0.36f, 0.55f, 0.32f);
            Part("Bill back", bundle, new Vector2(0.03f, 0.02f), new Vector2(0.42f, 0.18f), bill * 0.75f, 10, 6f);
            Part("Bill", bundle, new Vector2(-0.02f, 0f), new Vector2(0.42f, 0.18f), bill, 11, -4f);
            Part("Bill top", bundle, new Vector2(0f, 0.01f), new Vector2(0.42f, 0.18f), bill * 1.15f, 12);
            Part("Seal", bundle, new Vector2(0f, 0.01f), new Vector2(0.08f, 0.08f), new Color(0.2f, 0.32f, 0.18f), 13).sprite = _shapes[Enums.CharacterShape.Circle];
            Part("Clip", bundle, new Vector2(0f, 0.01f), new Vector2(0.07f, 0.2f), _hatBrass, 14);
            Assign(view, "_bundle", bundle);
            Assign(view, "_glint", glint);
            SavePrefab<GameObject>(root, CashPrefab);
        }
    }
}
#endif
