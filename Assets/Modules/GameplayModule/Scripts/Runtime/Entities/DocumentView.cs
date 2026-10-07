using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // The file a contract is after. Put one in a level prefab where it should be found; the run reads its
    // position, and the folder disappears once the player has taken it.
    public class DocumentView : MonoBehaviour
    {
        [Tooltip("Hidden once the document is taken.")]
        [SerializeField] private GameObject _visual;
        [Tooltip("Pulses while the document waits to be found.")]
        [SerializeField] private SpriteRenderer _glow;

        public void Apply(bool taken)
        {
            if (_visual != null && _visual.activeSelf == taken) _visual.SetActive(!taken);
            if (_glow == null || taken) return;
            Color color = _glow.color;
            color.a = 0.25f + 0.2f * Mathf.Sin(Time.time * 4f);
            _glow.color = color;
        }
    }
}
