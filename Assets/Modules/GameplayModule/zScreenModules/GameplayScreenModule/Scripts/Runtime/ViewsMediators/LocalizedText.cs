using UnityEngine;
using UnityEngine.UI;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    // Marks a Text on the screen as a fixed label: the CD_Localization key it shows. It is saved with the baked
    // layout, so a screen bound from its prefab still knows which texts to translate when the language changes.
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        public string Key;
    }
}
