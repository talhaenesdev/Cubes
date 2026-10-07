using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // The hideout wardrobe. Add an outfit here and it hangs in the wardrobe next run.
    [CreateAssetMenu(fileName = "CD_Outfits", menuName = "Night Shift/CD_Outfits")]
    public class CD_Outfits : ScriptableObject
    {
        [Tooltip("What the player wears before choosing anything.")]
        public string Default = "midnight";
        public List<OutfitCVO> Outfits = new();

        public OutfitCVO Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (OutfitCVO outfit in Outfits)
                if (outfit.Id == id) return outfit;
            return null;
        }

        public void FillDefaults()
        {
            Default = "midnight";
            Color silk = new(0.9f, 0.88f, 0.84f);
            Color crimson = new(0.55f, 0.07f, 0.1f);
            Color felt = new(0.07f, 0.06f, 0.06f);
            Color gold = new(0.72f, 0.58f, 0.3f);
            Outfits = new List<OutfitCVO>
            {
                new() {Id = "engagement", Name = "ENGAGEMENT BLACK", Coat = new(0.05f, 0.05f, 0.06f), Waistcoat = new(0.03f, 0.03f, 0.03f), Shirt = new(0.95f, 0.95f, 0.95f),
                    Tie = new(0.04f, 0.04f, 0.05f), Hat = felt, HatBand = new(0.04f, 0.04f, 0.05f), WearScarf = false, Cigarette = false},
                new() {Id = "midnight", Name = "MIDNIGHT", Coat = new(0.13f, 0.12f, 0.14f), Tie = crimson, Hat = felt, HatBand = crimson, Scarf = silk},
                new() {Id = "bone", Price = 300, Name = "BONE WHITE", Coat = new(0.78f, 0.74f, 0.66f), Waistcoat = new(0.62f, 0.58f, 0.5f), Tie = felt, Hat = new(0.82f, 0.78f, 0.7f), HatBand = felt, Scarf = felt},
                new() {Id = "oxblood", Price = 450, Name = "OXBLOOD", Coat = new(0.32f, 0.07f, 0.08f), Waistcoat = new(0.12f, 0.05f, 0.05f), Tie = gold, Hat = felt, HatBand = gold, Scarf = silk},
                new() {Id = "blue_note", Price = 600, Name = "BLUE NOTE", Coat = new(0.1f, 0.14f, 0.24f), Waistcoat = new(0.06f, 0.08f, 0.14f), Tie = new(0.7f, 0.72f, 0.75f), Hat = new(0.08f, 0.1f, 0.16f), HatBand = new(0.7f, 0.72f, 0.75f), Scarf = silk},
                new() {Id = "undertaker", Price = 750, Name = "UNDERTAKER", Coat = new(0.04f, 0.04f, 0.04f), Waistcoat = new(0.02f, 0.02f, 0.02f), Tie = new(0.04f, 0.04f, 0.04f), Hat = felt, HatBand = new(0.04f, 0.04f, 0.04f), WearScarf = false, Cigarette = false},
                new() {Id = "tobacco", Price = 350, Name = "TOBACCO ROAD", Coat = new(0.36f, 0.24f, 0.14f), Waistcoat = new(0.22f, 0.14f, 0.08f), Tie = gold, Hat = new(0.3f, 0.2f, 0.12f), HatBand = felt, Scarf = new(0.7f, 0.55f, 0.3f), WearHat = true},
                new() {Id = "bareheaded", Price = 0, Name = "BAREHEADED", Coat = new(0.2f, 0.19f, 0.2f), Tie = crimson, WearHat = false, WearScarf = false},
            };
        }
    }
}
