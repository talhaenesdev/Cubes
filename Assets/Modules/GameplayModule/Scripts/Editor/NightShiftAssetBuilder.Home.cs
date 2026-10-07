#if UNITY_EDITOR
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Entities;
using Modules.GameplayModule.Enums;
using UnityEditor;
using UnityEngine;

namespace Modules.GameplayModule.Editor
{
    // The lived-in hideout: a wardrobe and a weapon locker, warm lights, a neon sign, shelves - and a cat.
    // Added to the hideout prefab in place, under "Home", so nothing already there is touched.
    public partial class NightShiftAssetBuilder
    {
        public const string OutfitsAsset = ModuleRoot + "/Scriptables/CD_Outfits.asset";
        private const string Wardrobe = Prefabs + "/Fixtures/PB_Wardrobe.prefab";
        private const string WeaponLocker = Prefabs + "/Fixtures/PB_WeaponLocker.prefab";
        private const string Cat = Prefabs + "/Fixtures/PB_Cat.prefab";
        private const string HomeChild = "Home";

        private void BuildHome()
        {
            if (AssetDatabase.LoadAssetAtPath<CD_Outfits>(OutfitsAsset) == null)
            {
                var outfits = ScriptableObject.CreateInstance<CD_Outfits>();
                outfits.FillDefaults();
                AssetDatabase.CreateAsset(outfits, OutfitsAsset);
                _report.Add("Wrote " + OutfitsAsset);
            }
            // An outfit added to the defaults after CD_Outfits was written is hung in it too.
            var wardrobe = AssetDatabase.LoadAssetAtPath<CD_Outfits>(OutfitsAsset);
            var defaults = ScriptableObject.CreateInstance<CD_Outfits>();
            defaults.FillDefaults();
            foreach (var outfit in defaults.Outfits)
                if (wardrobe.Find(outfit.Id) == null)
                {
                    wardrobe.Outfits.Add(outfit);
                    EditorUtility.SetDirty(wardrobe);
                    _report.Add("Hung " + outfit.Name + " in the wardrobe");
                }
            Object.DestroyImmediate(defaults);
            BuildWardrobe();
            BuildLocker();
            BuildCat();
            HomeForHideout();
        }

        private void BuildWardrobe()
        {
            if (!ShouldWrite(Wardrobe)) return;
            GameObject root = Node("PB_Wardrobe", null, Vector2.zero);
            root.AddComponent<StationView>().Kind = StationKind.Wardrobe;
            Color oak = new(0.3f, 0.19f, 0.11f);
            Color dark = new(0.18f, 0.11f, 0.06f);
            Part("Carcass", root.transform, new Vector2(0f, 1.25f), new Vector2(1.3f, 2.5f), dark, -22);
            Part("Crown", root.transform, new Vector2(0f, 2.55f), new Vector2(1.45f, 0.14f), oak, -21);
            Part("Plinth", root.transform, new Vector2(0f, 0.08f), new Vector2(1.4f, 0.16f), oak, -21);
            Part("Door L", root.transform, new Vector2(-0.31f, 1.3f), new Vector2(0.58f, 2.2f), oak, -21);
            Part("Door R", root.transform, new Vector2(0.31f, 1.3f), new Vector2(0.58f, 2.2f), oak, -21);
            Part("Panel L", root.transform, new Vector2(-0.31f, 1.65f), new Vector2(0.42f, 1.1f), dark, -20);
            Part("Mirror", root.transform, new Vector2(0.31f, 1.55f), new Vector2(0.42f, 1.3f), new Color(0.35f, 0.4f, 0.42f), -20);
            Part("Mirror sheen", root.transform, new Vector2(0.24f, 1.7f), new Vector2(0.06f, 0.9f), new Color(1f, 1f, 1f, 0.18f), -19, 12f);
            Part("Handle L", root.transform, new Vector2(-0.07f, 1.2f), new Vector2(0.04f, 0.16f), _hatBrass, -19);
            Part("Handle R", root.transform, new Vector2(0.07f, 1.2f), new Vector2(0.04f, 0.16f), _hatBrass, -19);
            Part("Hat on top", root.transform, new Vector2(-0.2f, 2.72f), new Vector2(0.42f, 0.16f), _felt, -20);
            Part("Hat brim on top", root.transform, new Vector2(-0.18f, 2.65f), new Vector2(0.64f, 0.04f), _felt, -20);
            SavePrefab<GameObject>(root, Wardrobe);
        }

