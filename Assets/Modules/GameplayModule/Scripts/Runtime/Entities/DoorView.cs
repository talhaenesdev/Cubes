using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    public class DoorView : MonoBehaviour
    {
        [SerializeField] private GameObject _closed;
        [SerializeField] private GameObject _open;

        public void Apply(bool open)
        {
            if (_closed != null && _closed.activeSelf == open) _closed.SetActive(!open);
            if (_open != null && _open.activeSelf != open) _open.SetActive(open);
        }
    }
}
