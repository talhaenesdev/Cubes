using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // A speech bubble in the level, over the head of whoever speaks: a cream card with a tail and dark lettering,
    // wrapped to a few words a line. It pops in on each new line. Built at runtime; nothing to author.
    public class SpeechBubbleView : MonoBehaviour
    {
        private const int WrapAt = 30;
        private const float Padding = 0.16f;
        private TextMesh _text;
        private SpriteRenderer _card;
        private SpriteRenderer _edge;
        private SpriteRenderer _tail;
        private string _shown;
        private float _age;

        // Makes the card, the tail and the text under this object; call once after adding the component.
        public void Build()
        {
            var texture = new Texture2D(1, 1) {filterMode = FilterMode.Point};
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            _edge = Part("Edge", pixel, new Color(0.08f, 0.06f, 0.06f), 70);
            _card = Part("Card", pixel, new Color(0.93f, 0.89f, 0.8f), 71);
            _tail = Part("Tail", pixel, new Color(0.93f, 0.89f, 0.8f), 71);
            _tail.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var label = new GameObject("Text");
            label.transform.SetParent(transform, false);
            _text = label.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.font = font;
            _text.fontSize = 48;
            _text.characterSize = 0.03f;
            _text.anchor = TextAnchor.MiddleCenter;
            _text.alignment = TextAlignment.Center;
            _text.color = new Color(0.1f, 0.07f, 0.06f);
            var renderer = label.GetComponent<MeshRenderer>();
            if (font != null) renderer.sharedMaterial = font.material;
            renderer.sortingOrder = 72;
        }

        private SpriteRenderer Part(string name, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var part = go.AddComponent<SpriteRenderer>();
            part.sprite = sprite;
            part.color = color;
            part.sortingOrder = order;
            return part;
        }

        // Over the speaker's head; hidden when nobody is speaking.
        public void Show(Vector2 head, string text)
        {
            bool speaking = !string.IsNullOrEmpty(text);
            if (gameObject.activeSelf != speaking) gameObject.SetActive(speaking);
            if (!speaking) { _shown = null; return; }
            if (text != _shown)
            {
                _shown = text;
                _age = 0f;
                _text.text = Wrap(text);
                Fit();
            }
            _age += Time.unscaledDeltaTime;
            // A new line fades in over a moment, then the bubble bobs gently. Its size never changes, so it is measured once.
            float fade = Mathf.Clamp01(_age / 0.15f);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, fade);
            _card.color = new Color(_card.color.r, _card.color.g, _card.color.b, fade);
            _edge.color = new Color(_edge.color.r, _edge.color.g, _edge.color.b, fade);
            _tail.color = new Color(_tail.color.r, _tail.color.g, _tail.color.b, fade);
            transform.localScale = Vector3.one;
            transform.localPosition = new Vector3(head.x, head.y + 0.1f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.03f, 0f);
        }

        // The card sized to the text, the tail under its bottom edge.
        private void Fit()
        {
            Bounds bounds = _text.GetComponent<MeshRenderer>().bounds;
            Vector3 lossy = transform.lossyScale;
            float width = bounds.size.x / Mathf.Max(0.0001f, lossy.x) + Padding * 2f;
            float height = bounds.size.y / Mathf.Max(0.0001f, lossy.y) + Padding * 2f;
            width = Mathf.Max(width, 1.2f);
            height = Mathf.Max(height, 0.6f);
            _card.transform.localScale = new Vector3(width, height, 1f);
            _edge.transform.localScale = new Vector3(width + 0.08f, height + 0.08f, 1f);
            // The whole bubble sits above the point it is given.
            float middle = height * 0.5f + 0.2f;
            _card.transform.localPosition = new Vector3(0f, middle, 0f);
            _edge.transform.localPosition = new Vector3(0f, middle, 0f);
            _text.transform.localPosition = new Vector3(0f, middle, 0f);
            _tail.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
            _tail.transform.localPosition = new Vector3(-width * 0.25f, middle - height * 0.5f, 0f);
        }

        private static string Wrap(string text)
        {
            var lines = new System.Text.StringBuilder();
            int length = 0;
            foreach (string word in text.Split(' '))
            {
                if (length > 0 && length + word.Length > WrapAt)
                {
                    lines.Append('\n');
                    length = 0;
                }
                else if (length > 0)
                {
                    lines.Append(' ');
                    length++;
                }
                lines.Append(word);
                length += word.Length;
            }
            return lines.ToString();
        }
    }
}