        private void BuildLocker()
        {
            if (!ShouldWrite(WeaponLocker)) return;
            GameObject root = Node("PB_WeaponLocker", null, Vector2.zero);
            root.AddComponent<StationView>().Kind = StationKind.WeaponLocker;
            Part("Cabinet", root.transform, new Vector2(0f, 1.15f), new Vector2(1f, 2.3f), new Color(0.12f, 0.13f, 0.12f), -22);
            Part("Inside", root.transform, new Vector2(0f, 1.2f), new Vector2(0.84f, 2f), new Color(0.05f, 0.05f, 0.05f), -21);
            Part("Felt", root.transform, new Vector2(0f, 1.2f), new Vector2(0.8f, 1.95f), new Color(0.25f, 0.06f, 0.07f), -21);
            for (int i = 0; i < 3; i++)
            {
                float x = -0.25f + i * 0.25f;
                Part("Rifle " + i, root.transform, new Vector2(x, 1.25f), new Vector2(0.06f, 1.6f), _ink, -20);
                Part("Stock " + i, root.transform, new Vector2(x, 0.55f), new Vector2(0.1f, 0.3f), _wood, -20);
            }
            Part("Pistol rack", root.transform, new Vector2(0f, 2.05f), new Vector2(0.7f, 0.05f), _steel, -20);
            Part("Pistol", root.transform, new Vector2(-0.15f, 1.98f), new Vector2(0.22f, 0.08f), _ink, -19);
            Part("Glass", root.transform, new Vector2(0f, 1.2f), new Vector2(0.84f, 2f), new Color(0.6f, 0.7f, 0.75f, 0.12f), -18);
            Part("Glass sheen", root.transform, new Vector2(-0.2f, 1.4f), new Vector2(0.05f, 1.4f), new Color(1f, 1f, 1f, 0.15f), -17, 10f);
            Part("Lock", root.transform, new Vector2(0.38f, 1.2f), new Vector2(0.06f, 0.12f), _hatBrass, -17);
            SavePrefab<GameObject>(root, WeaponLocker);
        }

