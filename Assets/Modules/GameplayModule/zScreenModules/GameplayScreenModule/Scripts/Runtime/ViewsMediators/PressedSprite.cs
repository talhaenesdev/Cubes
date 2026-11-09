using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    // Shows a touch control's pressed drawing while a finger holds it. Pure presentation: the on-screen control on the
    // same object still presses the key, this only changes what it looks like.
    public class PressedSprite : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _normal;
        [SerializeField] private Sprite _pressed;

        public void Set(Image image, Sprite normal, Sprite pressed)
        {
            _image = image;
            _normal = normal;
            _pressed = pressed;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_image != null && _pressed != null) _image.sprite = _pressed;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_image != null && _normal != null) _image.sprite = _normal;
        }

        // A screen is pooled: a control that was held when the screen closed opens again released.
        private void OnDisable()
        {
            if (_image != null && _normal != null) _image.sprite = _normal;
        }
    }
}
