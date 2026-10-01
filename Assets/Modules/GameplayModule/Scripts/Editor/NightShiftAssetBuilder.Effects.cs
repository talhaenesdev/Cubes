#if UNITY_EDITOR
using Modules.GameplayModule.Entities;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    public partial class NightShiftAssetBuilder
    {
        private readonly Color _smoke = new(0.62f, 0.66f, 0.7f, 0.35f);
        private readonly Color _blood = new(0.6f, 0.04f, 0.07f);
        private readonly Color _brass = new(0.95f, 0.72f, 0.3f);

        // ---------------------------------------------------------------- bullet

        private void BuildBullet()
        {
            string path = Prefabs + "/Projectiles/PB_Bullet.prefab";
            if (!ShouldWrite(path)) return;
            GameObject go = Node("PB_Bullet", null, Vector2.zero);
            var bullet = go.AddComponent<BulletView>();
            SpriteRenderer glow = Part("Glow", go.transform, new Vector2(-0.15f, 0f), new Vector2(0.45f, 0.1f), new Color(1f, 0.7f, 0.3f, 0.35f), 47);
            SpriteRenderer body = Part("Slug", go.transform, new Vector2(-0.05f, 0f), new Vector2(0.2f, 0.045f), new Color(1f, 0.96f, 0.82f), 48);
            GameObject trailObject = Node("Trail", go.transform, Vector2.zero);
            var trail = trailObject.AddComponent<TrailRenderer>();
            trail.time = 0.07f;
            trail.minVertexDistance = 0.05f;
            trail.widthCurve = new AnimationCurve(new Keyframe(0f, 0.07f), new Keyframe(1f, 0f));
            trail.startColor = new Color(1f, 0.85f, 0.55f, 0.8f);
            trail.endColor = new Color(1f, 0.5f, 0.2f, 0f);
            trail.sharedMaterial = _particle;
            trail.sortingOrder = 46;
            trail.emitting = false;
            glow.gameObject.name = "Glow";
            Assign(bullet, "_trail", trail);
            Assign(bullet, "_body", body);
            SavePrefab<GameObject>(go, path);
        }

        // ---------------------------------------------------------------- effects

        private void BuildEffects()
        {
            Effect("PB_FX_Muzzle", 1.8f, root =>
            {
                Emitter(root, "Flash", 1, new Vector2(0.05f, 0.07f), Vector2.zero, new Vector2(0.5f, 0.7f), new Color(1f, 0.95f, 0.7f), new Color(1f, 0.7f, 0.35f), 0f, 0f, false, 0f, 1f, 43);
                Emitter(root, "Sparks", 14, new Vector2(0.12f, 0.2f), new Vector2(6f, 14f), new Vector2(0.04f, 0.06f), new Color(1f, 0.95f, 0.5f), new Color(1f, 0.55f, 0.25f), 0f, 20f, true, 6f, 1f, 45);
                Emitter(root, "Smoke", 7, new Vector2(0.6f, 1.1f), new Vector2(0.6f, 2f), new Vector2(0.12f, 0.16f), _smoke, _smoke, -0.05f, 35f, false, 2.5f, 4f, 44);
                Casing(root);
                Audio(root.gameObject, "SND_SFX_Shot_01", 0.65f);
            });
            Effect("PB_FX_GuardMuzzle", 1.5f, root =>
            {
                Emitter(root, "Flash", 1, new Vector2(0.05f, 0.07f), Vector2.zero, new Vector2(0.45f, 0.6f), new Color(1f, 0.8f, 0.6f), new Color(1f, 0.4f, 0.3f), 0f, 0f, false, 0f, 1f, 43);
                Emitter(root, "Sparks", 10, new Vector2(0.1f, 0.18f), new Vector2(5f, 12f), new Vector2(0.04f, 0.06f), new Color(1f, 0.6f, 0.4f), new Color(1f, 0.35f, 0.3f), 0f, 20f, true, 6f, 1f, 45);
                Emitter(root, "Smoke", 5, new Vector2(0.5f, 0.9f), new Vector2(0.6f, 1.6f), new Vector2(0.12f, 0.15f), _smoke, _smoke, -0.05f, 35f, false, 2.5f, 4f, 44);
                Audio(root.gameObject, "SND_SFX_GuardShot_01", 0.45f);
            });
            Effect("PB_FX_ImpactWall", 1.2f, root =>
            {
                Emitter(root, "Sparks", 12, new Vector2(0.12f, 0.3f), new Vector2(3f, 9f), new Vector2(0.03f, 0.05f), new Color(1f, 0.95f, 0.6f), new Color(1f, 0.6f, 0.4f), 1.2f, 60f, true, 3f, 1f, 45);
                Emitter(root, "Dust", 6, new Vector2(0.5f, 1f), new Vector2(0.4f, 1.6f), new Vector2(0.1f, 0.14f), new Color(0.55f, 0.55f, 0.52f, 0.4f), new Color(0.45f, 0.45f, 0.42f, 0.3f), 0.12f, 50f, false, 2f, 3f, 44);
            });
            Effect("PB_FX_ImpactBlood", 1.2f, root =>
            {
                Emitter(root, "Droplets", 16, new Vector2(0.4f, 0.9f), new Vector2(1.5f, 6f), new Vector2(0.05f, 0.11f), _blood, new Color(0.45f, 0.03f, 0.06f), 1.4f, 50f, false, 0.5f, 1f, 44);
                Emitter(root, "Mist", 4, new Vector2(0.3f, 0.5f), new Vector2(0.3f, 1f), new Vector2(0.2f, 0.3f), new Color(0.6f, 0.05f, 0.08f, 0.3f), new Color(0.5f, 0.04f, 0.06f, 0.2f), 0f, 40f, false, 3f, 3f, 43);
            });
            Effect("PB_FX_ShellLoad", 0.5f, root =>
            {
                ParticleSystem sparks = Emitter(root, "Glints", 4, new Vector2(0.15f, 0.25f), new Vector2(1f, 1.5f), new Vector2(0.04f, 0.05f), _amber, _brass, 0f, 0f, false, 3f, 1f, 34);
                ParticleSystem.ShapeModule shape = sparks.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                Audio(root.gameObject, "SND_SFX_ShellLoad_01", 0.38f);
            });
            Effect("PB_FX_Melee", 0.6f, root =>
            {
                ParticleSystem puff = Emitter(root, "Puff", 8, new Vector2(0.15f, 0.3f), new Vector2(1.5f, 3f), new Vector2(0.06f, 0.1f), new Color(1f, 1f, 1f, 0.7f), new Color(0.8f, 0.85f, 0.9f, 0.4f), 0f, 0f, false, 5f, 2f, 44);
                ParticleSystem.ShapeModule shape = puff.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                Audio(root.gameObject, "SND_SFX_Melee_01", 0.42f);
            });
            Effect("PB_FX_DoorKick", 1.2f, root =>
            {
                Emitter(root, "Dust", 10, new Vector2(0.5f, 1f), new Vector2(0.5f, 2f), new Vector2(0.15f, 0.25f), new Color(0.5f, 0.5f, 0.48f, 0.35f), new Color(0.4f, 0.4f, 0.38f, 0.25f), 0.05f, 70f, false, 2.5f, 3f, 44);
                Emitter(root, "Splinters", 8, new Vector2(0.4f, 0.8f), new Vector2(2f, 5f), new Vector2(0.04f, 0.08f), new Color(0.45f, 0.32f, 0.2f), _steel, 1.5f, 60f, false, 1f, 1f, 45);
                Audio(root.gameObject, "SND_SFX_DoorKick_01", 0.5f);
            });
        }

        private void Effect(string name, float duration, System.Action<Transform> fill)
        {
            string path = Prefabs + "/FX/" + name + ".prefab";
            if (!ShouldWrite(path)) return;
            GameObject go = Node(name, null, Vector2.zero);
            var effect = go.AddComponent<PooledEffect>();
            var serialized = new SerializedObject(effect);
            serialized.FindProperty("_duration").floatValue = duration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            fill(go.transform);
            SavePrefab<GameObject>(go, path);
        }

        // One burst along the effect's +X. Every number is the prefab's to retune afterwards.
        private ParticleSystem Emitter(Transform parent, string name, int count, Vector2 life, Vector2 speed, Vector2 size,
            Color from, Color to, float gravity, float cone, bool stretch, float drag, float grow, int order)
        {
            GameObject go = Node(name, parent, Vector2.zero);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = system.main;
            main.duration = 0.5f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startColor = new ParticleSystem.MinMaxGradient(from, to);
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = Mathf.Max(4, count * 2);
            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] {new ParticleSystem.Burst(0f, (short) count)});
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = cone > 0f;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = Mathf.Min(cone, 89f);
            shape.radius = 0.02f;
            shape.rotation = new Vector3(0f, 90f, 0f);
            ParticleSystem.ColorOverLifetimeModule fade = system.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] {new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)},
                new[] {new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f)});
            fade.color = new ParticleSystem.MinMaxGradient(gradient);
            if (drag > 0f)
            {
                ParticleSystem.LimitVelocityOverLifetimeModule limit = system.limitVelocityOverLifetime;
                limit.enabled = true;
                limit.limit = new ParticleSystem.MinMaxCurve(100f);
                limit.drag = new ParticleSystem.MinMaxCurve(drag);
                limit.multiplyDragByParticleSize = false;
                limit.multiplyDragByParticleVelocity = false;
            }
            if (!Mathf.Approximately(grow, 1f))
            {
                ParticleSystem.SizeOverLifetimeModule growth = system.sizeOverLifetime;
                growth.enabled = true;
                growth.size = new ParticleSystem.MinMaxCurve(grow, new AnimationCurve(new Keyframe(0f, 1f / grow), new Keyframe(1f, 1f)));
            }
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = _particle;
            renderer.sortingOrder = order;
            renderer.renderMode = stretch ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            if (stretch)
            {
                renderer.velocityScale = 0.03f;
                renderer.lengthScale = 1f;
            }
            return system;
        }

        // The spent casing kicks out backwards and up, then falls; the effect's flip keeps "up" up when aiming left.
        private void Casing(Transform parent)
        {
            ParticleSystem casing = Emitter(parent, "Casing", 1, new Vector2(1.2f, 1.6f), Vector2.zero, new Vector2(0.07f, 0.07f), _brass, _brass, 1.6f, 0f, false, 0f, 1f, 14);
            casing.transform.localPosition = new Vector3(-0.55f, 0f, 0f);
            ParticleSystem.VelocityOverLifetimeModule velocity = casing.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-2.6f, -1.5f);
            velocity.y = new ParticleSystem.MinMaxCurve(3f, 4.5f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            ParticleSystem.RotationOverLifetimeModule spin = casing.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(10f, 18f);
            var renderer = casing.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/MT_FX_Particle.mat");
        }

        // ---------------------------------------------------------------- decals

        private void BuildDecals()
        {
            string hole = Prefabs + "/Decals/PB_Decal_BulletHole.prefab";
            if (ShouldWrite(hole))
            {
                GameObject go = Node("PB_Decal_BulletHole", null, Vector2.zero);
                go.AddComponent<DecalView>();
                Part("Scorch", go.transform, Vector2.zero, new Vector2(0.22f, 0.22f), new Color(0f, 0f, 0f, 0.25f), 24, 45f);
                Part("Hole", go.transform, Vector2.zero, new Vector2(0.09f, 0.09f), new Color(0.01f, 0.01f, 0.015f, 0.95f), 25);
                for (int i = 0; i < 3; i++)
                {
                    float a = (40f + i * 125f) * Mathf.Deg2Rad;
                    Line("Crack " + i, go.transform, Vector2.zero, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (0.14f + i * 0.04f), 0.02f,
                        new Color(0.02f, 0.02f, 0.03f, 0.7f), 24);
                }
                SavePrefab<GameObject>(go, hole);
            }
            string blood = Prefabs + "/Decals/PB_Decal_Blood.prefab";
            if (ShouldWrite(blood))
            {
                GameObject go = Node("PB_Decal_Blood", null, Vector2.zero);
                var view = go.AddComponent<DecalView>();
                var random = new System.Random(71);
                for (int i = 0; i < 6; i++)
                {
                    float size = 0.08f + (float) random.NextDouble() * 0.2f;
                    Part("Blot " + i, go.transform, new Vector2((float) random.NextDouble() * 0.9f - 0.45f, (float) random.NextDouble() * 0.3f - 0.15f),
                        new Vector2(size, size * 0.6f), new Color(0.32f + (float) random.NextDouble() * 0.15f, 0.02f, 0.05f, 0.85f), -2);
                }
                Part("Drip", go.transform, new Vector2(0f, -0.3f), new Vector2(0.05f, 0.6f), new Color(0.35f, 0.02f, 0.05f, 0.7f), -2);
                var serialized = new SerializedObject(view);
                serialized.FindProperty("_randomRotation").boolValue = false;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                SavePrefab<GameObject>(go, blood);
            }
        }
    }
}
#endif