        private void BuildCat()
        {
            if (!ShouldWrite(Cat)) return;
            Sprite round = _shapes[CharacterShape.Circle];
            Color fur = new(0.1f, 0.09f, 0.09f);
            GameObject root = Node("PB_Cat", null, Vector2.zero);
            var cat = root.AddComponent<CatView>();
            Part("Shadow", root.transform, new Vector2(0f, 0.02f), new Vector2(0.7f, 0.08f), new Color(0f, 0f, 0f, 0.4f), -2).sprite = _soft;
            Transform body = Node("Body", root.transform, Vector2.zero).transform;
            var legs = new System.Collections.Generic.List<Object>();
            foreach (float x in new[] {-0.18f, -0.1f, 0.12f, 0.2f})
            {
                Transform leg = Node("Leg", body, new Vector2(x, 0.16f)).transform;
                Part("Paw", leg, new Vector2(0f, -0.07f), new Vector2(0.05f, 0.16f), fur, 3);
                legs.Add(leg);
            }
            Part("Torso", body, new Vector2(0f, 0.24f), new Vector2(0.5f, 0.2f), fur, 4).sprite = round;
            Part("Head", body, new Vector2(0.27f, 0.36f), new Vector2(0.2f, 0.18f), fur, 5).sprite = round;
            Part("Ear L", body, new Vector2(0.21f, 0.46f), new Vector2(0.06f, 0.06f), fur, 5, 45f);
            Part("Ear R", body, new Vector2(0.32f, 0.46f), new Vector2(0.06f, 0.06f), fur, 5, 45f);
            GameObject open = Node("Eyes open", body, Vector2.zero);
            Part("Eye L", open.transform, new Vector2(0.27f, 0.37f), new Vector2(0.035f, 0.03f), new Color(0.95f, 0.8f, 0.2f), 6).sprite = round;
            Part("Eye R", open.transform, new Vector2(0.33f, 0.37f), new Vector2(0.035f, 0.03f), new Color(0.95f, 0.8f, 0.2f), 6).sprite = round;
            Part("Glow", open.transform, new Vector2(0.3f, 0.37f), new Vector2(0.2f, 0.1f), new Color(1f, 0.85f, 0.3f, 0.18f), 5).sprite = _soft;
            GameObject shut = Node("Eyes shut", body, Vector2.zero);
            Part("Lid L", shut.transform, new Vector2(0.27f, 0.37f), new Vector2(0.04f, 0.008f), new Color(0.95f, 0.8f, 0.2f), 6);
            Part("Lid R", shut.transform, new Vector2(0.33f, 0.37f), new Vector2(0.04f, 0.008f), new Color(0.95f, 0.8f, 0.2f), 6);
            shut.SetActive(false);
            Transform tail = Node("Tail", body, new Vector2(-0.24f, 0.26f)).transform;
            Part("Tail base", tail, new Vector2(-0.08f, 0.06f), new Vector2(0.16f, 0.04f), fur, 3, 30f);
            Part("Tail tip", tail, new Vector2(-0.17f, 0.18f), new Vector2(0.04f, 0.16f), fur, 3, -10f);
            Assign(cat, "_body", body);
            Assign(cat, "_tail", tail);
            AssignArray(cat, "_legs", legs);
            Assign(cat, "_eyesOpen", open);
            Assign(cat, "_eyesShut", shut);
            SavePrefab<GameObject>(root, Cat);
        }

        public bool HideoutNeedsHome()
        {
            var level = AssetDatabase.LoadAssetAtPath<GameObject>(HideoutPrefab);
            return level != null && level.transform.Find(HomeChild) == null;
        }

