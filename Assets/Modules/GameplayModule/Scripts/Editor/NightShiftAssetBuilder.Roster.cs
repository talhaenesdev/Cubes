#if UNITY_EDITOR
using System.Collections.Generic;
using FlowIoC.BaseModule.Adapters;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The cast and the arsenal: shape characters, weapons, the pickup, and the two configs that list them.
    public partial class NightShiftAssetBuilder
    {
        public const string WeaponsAsset = ModuleRoot + "/Scriptables/CD_Weapons.asset";
        public const string CharactersAsset = ModuleRoot + "/Scriptables/CD_Characters.asset";
        private const string Pickup = Prefabs + "/Weapons/PB_WeaponPickup.prefab";
        private readonly Color _wood = new(0.42f, 0.27f, 0.16f);
        private readonly Dictionary<CharacterShape, Sprite> _shapes = new();

        private static readonly string[] Retired =
        {
            Prefabs + "/Characters/PB_Player.prefab", Prefabs + "/Characters/PB_Guard.prefab", Prefabs + "/Weapons/PB_Revolver.prefab",
        };

        private string WeaponPath(string id) => Prefabs + "/Weapons/PB_Weapon_" + Pascal(id) + ".prefab";
        private string CharacterPath(CharacterShape shape) => Prefabs + "/Characters/PB_Character_" + shape + ".prefab";

        private string Pascal(string id)
        {
            var parts = id.Split('_');
            for (int i = 0; i < parts.Length; i++)
                if (parts[i].Length > 0) parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            return string.Join("", parts);
        }

        // ---------------------------------------------------------------- shapes

        private void BuildShapes()
        {
            foreach (CharacterShape shape in System.Enum.GetValues(typeof(CharacterShape)))
            {
                CharacterShape s = shape;
                _shapes[shape] = SpriteAsset(Art + "/Textures/SPR_Shape_" + shape + ".png", 64, FilterMode.Bilinear,
                    (x, y, size) => Inside(s, (x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f) ? Color.white : Color.clear);
            }
        }

        private bool Inside(CharacterShape shape, float x, float y)
        {
            switch (shape)
            {
                case CharacterShape.Square: return Mathf.Abs(x) < 0.94f && Mathf.Abs(y) < 0.94f;
                case CharacterShape.Circle: return x * x + y * y < 0.94f * 0.94f;
                case CharacterShape.Diamond: return Mathf.Abs(x) * 1.25f + Mathf.Abs(y) < 0.98f;
                case CharacterShape.Triangle: return Polygon(3, x, y);
                case CharacterShape.Pentagon: return Polygon(5, x, y);
                default: return Polygon(6, x, y);
            }
        }

        // A regular polygon with a vertex at the top, filling the unit circle.
        private bool Polygon(int sides, float x, float y)
        {
            float radius = 0.97f;
            for (int i = 0; i < sides; i++)
            {
                float a0 = Mathf.PI / 2f + i * 2f * Mathf.PI / sides;
                float a1 = a0 + 2f * Mathf.PI / sides;
                Vector2 p0 = new(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius);
                Vector2 p1 = new(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius);
                if ((p1.x - p0.x) * (y - p0.y) - (p1.y - p0.y) * (x - p0.x) < 0f) return false;
            }
            return true;
        }

        // ---------------------------------------------------------------- weapons

        private void BuildWeapons()
        {
            Weapon("revolver", 0.43f, gun =>
            {
                Part("Grip", gun, new Vector2(-0.04f, -0.07f), new Vector2(0.09f, 0.16f), _wood, 11, 15f);
                Part("Barrel", gun, new Vector2(0.21f, 0f), new Vector2(0.42f, 0.13f), _ink, 11);
                Part("Rib", gun, new Vector2(0.2f, 0.045f), new Vector2(0.37f, 0.035f), _steel, 12);
                Part("Cylinder", gun, new Vector2(0.03f, 0f), new Vector2(0.14f, 0.14f), _steel, 12);
            });
            Weapon("pistol", 0.37f, gun =>
            {
                Part("Grip", gun, new Vector2(-0.02f, -0.08f), new Vector2(0.09f, 0.16f), _ink, 11, 10f);
                Part("Slide", gun, new Vector2(0.17f, 0.02f), new Vector2(0.36f, 0.1f), _ink, 11);
                Part("Top", gun, new Vector2(0.17f, 0.06f), new Vector2(0.32f, 0.025f), _steel, 12);
            });
            Weapon("silenced_pistol", 0.66f, gun =>
            {
                Part("Grip", gun, new Vector2(-0.02f, -0.08f), new Vector2(0.09f, 0.16f), _ink, 11, 10f);
                Part("Slide", gun, new Vector2(0.17f, 0.02f), new Vector2(0.36f, 0.1f), _ink, 11);
                Part("Suppressor", gun, new Vector2(0.5f, 0.02f), new Vector2(0.32f, 0.08f), _steel, 12);
            });
            Weapon("shotgun", 0.71f, gun =>
            {
                Part("Stock", gun, new Vector2(-0.18f, -0.03f), new Vector2(0.32f, 0.11f), _wood, 11, -8f);
                Part("Receiver", gun, new Vector2(0.06f, 0f), new Vector2(0.2f, 0.11f), _ink, 11);
                Part("Barrel", gun, new Vector2(0.4f, 0.03f), new Vector2(0.62f, 0.06f), _ink, 11);
                Part("Pump", gun, new Vector2(0.32f, -0.03f), new Vector2(0.24f, 0.06f), _wood, 12);
            });
            Weapon("smg", 0.52f, gun =>
            {
                Part("Body", gun, new Vector2(0.15f, 0f), new Vector2(0.4f, 0.13f), _ink, 11);
                Part("Magazine", gun, new Vector2(0.12f, -0.14f), new Vector2(0.07f, 0.2f), _steel, 12, -6f);
                Part("Barrel", gun, new Vector2(0.43f, 0.02f), new Vector2(0.16f, 0.05f), _ink, 11);
                Part("Grip", gun, new Vector2(-0.02f, -0.08f), new Vector2(0.08f, 0.14f), _ink, 11, 10f);
            });
            Weapon("rifle", 0.92f, gun =>
            {
                Part("Stock", gun, new Vector2(-0.2f, -0.02f), new Vector2(0.36f, 0.11f), _wood, 11, -5f);
                Part("Body", gun, new Vector2(0.12f, 0.01f), new Vector2(0.34f, 0.1f), _ink, 11);
                Part("Barrel", gun, new Vector2(0.6f, 0.02f), new Vector2(0.66f, 0.05f), _ink, 11);
                Part("Scope", gun, new Vector2(0.2f, 0.1f), new Vector2(0.3f, 0.06f), _steel, 12);
            });
            if (ShouldWrite(Pickup))
            {
                GameObject pickup = Node("PB_WeaponPickup", null, Vector2.zero);
                var view = pickup.AddComponent<WeaponPickupView>();
                Part("Glint", pickup.transform, new Vector2(0f, 0.04f), new Vector2(0.9f, 0.05f), new Color(1f, 0.8f, 0.4f, 0.55f), -3);
                Part("Shadow", pickup.transform, new Vector2(0f, 0.02f), new Vector2(0.7f, 0.04f), new Color(0f, 0f, 0f, 0.4f), -4);
                Transform holder = Node("Holder", pickup.transform, new Vector2(0f, 0.18f)).transform;
                Assign(view, "_holder", holder);
                SavePrefab<GameObject>(pickup, Pickup);
            }
        }

        private void Weapon(string id, float muzzle, System.Action<Transform> draw)
        {
            string path = WeaponPath(id);
            if (!ShouldWrite(path)) return;
            GameObject gun = Node("PB_Weapon_" + Pascal(id), null, Vector2.zero);
            draw(gun.transform);
            Node("Muzzle", gun.transform, new Vector2(muzzle, 0.02f));
            SavePrefab<GameObject>(gun, path);
        }

        // ---------------------------------------------------------------- characters

        private void BuildShapeCharacters()
        {
            ShapeCharacter(CharacterShape.Square, new Color(0.17f, 0.18f, 0.22f));
            ShapeCharacter(CharacterShape.Triangle, new Color(0.42f, 0.13f, 0.15f));
            ShapeCharacter(CharacterShape.Pentagon, new Color(0.3f, 0.21f, 0.36f));
            ShapeCharacter(CharacterShape.Hexagon, new Color(0.4f, 0.29f, 0.18f));
            ShapeCharacter(CharacterShape.Circle, new Color(0.55f, 0.5f, 0.38f));
            ShapeCharacter(CharacterShape.Diamond, new Color(0.2f, 0.28f, 0.36f));
        }

        private void ShapeCharacter(CharacterShape shape, Color color)
        {
            string path = CharacterPath(shape);
            if (!ShouldWrite(path) && !NeedsRestyle(path)) return;
            if (NeedsRestyle(path)) _report.Add(path + " was from an earlier style - rebuilt");
            Sprite sprite = _shapes[shape];
            GameObject root = Node("PB_Character_" + shape, null, Vector2.zero);
            var rig = root.AddComponent<CharacterRig>();
            var coat = new List<Object>();
            Color dark = color * new Color(0.55f, 0.55f, 0.55f, 1f);

            SpriteRenderer shadow = Part("Shadow", root.transform, new Vector2(0f, 0.03f), new Vector2(1.1f, 0.2f), new Color(0f, 0f, 0f, 0.5f), -1);
            shadow.sprite = _soft;
            SpriteRenderer blood = Part("BloodPool", root.transform, new Vector2(1.25f, 0.025f), new Vector2(1.3f, 0.045f), new Color(0.29f, 0.07f, 0.10f), 0);
            blood.gameObject.SetActive(false);

            Transform pose = Node("Pose", root.transform, Vector2.zero).transform;
            Transform body = Node("Body", pose, Vector2.zero).transform;
            Sprite round = _shapes[CharacterShape.Circle];
            Color light = Color.Lerp(color, Color.white, 0.35f);
            bool friendly = shape == CharacterShape.Square || shape == CharacterShape.Circle;

            Transform legBack = Node("LegBack", body, new Vector2(-0.16f, 0.42f)).transform;
            Part("Leg", legBack, new Vector2(0f, -0.2f), new Vector2(0.12f, 0.4f), _ink, 2);
            Part("Foot", legBack, new Vector2(0.05f, -0.39f), new Vector2(0.24f, 0.1f), _ink, 2).sprite = round;
            Transform legFront = Node("LegFront", body, new Vector2(0.16f, 0.42f)).transform;
            Part("Leg", legFront, new Vector2(0f, -0.2f), new Vector2(0.12f, 0.4f), dark, 3);
            Part("Foot", legFront, new Vector2(0.05f, -0.39f), new Vector2(0.24f, 0.1f), _ink, 3).sprite = round;

            // Body: ink outline, a shaded back layer, the lit face of the shape and a soft highlight.
            Transform torso = Node("Torso", body, new Vector2(0f, 0.97f)).transform;
            Part("Outline", torso, Vector2.zero, new Vector2(1.12f, 1.12f), _ink, 4).sprite = sprite;
            Part("Shade", torso, new Vector2(0.03f, -0.04f), new Vector2(0.97f, 0.97f), dark, 5).sprite = sprite;
            SpriteRenderer fill = Part("Shape", torso, new Vector2(-0.015f, 0.02f), new Vector2(0.9f, 0.9f), color, 6);
            fill.sprite = sprite;
            coat.Add(fill);
            SpriteRenderer shine = Part("Shine", torso, new Vector2(-0.2f, 0.24f), new Vector2(0.34f, 0.2f), new Color(1f, 1f, 1f, 0.18f), 7, 20f);
            shine.sprite = _soft;
            SpriteRenderer rim = Part("Rim", torso, new Vector2(0.28f, -0.2f), new Vector2(0.2f, 0.34f), new Color(light.r, light.g, light.b, 0.2f), 7, -20f);
            rim.sprite = _soft;

            if (shape == CharacterShape.Square) Reaper(torso, sprite);
            else Face(torso, round, friendly);
            Suit(shape, torso);
            Accessory(shape, torso, color, dark);

            Transform arm = Node("ArmPivot", body, new Vector2(0.05f, 0.85f)).transform;
            coat.Add(Part("Arm", arm, new Vector2(0.2f, 0f), new Vector2(0.36f, 0.12f), color, 11));
            Part("Hand", arm, new Vector2(0.4f, 0f), new Vector2(0.16f, 0.16f), dark, 13).sprite = round;
            Transform socket = Node("WeaponSocket", arm, new Vector2(0.42f, 0.02f)).transform;

            GameObject spotted = Node("Mark Spotted", root.transform, new Vector2(0f, 1.9f));
            Part("Bar", spotted.transform, new Vector2(0f, 0.1f), new Vector2(0.08f, 0.3f), _red, 30);
            Part("Dot", spotted.transform, new Vector2(0f, -0.12f), new Vector2(0.08f, 0.08f), _red, 30);
            spotted.SetActive(false);
            GameObject investigate = Node("Mark Investigate", root.transform, new Vector2(0f, 1.8f));
            Part("Top", investigate.transform, new Vector2(0f, 0.18f), new Vector2(0.22f, 0.07f), _amber, 30);
            Part("Side", investigate.transform, new Vector2(0.1f, 0.08f), new Vector2(0.07f, 0.14f), _amber, 30);
            Part("Neck", investigate.transform, new Vector2(0f, -0.02f), new Vector2(0.07f, 0.1f), _amber, 30);
            Part("Dot", investigate.transform, new Vector2(0f, -0.16f), new Vector2(0.07f, 0.07f), _amber, 30);
            investigate.SetActive(false);
            GameObject stun = Node("Mark Stun", root.transform, new Vector2(0f, 1.65f));
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f * Mathf.Deg2Rad;
                Part("Star", stun.transform, new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.35f) * 0.3f, new Vector2(0.07f, 0.07f), _amber, 30, 45f);
            }
            stun.SetActive(false);
            GameObject eyes = Node("Vent Eyes", root.transform, new Vector2(0f, 0.32f));
            Part("Left", eyes.transform, new Vector2(-0.1f, 0f), new Vector2(0.07f, 0.05f), color, 14);
            Part("Right", eyes.transform, new Vector2(0.1f, 0f), new Vector2(0.07f, 0.05f), color, 14);
            eyes.SetActive(false);
            RevolverCylinder cylinder = Cylinder(root.transform);
            cylinder.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            Assign(rig, "_magazine", MagazineIndicator(root.transform));
            Node(StyleMarker, root.transform, Vector2.zero);

            Assign(rig, "_pose", pose);
            Assign(rig, "_body", body);
            Assign(rig, "_legFront", legFront);
            Assign(rig, "_legBack", legBack);
            Assign(rig, "_torso", torso);
            Assign(rig, "_armPivot", arm);
            Assign(rig, "_weaponSocket", socket);
            AssignArray(rig, "_coat", coat);
            Assign(rig, "_spottedMark", spotted);
            Assign(rig, "_investigateMark", investigate);
            Assign(rig, "_stunMark", stun);
            Assign(rig, "_ventEyes", eyes);
            Assign(rig, "_shadow", shadow.transform);
            Assign(rig, "_bloodPool", blood.transform);
            Assign(rig, "_cylinder", cylinder);
            var serialized = new SerializedObject(rig);
            serialized.FindProperty("_coatColor").colorValue = color;
            serialized.FindProperty("_alertColor").colorValue = Color.Lerp(color, _red, 0.7f);
            serialized.FindProperty("_crouchScale").floatValue = 0.72f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            SavePrefab<GameObject>(root, path);
        }

        private readonly Color _felt = new(0.07f, 0.06f, 0.06f);
        private readonly Color _shirt = new(0.86f, 0.82f, 0.74f);
        private readonly Color _crimson = new(0.55f, 0.07f, 0.1f);
        private readonly Color _hatBrass = new(0.72f, 0.58f, 0.3f);

        // The player: a hood pulled low over the square, and two pale eyes burning in the dark of it.
        private void Reaper(Transform torso, Sprite shape)
        {
            Part("Hood", torso, new Vector2(-0.02f, 0.12f), new Vector2(1.18f, 1.02f), _felt, 7).sprite = shape;
            Part("Hood opening", torso, new Vector2(0.14f, 0.05f), new Vector2(0.62f, 0.52f), new Color(0.02f, 0.02f, 0.03f), 8).sprite = _shapes[CharacterShape.Circle];
            Color glow = new(0.75f, 0.92f, 1f);
            Part("Eye glow", torso, new Vector2(0.18f, 0.08f), new Vector2(0.5f, 0.24f), new Color(0.5f, 0.8f, 1f, 0.35f), 9).sprite = _soft;
            Part("Eye", torso, new Vector2(0.26f, 0.08f), new Vector2(0.1f, 0.06f), glow, 10).sprite = _shapes[CharacterShape.Circle];
            Part("Eye far", torso, new Vector2(0.08f, 0.08f), new Vector2(0.08f, 0.05f), glow, 10).sprite = _shapes[CharacterShape.Circle];
            Part("Hood seam", torso, new Vector2(-0.2f, 0.35f), new Vector2(0.04f, 0.4f), new Color(0.12f, 0.11f, 0.12f), 9, -20f);
        }

        private void Face(Transform torso, Sprite round, bool friendly)
        {
            Part("Eye far", torso, new Vector2(0.02f, 0.1f), new Vector2(0.18f, 0.18f), new Color(0.85f, 0.83f, 0.78f), 8).sprite = round;
            Part("Pupil far", torso, new Vector2(0.06f, 0.09f), new Vector2(0.08f, 0.09f), _ink, 9).sprite = round;
            Part("Eye", torso, new Vector2(0.22f, 0.1f), new Vector2(0.22f, 0.22f), new Color(0.88f, 0.86f, 0.8f), 8).sprite = round;
            Part("Pupil", torso, new Vector2(0.27f, 0.09f), new Vector2(0.1f, 0.11f), _ink, 9).sprite = round;
            if (friendly)
            {
                Part("Brow", torso, new Vector2(0.21f, 0.26f), new Vector2(0.2f, 0.035f), _ink, 10, 6f);
                Part("Mouth", torso, new Vector2(0.18f, -0.12f), new Vector2(0.12f, 0.03f), _ink, 10);
            }
            else
            {
                Part("Brow", torso, new Vector2(0.2f, 0.25f), new Vector2(0.26f, 0.05f), _ink, 10, -22f);
                Part("Brow far", torso, new Vector2(0.03f, 0.23f), new Vector2(0.16f, 0.04f), _ink, 10, -14f);
                Part("Scowl", torso, new Vector2(0.18f, -0.13f), new Vector2(0.16f, 0.035f), _ink, 10, 8f);
                Part("Stubble", torso, new Vector2(0.14f, -0.2f), new Vector2(0.34f, 0.12f), new Color(0f, 0f, 0f, 0.25f), 9).sprite = _soft;
            }
        }

        // A dark jacket's lapels over a white shirt and a tie; the civilian wears shirt sleeves and braces.
        private void Suit(CharacterShape shape, Transform torso)
        {
            if (shape == CharacterShape.Circle)
            {
                Part("Shirt", torso, new Vector2(0.06f, -0.3f), new Vector2(0.5f, 0.3f), _shirt, 10);
                Part("Brace L", torso, new Vector2(-0.08f, -0.28f), new Vector2(0.04f, 0.34f), _ink, 11);
                Part("Brace R", torso, new Vector2(0.2f, -0.28f), new Vector2(0.04f, 0.34f), _ink, 11);
                return;
            }
            Color jacket = shape == CharacterShape.Square ? _felt : new Color(0.1f, 0.09f, 0.1f);
            Part("Jacket", torso, new Vector2(0.04f, -0.33f), new Vector2(0.78f, 0.3f), jacket, 10);
            Part("Shirt", torso, new Vector2(0.1f, -0.26f), new Vector2(0.16f, 0.26f), _shirt, 11, 0f);
            Part("Lapel L", torso, new Vector2(-0.02f, -0.24f), new Vector2(0.08f, 0.3f), jacket, 12, -22f);
            Part("Lapel R", torso, new Vector2(0.22f, -0.24f), new Vector2(0.08f, 0.3f), jacket, 12, 22f);
            Part("Tie", torso, new Vector2(0.1f, -0.3f), new Vector2(0.06f, 0.24f), shape == CharacterShape.Square ? new Color(0.25f, 0.25f, 0.3f) : _crimson, 13);
            Part("Knot", torso, new Vector2(0.1f, -0.16f), new Vector2(0.08f, 0.06f), shape == CharacterShape.Square ? new Color(0.25f, 0.25f, 0.3f) : _crimson, 13);
        }

        // A fedora: crown with its pinch, a band, and a brim tipped over the eyes.
        private void Fedora(Transform torso, Color band, float tilt = -4f)
        {
            Part("Hat crown", torso, new Vector2(0.03f, 0.53f), new Vector2(0.52f, 0.22f), _felt, 14, tilt);
            Part("Hat pinch", torso, new Vector2(0.03f, 0.63f), new Vector2(0.18f, 0.04f), new Color(0.02f, 0.02f, 0.02f), 15, tilt);
            Part("Hat band", torso, new Vector2(0.03f, 0.45f), new Vector2(0.53f, 0.06f), band, 15, tilt);
            Part("Hat brim", torso, new Vector2(0.08f, 0.4f), new Vector2(0.9f, 0.06f), _felt, 15, tilt);
        }

        // One touch per shape on top of the suit, so the cast reads apart at a glance.
        private void Accessory(CharacterShape shape, Transform torso, Color color, Color dark)
        {
            switch (shape)
            {
                case CharacterShape.Square:
                    Part("Scarf", torso, new Vector2(-0.4f, -0.3f), new Vector2(0.1f, 0.36f), _crimson, 14, -25f);
                    break;
                case CharacterShape.Triangle:
                    Fedora(torso, _crimson);
                    Part("Shades", torso, new Vector2(0.16f, 0.1f), new Vector2(0.44f, 0.12f), new Color(0.02f, 0.02f, 0.02f), 11);
                    Part("Shades glint", torso, new Vector2(0.28f, 0.13f), new Vector2(0.08f, 0.025f), new Color(0.8f, 0.8f, 0.8f, 0.8f), 12);
                    break;
                case CharacterShape.Pentagon:
                    Fedora(torso, new Color(0.35f, 0.3f, 0.4f), -8f);
                    Part("Pinstripe", torso, new Vector2(-0.2f, -0.05f), new Vector2(0.02f, 0.55f), new Color(1f, 1f, 1f, 0.12f), 7);
                    Part("Pinstripe 2", torso, new Vector2(-0.05f, -0.05f), new Vector2(0.02f, 0.6f), new Color(1f, 1f, 1f, 0.12f), 7);
                    Part("Pinstripe 3", torso, new Vector2(0.3f, -0.05f), new Vector2(0.02f, 0.5f), new Color(1f, 1f, 1f, 0.12f), 7);
                    break;
                case CharacterShape.Hexagon:
                    Fedora(torso, new Color(0.3f, 0.22f, 0.12f), 0f);
                    Part("Cigar", torso, new Vector2(0.38f, -0.1f), new Vector2(0.24f, 0.06f), new Color(0.35f, 0.22f, 0.12f), 14, -8f);
                    Part("Ember", torso, new Vector2(0.5f, -0.12f), new Vector2(0.05f, 0.06f), new Color(1f, 0.45f, 0.1f), 15);
                    Part("Smoke", torso, new Vector2(0.56f, 0.05f), new Vector2(0.16f, 0.3f), new Color(0.7f, 0.7f, 0.7f, 0.25f), 15).sprite = _soft;
                    Part("Shades", torso, new Vector2(0.16f, 0.1f), new Vector2(0.44f, 0.12f), new Color(0.02f, 0.02f, 0.02f), 11);
                    break;
                case CharacterShape.Diamond:
                    Fedora(torso, _hatBrass, -2f);
                    Part("Monocle", torso, new Vector2(0.22f, 0.1f), new Vector2(0.3f, 0.3f), _hatBrass, 7).sprite = _shapes[CharacterShape.Circle];
                    Part("Monocle chain", torso, new Vector2(0.26f, -0.1f), new Vector2(0.02f, 0.28f), _hatBrass, 11, 10f);
                    break;
                case CharacterShape.Circle:
                    Part("Flat cap", torso, new Vector2(0.02f, 0.48f), new Vector2(0.52f, 0.16f), new Color(0.3f, 0.25f, 0.18f), 14);
                    Part("Cap peak", torso, new Vector2(0.24f, 0.41f), new Vector2(0.36f, 0.05f), new Color(0.25f, 0.2f, 0.14f), 14, -6f);
                    break;
            }
        }

        private const string StyleMarker = "Style Mafia";

        // The whole-magazine reload: an empty magazine drops from the grip, a full one rises in, a bar fills.
        private MagazineSwap MagazineIndicator(Transform parent)
        {
            GameObject go = Node("Magazine Swap", parent, new Vector2(0f, 2.15f));
            var swap = go.AddComponent<MagazineSwap>();
            Part("Backing", go.transform, Vector2.zero, new Vector2(0.62f, 0.62f), new Color(0.02f, 0.03f, 0.05f, 0.75f), 30, 45f);
            Part("Grip", go.transform, new Vector2(0.05f, 0.08f), new Vector2(0.16f, 0.32f), _ink, 32, 10f);
            Part("Slide", go.transform, new Vector2(0.18f, 0.22f), new Vector2(0.4f, 0.1f), _ink, 32);
            GameObject empty = Node("Empty", go.transform, new Vector2(0.05f, -0.12f));
            SpriteRenderer emptyBody = Part("Body", empty.transform, Vector2.zero, new Vector2(0.11f, 0.24f), _steel, 31, 10f);
            GameObject full = Node("Full", go.transform, new Vector2(0.05f, -0.12f));
            Part("Body", full.transform, Vector2.zero, new Vector2(0.11f, 0.24f), _steel, 31, 10f);
            Part("Rounds", full.transform, new Vector2(0.01f, 0.1f), new Vector2(0.08f, 0.05f), _amber, 33, 10f);
            Part("Bar back", go.transform, new Vector2(0f, -0.42f), new Vector2(0.5f, 0.05f), _ink, 31);
            GameObject bar = Node("Bar", go.transform, new Vector2(-0.25f, -0.42f));
            Part("Fill", bar.transform, new Vector2(0.25f, 0f), new Vector2(0.5f, 0.04f), _amber, 32);
            Assign(swap, "_empty", empty.transform);
            Assign(swap, "_emptyRenderer", emptyBody);
            Assign(swap, "_full", full.transform);
            Assign(swap, "_bar", bar.transform);
            go.SetActive(false);
            return swap;
        }

        // A character prefab written before the magazine reload or the mafia look is rebuilt.
        public bool NeedsRestyle(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab != null && (prefab.transform.Find("Magazine Swap") == null || prefab.transform.Find(StyleMarker) == null);
        }

        // ---------------------------------------------------------------- configs

        private void BuildConfigs()
        {
            var weapons = AssetDatabase.LoadAssetAtPath<CD_Weapons>(WeaponsAsset);
            if (weapons == null)
            {
                weapons = ScriptableObject.CreateInstance<CD_Weapons>();
                weapons.FillDefaults();
                AssetDatabase.CreateAsset(weapons, WeaponsAsset);
                _report.Add("Wrote " + WeaponsAsset);
            }
            foreach (WeaponCVO weapon in weapons.Weapons)
                if (weapon.Prefab == null) weapon.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WeaponPath(weapon.Id));
            EditorUtility.SetDirty(weapons);

            var characters = AssetDatabase.LoadAssetAtPath<CD_Characters>(CharactersAsset);
            if (characters == null)
            {
                characters = ScriptableObject.CreateInstance<CD_Characters>();
                characters.FillDefaults();
                AssetDatabase.CreateAsset(characters, CharactersAsset);
                _report.Add("Wrote " + CharactersAsset);
            }
            foreach (CharacterTypeCVO type in characters.Types)
                if (type.Prefab == null) type.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath(type.Shape));
            EditorUtility.SetDirty(characters);
        }

        // Files both configs on GameplaySystemRoot's RootAdapter, where NightShiftModel reads them.
        private void FileConfigs(GameObject root)
        {
            var adapter = root.GetComponent<RootAdapter>();
            if (adapter == null)
            {
                adapter = root.AddComponent<RootAdapter>();
                _report.Add("Added a RootAdapter to GameplaySystemRoot");
            }
            var serialized = new SerializedObject(adapter);
            SerializedProperty keys = serialized.FindProperty("_scriptableMap.m_Keys");
            SerializedProperty values = serialized.FindProperty("_scriptableMap.m_Values");
            FileEntry(keys, values, nameof(CD_Weapons), AssetDatabase.LoadAssetAtPath<CD_Weapons>(WeaponsAsset));
            FileEntry(keys, values, nameof(CD_Characters), AssetDatabase.LoadAssetAtPath<CD_Characters>(CharactersAsset));
            FileEntry(keys, values, nameof(CD_Missions), AssetDatabase.LoadAssetAtPath<CD_Missions>(MissionsAsset));
            FileEntry(keys, values, nameof(CD_Upgrades), AssetDatabase.LoadAssetAtPath<CD_Upgrades>(UpgradesAsset));
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void FileEntry(SerializedProperty keys, SerializedProperty values, string key, Object asset)
        {
            for (int i = 0; i < keys.arraySize; i++)
            {
                if (keys.GetArrayElementAtIndex(i).stringValue != key) continue;
                if (values.GetArrayElementAtIndex(i).objectReferenceValue == null) values.GetArrayElementAtIndex(i).objectReferenceValue = asset;
                return;
            }
            keys.arraySize++;
            values.arraySize++;
            keys.GetArrayElementAtIndex(keys.arraySize - 1).stringValue = key;
            values.GetArrayElementAtIndex(values.arraySize - 1).objectReferenceValue = asset;
            _report.Add("Filed " + key + " on GameplaySystemRoot's RootAdapter");
        }

        // The humanoid prefabs from the first build: removed from the pool group, then deleted.
        private void RetireHumanoids()
        {
            var group = AssetDatabase.LoadAssetAtPath<FlowIoC.PoolModule.Data.UnityObjects.CD_PoolGroup>(PoolAsset);
            if (group != null && group.Items.RemoveAll(item => item.PoolKey == "gameplay.player" || item.PoolKey == "gameplay.guard") > 0)
            {
                EditorUtility.SetDirty(group);
                _report.Add("Removed gameplay.player and gameplay.guard from Pool_Gameplay - characters come from CD_Characters");
            }
            foreach (string path in Retired)
            {
                if (!Exists(path)) continue;
                AssetDatabase.DeleteAsset(path);
                _report.Add("Deleted " + path + " - replaced by the shape characters");
            }
        }
    }
}
#endif
