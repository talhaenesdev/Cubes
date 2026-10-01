#if UNITY_EDITOR
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // Things a bullet breaks, and the bursts that go with every surface a bullet can hit.
    public partial class NightShiftAssetBuilder
    {
        private string PropPath(PropKind kind) => Prefabs + "/Fixtures/PB_Prop_" + kind + ".prefab";

        private void BuildProps()
        {
            Sound("SND_SFX_Glass_01", 0.4f, 2600f, 9f, 1);
            Sound("SND_SFX_Ricochet_01", 0.35f, 1900f, 9f, 0);
            Sound("SND_SFX_Explosion_01", 1.2f, 45f, 3.2f, 2);

            Prop(PropKind.CeilingLamp, new Vector2(0f, 3.45f), new Vector2(0.6f, 0.5f), (root, body, intact, broken) =>
            {
                Part("Cord", root, new Vector2(0f, 3.78f), new Vector2(0.03f, 0.35f), _ink, -6);
                Part("Shade", body, new Vector2(0f, 0.05f), new Vector2(0.6f, 0.22f), new Color(0.2f, 0.18f, 0.16f), -5);
                Part("Shade rim", body, new Vector2(0f, -0.06f), new Vector2(0.64f, 0.04f), _hatBrass, -4);
                SpriteRenderer bulb = Part("Bulb", intact, new Vector2(0f, -0.1f), new Vector2(0.16f, 0.12f), new Color(1f, 0.85f, 0.5f), -4);
                bulb.sprite = _shapes[CharacterShape.Circle];
                Part("Glow", intact, new Vector2(0f, -0.2f), new Vector2(1.6f, 0.9f), new Color(1f, 0.75f, 0.4f, 0.22f), -27).sprite = _soft;
                Part("Cone", intact, new Vector2(0f, -1.6f), new Vector2(2.4f, 3f), new Color(1f, 0.75f, 0.4f, 0.06f), -27).sprite = _soft;
                Part("Dent", broken, new Vector2(0.1f, 0.02f), new Vector2(0.3f, 0.06f), _ink, -3, 12f);
                Part("Shards", broken, new Vector2(-0.12f, -0.1f), new Vector2(0.12f, 0.05f), new Color(0.8f, 0.8f, 0.7f), -3, -20f);
            });
            Prop(PropKind.Bottles, new Vector2(0f, 1.05f), new Vector2(0.62f, 0.42f), (root, body, intact, broken) =>
            {
                Part("Table", root, new Vector2(0f, 0.78f), new Vector2(0.9f, 0.08f), _wood, -20);
                Part("Leg L", root, new Vector2(-0.35f, 0.38f), new Vector2(0.06f, 0.76f), _wood, -20);
                Part("Leg R", root, new Vector2(0.35f, 0.38f), new Vector2(0.06f, 0.76f), _wood, -20);
                Color[] glass = {new(0.2f, 0.35f, 0.2f, 0.9f), new(0.45f, 0.28f, 0.12f, 0.9f), new(0.6f, 0.65f, 0.6f, 0.7f)};
                for (int i = 0; i < 3; i++)
                {
                    float x = -0.2f + i * 0.2f;
                    Part("Bottle " + i, intact, new Vector2(x, 1.0f), new Vector2(0.12f, 0.3f), glass[i], -19);
                    Part("Neck " + i, intact, new Vector2(x, 1.2f), new Vector2(0.05f, 0.12f), glass[i], -19);
                }
                Part("Spill", broken, new Vector2(0f, 0.83f), new Vector2(0.7f, 0.03f), new Color(0.3f, 0.15f, 0.08f, 0.8f), -19);
                Part("Stub", broken, new Vector2(-0.2f, 0.88f), new Vector2(0.12f, 0.08f), glass[0], -19, 15f);
            });
            Prop(PropKind.Monitor, new Vector2(0f, 1.3f), new Vector2(0.62f, 0.5f), (root, body, intact, broken) =>
            {
                Part("Desk", root, new Vector2(0f, 0.9f), new Vector2(1.4f, 0.12f), _wood, -20);
                Part("Leg L", root, new Vector2(-0.6f, 0.45f), new Vector2(0.08f, 0.9f), _wood, -20);
                Part("Leg R", root, new Vector2(0.6f, 0.45f), new Vector2(0.08f, 0.9f), _wood, -20);
                Part("Case", root, new Vector2(0f, 1.3f), new Vector2(0.62f, 0.5f), new Color(0.35f, 0.32f, 0.28f), -19);
                Part("Screen", intact, new Vector2(0f, 1.32f), new Vector2(0.46f, 0.34f), new Color(0.75f, 0.55f, 0.2f), -18);
                Part("Screen glow", intact, new Vector2(0f, 1.32f), new Vector2(1.2f, 0.9f), new Color(1f, 0.7f, 0.3f, 0.12f), -26).sprite = _soft;
                Part("Dead screen", broken, new Vector2(0f, 1.32f), new Vector2(0.46f, 0.34f), new Color(0.04f, 0.04f, 0.04f), -18);
                Line("Crack", broken, new Vector2(-0.18f, 1.45f), new Vector2(0.15f, 1.2f), 0.02f, new Color(0.6f, 0.6f, 0.6f), -17);
                Line("Crack 2", broken, new Vector2(0f, 1.32f), new Vector2(0.2f, 1.45f), 0.02f, new Color(0.6f, 0.6f, 0.6f), -17);
            });
            Prop(PropKind.Barrel, new Vector2(0f, 0.5f), new Vector2(0.76f, 1f), (root, body, intact, broken) =>
            {
                Part("Drum", intact, new Vector2(0f, 0.5f), new Vector2(0.72f, 1f), new Color(0.5f, 0.08f, 0.08f), 14);
                Part("Rib top", intact, new Vector2(0f, 0.82f), new Vector2(0.76f, 0.05f), new Color(0.3f, 0.05f, 0.05f), 15);
                Part("Rib low", intact, new Vector2(0f, 0.2f), new Vector2(0.76f, 0.05f), new Color(0.3f, 0.05f, 0.05f), 15);
                Part("Hazard", intact, new Vector2(0f, 0.52f), new Vector2(0.72f, 0.16f), new Color(0.85f, 0.7f, 0.15f), 15);
                Part("Hazard mark", intact, new Vector2(0f, 0.52f), new Vector2(0.1f, 0.1f), _ink, 16, 45f);
                Part("Wreck", broken, new Vector2(0f, 0.2f), new Vector2(0.8f, 0.4f), new Color(0.1f, 0.07f, 0.06f), 14);
                Part("Torn lip", broken, new Vector2(0.2f, 0.42f), new Vector2(0.3f, 0.08f), new Color(0.2f, 0.06f, 0.05f), 15, 30f);
            });
            Prop(PropKind.Plant, new Vector2(0f, 0.45f), new Vector2(0.52f, 0.9f), (root, body, intact, broken) =>
            {
                Part("Pot", intact, new Vector2(0f, 0.2f), new Vector2(0.4f, 0.4f), new Color(0.55f, 0.28f, 0.18f), -8);
                for (int i = 0; i < 5; i++)
                    Part("Leaf " + i, intact, new Vector2(-0.16f + i * 0.08f, 0.62f + (i % 2) * 0.12f), new Vector2(0.1f, 0.42f), new Color(0.12f, 0.3f, 0.16f), -9, -30f + i * 15f);
                Part("Soil", broken, new Vector2(0f, 0.05f), new Vector2(0.6f, 0.1f), new Color(0.2f, 0.13f, 0.08f), -8);
                Part("Shard", broken, new Vector2(0.15f, 0.08f), new Vector2(0.15f, 0.12f), new Color(0.55f, 0.28f, 0.18f), -7, 25f);
                Part("Stalk", broken, new Vector2(-0.1f, 0.1f), new Vector2(0.35f, 0.06f), new Color(0.12f, 0.3f, 0.16f), -7, 10f);
            });

            Effect("PB_FX_CeilingDebris", 1.6f, root =>
            {
                Emitter(root, "Plaster", 10, new Vector2(0.6f, 1.2f), new Vector2(0.8f, 2.8f), new Vector2(0.05f, 0.13f), new Color(0.6f, 0.57f, 0.52f), new Color(0.4f, 0.37f, 0.34f), 2.2f, 35f, false, 0.4f, 1f, 44);
                Emitter(root, "Dust", 6, new Vector2(0.9f, 1.5f), new Vector2(0.3f, 0.9f), new Vector2(0.18f, 0.26f), new Color(0.6f, 0.58f, 0.55f, 0.3f), new Color(0.5f, 0.48f, 0.45f, 0.2f), 0.15f, 45f, false, 2f, 3f, 43);
            });
            Effect("PB_FX_FloorDust", 1.2f, root =>
            {
                Emitter(root, "Chips", 8, new Vector2(0.3f, 0.6f), new Vector2(1.5f, 4f), new Vector2(0.03f, 0.06f), new Color(0.35f, 0.3f, 0.25f), new Color(0.25f, 0.2f, 0.16f), 1.8f, 60f, false, 1f, 1f, 44);
                Emitter(root, "Dust", 5, new Vector2(0.6f, 1.1f), new Vector2(0.3f, 1f), new Vector2(0.14f, 0.2f), new Color(0.5f, 0.48f, 0.45f, 0.3f), new Color(0.45f, 0.42f, 0.4f, 0.2f), -0.05f, 60f, false, 2f, 3f, 43);
            });
            Effect("PB_FX_Splinters", 1.2f, root =>
            {
                Emitter(root, "Splinters", 12, new Vector2(0.4f, 0.8f), new Vector2(2f, 6f), new Vector2(0.04f, 0.09f), new Color(0.55f, 0.38f, 0.22f), new Color(0.35f, 0.22f, 0.12f), 1.6f, 60f, true, 1f, 1f, 45);
                Emitter(root, "Sawdust", 5, new Vector2(0.5f, 0.9f), new Vector2(0.3f, 1f), new Vector2(0.1f, 0.16f), new Color(0.6f, 0.5f, 0.35f, 0.3f), new Color(0.5f, 0.4f, 0.3f, 0.2f), 0.2f, 50f, false, 2f, 2.5f, 44);
            });
            Effect("PB_FX_Glass", 1.4f, root =>
            {
                Emitter(root, "Shards", 16, new Vector2(0.5f, 1f), new Vector2(1.5f, 5f), new Vector2(0.03f, 0.08f), new Color(0.85f, 0.95f, 1f, 0.9f), new Color(0.5f, 0.7f, 0.6f, 0.8f), 1.8f, 80f, false, 0.6f, 1f, 45);
                Emitter(root, "Glints", 6, new Vector2(0.1f, 0.2f), new Vector2(2f, 5f), new Vector2(0.03f, 0.04f), Color.white, new Color(1f, 0.9f, 0.7f), 0f, 70f, true, 5f, 1f, 46);
                Audio(root.gameObject, "SND_SFX_Glass_01", 0.5f);
            });
            Effect("PB_FX_MetalSparks", 1f, root =>
            {
                Emitter(root, "Sparks", 18, new Vector2(0.15f, 0.4f), new Vector2(4f, 11f), new Vector2(0.03f, 0.05f), new Color(1f, 0.95f, 0.6f), new Color(1f, 0.55f, 0.2f), 1f, 70f, true, 2.5f, 1f, 46);
                Emitter(root, "Smoke", 4, new Vector2(0.6f, 1f), new Vector2(0.2f, 0.6f), new Vector2(0.12f, 0.18f), new Color(0.3f, 0.3f, 0.3f, 0.3f), new Color(0.2f, 0.2f, 0.2f, 0.2f), -0.1f, 40f, false, 2f, 3f, 44);
                Audio(root.gameObject, "SND_SFX_Ricochet_01", 0.4f);
            });
            Effect("PB_FX_Dirt", 1.3f, root =>
            {
                Emitter(root, "Soil", 16, new Vector2(0.5f, 1f), new Vector2(1.5f, 5f), new Vector2(0.04f, 0.1f), new Color(0.25f, 0.16f, 0.1f), new Color(0.15f, 0.1f, 0.06f), 2f, 75f, false, 0.6f, 1f, 45);
                Emitter(root, "Shards", 6, new Vector2(0.5f, 0.9f), new Vector2(2f, 4f), new Vector2(0.05f, 0.1f), new Color(0.55f, 0.28f, 0.18f), new Color(0.45f, 0.22f, 0.14f), 2f, 70f, false, 0.6f, 1f, 45);
                Audio(root.gameObject, "SND_SFX_Glass_01", 0.3f);
            });
            Effect("PB_FX_Explosion", 2.6f, root =>
            {
                Emitter(root, "Fireball", 1, new Vector2(0.3f, 0.4f), Vector2.zero, new Vector2(2.2f, 2.6f), new Color(1f, 0.85f, 0.5f), new Color(1f, 0.5f, 0.15f), 0f, 0f, false, 0f, 2f, 47);
                ParticleSystem flames = Emitter(root, "Flames", 24, new Vector2(0.3f, 0.7f), new Vector2(3f, 8f), new Vector2(0.2f, 0.45f), new Color(1f, 0.6f, 0.2f), new Color(0.85f, 0.2f, 0.08f), -0.3f, 0f, false, 4f, 2f, 46);
                ParticleSystem.ShapeModule round = flames.shape;
                round.enabled = true;
                round.shapeType = ParticleSystemShapeType.Circle;
                round.radius = 0.3f;
                ParticleSystem smoke = Emitter(root, "Smoke", 14, new Vector2(1.2f, 2.3f), new Vector2(1f, 3f), new Vector2(0.4f, 0.7f), new Color(0.12f, 0.1f, 0.1f, 0.6f), new Color(0.25f, 0.23f, 0.22f, 0.4f), -0.15f, 0f, false, 1.5f, 3f, 45);
                ParticleSystem.ShapeModule puff = smoke.shape;
                puff.enabled = true;
                puff.shapeType = ParticleSystemShapeType.Circle;
                puff.radius = 0.5f;
                ParticleSystem debris = Emitter(root, "Debris", 14, new Vector2(0.8f, 1.4f), new Vector2(4f, 10f), new Vector2(0.06f, 0.14f), new Color(0.15f, 0.1f, 0.08f), _steel, 2.2f, 0f, false, 0.5f, 1f, 46);
                ParticleSystem.ShapeModule burst = debris.shape;
                burst.enabled = true;
                burst.shapeType = ParticleSystemShapeType.Circle;
                burst.radius = 0.2f;
                Audio(root.gameObject, "SND_SFX_Explosion_01", 0.9f);
            });

            string scorch = Prefabs + "/Decals/PB_Decal_Scorch.prefab";
            if (ShouldWrite(scorch))
            {
                GameObject go = Node("PB_Decal_Scorch", null, Vector2.zero);
                var view = go.AddComponent<DecalView>();
                Part("Char", go.transform, Vector2.zero, new Vector2(2.6f, 1.8f), new Color(0.02f, 0.02f, 0.02f, 0.75f), -2).sprite = _soft;
                Part("Soot", go.transform, new Vector2(0f, 0.6f), new Vector2(2.2f, 2.4f), new Color(0.05f, 0.04f, 0.04f, 0.4f), -2).sprite = _soft;
                var serialized = new SerializedObject(view);
                serialized.FindProperty("_randomRotation").boolValue = false;
                serialized.FindProperty("_floorSquash").floatValue = 1f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                SavePrefab<GameObject>(go, scorch);
            }
        }

        // A prop prefab: the marker on the floor, a Body that a lamp drops with, and its intact and broken looks.
        private void Prop(PropKind kind, Vector2 hit, Vector2 size, System.Action<Transform, Transform, Transform, Transform> draw)
        {
            string path = PropPath(kind);
            if (!ShouldWrite(path)) return;
            GameObject root = Node("PB_Prop_" + kind, null, Vector2.zero);
            var view = root.AddComponent<PropView>();
            view.Kind = kind;
            view.HitCenter = hit;
            view.HitSize = size;
            Transform body = Node("Body", root.transform, kind == PropKind.CeilingLamp ? hit : Vector2.zero).transform;
            GameObject intact = Node("Intact", body, Vector2.zero);
            GameObject broken = Node("Broken", body, Vector2.zero);
            draw(root.transform, body, intact.transform, broken.transform);
            broken.SetActive(false);
            Assign(view, "_body", body);
            Assign(view, "_intact", intact);
            Assign(view, "_broken", broken);
            SavePrefab<GameObject>(root, path);
        }
    }
}
#endif