        // Wardrobe by the bed, the locker beside the tape deck, a cat, and the warm clutter of a home.
        private void HomeForHideout()
        {
            if (!HideoutNeedsHome()) return;
            GameObject contents = PrefabUtility.LoadPrefabContents(HideoutPrefab);
            try
            {
                Transform home = Node(HomeChild, contents.transform, Vector2.zero).transform;
                Transform fixtures = contents.transform.Find("Fixtures") ?? contents.transform;
                Place(Wardrobe, fixtures, new Vector2(-7.35f, 0f));
                Place(WeaponLocker, fixtures, new Vector2(2.65f, 0f));
                Place(Cat, home, new Vector2(-3f, 0f));
                Decorate(home);
                PrefabUtility.SaveAsPrefabAsset(contents, HideoutPrefab);
                _report.Add("Furnished the hideout: wardrobe, weapon locker, lights, a cat");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private void Decorate(Transform home)
        {
            Color warm = new(1f, 0.72f, 0.38f);
            // pendant lamps with a pool of light under each
            foreach (float x in new[] {-4.6f, 0.4f, 5.6f})
            {
                Part("Pendant cord", home, new Vector2(x, 3.6f), new Vector2(0.02f, 0.6f), _ink, -30);
                Part("Pendant shade", home, new Vector2(x, 3.25f), new Vector2(0.5f, 0.2f), _hatBrass, -29);
                Part("Pendant bulb", home, new Vector2(x, 3.13f), new Vector2(0.12f, 0.08f), new Color(1f, 0.9f, 0.6f), -28).sprite = _shapes[CharacterShape.Circle];
                Part("Pendant glow", home, new Vector2(x, 3.05f), new Vector2(1.8f, 1f), new Color(warm.r, warm.g, warm.b, 0.3f), -27).sprite = _soft;
                Part("Light pool", home, new Vector2(x, 1.4f), new Vector2(3.2f, 3.4f), new Color(warm.r, warm.g, warm.b, 0.07f), -27).sprite = _soft;
                Part("Floor pool", home, new Vector2(x, 0.05f), new Vector2(2.4f, 0.25f), new Color(warm.r, warm.g, warm.b, 0.14f), -9).sprite = _soft;
            }
            // a string of fairy lights sagging along the ceiling
            for (int i = 0; i < 34; i++)
            {
                float x = -7.6f + i * 0.46f;
                float y = 3.82f - Mathf.Abs(Mathf.Sin(i * 0.55f)) * 0.18f;
                Color bulb = i % 3 == 0 ? new Color(1f, 0.5f, 0.35f) : i % 3 == 1 ? new Color(1f, 0.85f, 0.5f) : new Color(0.95f, 0.75f, 0.4f);
                Part("Fairy bulb", home, new Vector2(x, y), new Vector2(0.06f, 0.06f), bulb, -26).sprite = _shapes[CharacterShape.Circle];
                Part("Fairy glow", home, new Vector2(x, y), new Vector2(0.3f, 0.3f), new Color(bulb.r, bulb.g, bulb.b, 0.25f), -27).sprite = _soft;
            }
            // a neon sign on the wall
            // shelves of books and records over the couch, a plant by the wall, a framed photo
            Part("Shelf", home, new Vector2(5.8f, 2.55f), new Vector2(1.9f, 0.06f), _wood, -38);
            var random = new System.Random(5);
            for (int i = 0; i < 10; i++)
            {
                Color spine = new((float) random.NextDouble() * 0.4f + 0.15f, (float) random.NextDouble() * 0.2f + 0.08f, (float) random.NextDouble() * 0.15f + 0.06f);
                Part("Book", home, new Vector2(5f + i * 0.16f, 2.77f), new Vector2(0.12f, 0.36f + (float) random.NextDouble() * 0.1f), spine, -37, i == 7 ? 14f : 0f);
            }
            Part("Plant pot", home, new Vector2(7.55f, 0.25f), new Vector2(0.4f, 0.5f), new Color(0.45f, 0.24f, 0.16f), -19);
            for (int i = 0; i < 6; i++)
                Part("Palm leaf", home, new Vector2(7.4f + i * 0.06f, 0.95f + (i % 2) * 0.15f), new Vector2(0.12f, 0.7f), new Color(0.1f, 0.24f, 0.13f), -20, -40f + i * 16f);
            Part("Photo frame", home, new Vector2(-3.8f, 2.4f), new Vector2(0.6f, 0.75f), _hatBrass, -40);
            Part("Photo", home, new Vector2(-3.8f, 2.4f), new Vector2(0.5f, 0.65f), new Color(0.25f, 0.22f, 0.2f), -39);
            Part("Photo figure", home, new Vector2(-3.8f, 2.35f), new Vector2(0.16f, 0.32f), new Color(0.1f, 0.09f, 0.1f), -38);
            // whisky and a glass on the sideboard by the window
            Part("Bottle", home, new Vector2(1.6f, 1.06f), new Vector2(0.1f, 0.3f), new Color(0.55f, 0.32f, 0.1f, 0.9f), -16);
            Part("Glass", home, new Vector2(1.8f, 0.97f), new Vector2(0.1f, 0.12f), new Color(0.8f, 0.7f, 0.5f, 0.6f), -16);
            // the cat's bowl
            Part("Bowl", home, new Vector2(-2.9f, 0.05f), new Vector2(0.3f, 0.08f), _crimson, -8);
        }
    }
}
#endif
