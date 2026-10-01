using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    public class LightRoomView : MonoBehaviour
    {
        public float Left = -5f;
        public float Right = 5f;
        [SerializeField] private GameObject _switchOn;
        [SerializeField] private GameObject _switchOff;
        [SerializeField] private SpriteRenderer _darkness;
        [SerializeField] private float _roomHeight = 3.8f;

        public void Apply(bool on, float floor)
        {
            if (_switchOn != null && _switchOn.activeSelf != on) _switchOn.SetActive(on);
            if (_switchOff != null && _switchOff.activeSelf == on) _switchOff.SetActive(!on);
            if (_darkness == null) return;
            if (_darkness.gameObject.activeSelf == on) _darkness.gameObject.SetActive(!on);
            if (on) return;
            Transform dark = _darkness.transform;
            dark.position = new Vector3((Left + Right) * 0.5f, floor + _roomHeight * 0.5f + 0.05f, dark.position.z);
            Vector3 parent = dark.parent != null ? dark.parent.lossyScale : Vector3.one;
            dark.localScale = new Vector3((Right - Left) / parent.x, _roomHeight / parent.y, 1f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.65f, 0.3f, 0.5f);
            float y = transform.position.y;
            Gizmos.DrawLine(new Vector3(Left, y), new Vector3(Right, y));
            Gizmos.DrawLine(new Vector3(Left, y - 1f), new Vector3(Left, y + 2f));
            Gizmos.DrawLine(new Vector3(Right, y - 1f), new Vector3(Right, y + 2f));
        }
    }
}
