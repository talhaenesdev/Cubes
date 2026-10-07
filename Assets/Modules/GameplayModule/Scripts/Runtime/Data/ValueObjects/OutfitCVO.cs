using System;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One outfit in CD_Outfits: the colours the player's coat, tie, hat and scarf are dyed, and which pieces show.
    [Serializable]
    public class OutfitCVO
    {
        public string Id = "new_outfit";
        public string Name = "NEW OUTFIT";
        [Tooltip("Cash to buy it at the wardrobe. 0 is owned from the start.")]
        [Min(0)] public int Price;
        public Color Coat = new(0.13f, 0.12f, 0.14f);
        public Color Waistcoat = new(0.08f, 0.07f, 0.08f);
        public Color Shirt = new(0.9f, 0.88f, 0.84f);
        public Color Tie = new(0.55f, 0.07f, 0.1f);
        public Color Hat = new(0.07f, 0.06f, 0.06f);
        public Color HatBand = new(0.55f, 0.07f, 0.1f);
        public Color Scarf = new(0.9f, 0.88f, 0.84f);
        public bool WearHat = true;
        public bool WearScarf = true;
        public bool Cigarette = true;
    }
}
