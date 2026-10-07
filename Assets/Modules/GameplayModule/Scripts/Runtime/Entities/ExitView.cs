using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    public class ExitView : MonoBehaviour
    {
        [Tooltip("The arrow over the door, shown once the document is taken.")]
        [SerializeField] private Transform _guide;

        public void Apply(bool guide)
        {
            if (_guide == null) return;
            if (_guide.gameObject.activeSelf != guide) _guide.gameObject.SetActive(guide);
            if (guide) _guide.localPosition = new Vector3(0f, 3.45f + Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 0.35f, 0f);
        }
    }
}
