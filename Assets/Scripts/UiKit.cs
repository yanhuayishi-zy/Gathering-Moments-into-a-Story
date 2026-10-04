using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RebuildHighSchool
{
    public static class UiKit
    {
        private static Font cachedFont;
        private static Font displayFont;
        private static Sprite whiteSprite;
        private static Sprite circleSprite;
        private static Sprite glowRingSprite;

        public static Font MainFont
        {
            get
            {
                if (cachedFont != null) return cachedFont;
                cachedFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Noto Sans CJK SC", "Noto Sans SC", "SimHei", "Arial" }, 32);
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return cachedFont;
            }
        }

        public static Font DisplayFont
        {
            get
            {
                if (displayFont != null) return displayFont;
                displayFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "KaiTi", "STKaiti", "FangSong", "STFangsong", "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 36);
                if (displayFont == null) displayFont = MainFont;
                return displayFont;
            }
        }

        public static Sprite WhiteSprite
        {
            get
            {
                if (whiteSprite != null) return whiteSprite;
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                texture.Apply();
                whiteSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));
                return whiteSprite;
            }
        }

        public static Sprite CircleSprite
        {
            get
            {
                if (circleSprite != null) return circleSprite;
                const int size = 96;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color32[size * size];
                Vector2 center = new Vector2((size - 1) * .5f, (size - 1) * .5f);
                float radius = size * .46f;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(radius - distance + 1.5f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f));
                return circleSprite;
            }
        }

        public static Sprite GlowRingSprite
        {
            get
            {
                if (glowRingSprite != null) return glowRingSprite;
                const int size = 128;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color32[size * size];
                Vector2 center = new Vector2((size - 1) * .5f, (size - 1) * .5f);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / (size * .5f);
                    float ring = Mathf.Exp(-Mathf.Pow((distance - .72f) / .19f, 2f));
                    float edgeFade = 1f - Mathf.SmoothStep(.92f, 1f, distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, ring * edgeFade);
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                glowRingSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f));
                return glowRingSprite;
            }
        }

        public static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("GameCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            return canvas;
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static RectTransform Stretch(string name, Transform parent)
        {
            var rect = Rect(name, parent, Vector2.zero, Vector2.zero);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Image Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var rect = Rect(name, parent, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = WhiteSprite;
            image.color = color;
            return image;
        }

        public static RawImage TexturePanel(string name, Transform parent, Vector2 position, Vector2 size, Texture texture)
        {
            var rect = Rect(name, parent, position, size);
            var image = rect.gameObject.AddComponent<RawImage>();
            image.texture = texture;
            image.color = Color.white;
            return image;
        }

        public static Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size,
            int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var rect = Rect(name, parent, position, size);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = MainFont;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = true;
            return text;
        }

        public static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size,
            Color color, Color textColor, UnityEngine.Events.UnityAction onClick, int fontSize = 28)
        {
            var image = Panel(name, parent, position, size, color);
            AddOutline(image, new Color(textColor.r, textColor.g, textColor.b, .22f), new Vector2(1.5f, -1.5f));
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(.94f, .94f, .94f, 1f);
            colors.pressedColor = new Color(.78f, .78f, .78f, 1f);
            colors.disabledColor = new Color(.45f, .45f, .45f, .55f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(onClick);

            var text = Label("Label", image.transform, label, Vector2.zero, size - new Vector2(20, 10), fontSize, textColor,
                TextAnchor.MiddleCenter, FontStyle.Normal);
            text.font = DisplayFont;
            text.raycastTarget = false;
            Image topRule = Panel("TopRule", image.transform, new Vector2(0, size.y * .5f - 3f), new Vector2(size.x - 12f, 2f),
                new Color(textColor.r, textColor.g, textColor.b, .20f));
            topRule.raycastTarget = false;
            return button;
        }

        public static Button MarkerButton(string name, Transform parent, Vector2 position,
            Vector2 objectSize, Color color, UnityEngine.Events.UnityAction onClick)
        {
            // The hit area remains generous while the hint itself is a compact,
            // centered marker that only appears after the player requests help.
            Vector2 touchSize = new Vector2(Mathf.Max(168f, objectSize.x + 48f), Mathf.Max(132f, objectSize.y + 48f));
            var rect = Rect(name, parent, position, touchSize);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = WhiteSprite;
            image.color = new Color(1f, 1f, 1f, .008f);
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, .74f);
            colors.pressedColor = new Color(.82f, .82f, .82f, .72f);
            colors.disabledColor = new Color(1f, 1f, 1f, .01f);
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(onClick);
            var markerRect = Rect("HintMarker", rect, Vector2.zero, new Vector2(52f, 52f));
            var marker = markerRect.gameObject.AddComponent<Image>();
            marker.sprite = CircleSprite;
            marker.color = new Color(1f, .76f, .20f, 0f);
            marker.raycastTarget = false;
            marker.gameObject.SetActive(false);
            rect.gameObject.AddComponent<HotspotGlow>().Configure(marker, null, color);
            return button;
        }

        public static void AddOutline(Graphic graphic, Color color, Vector2 distance)
        {
            var outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }
    }

    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect lastSafeArea;
        private Vector2Int lastScreen;

        private void Awake() => Apply();

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreen.x != Screen.width || lastScreen.y != Screen.height) Apply();
        }

        private void Apply()
        {
            var rect = (RectTransform)transform;
            Rect safe = Screen.safeArea;
            rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            lastSafeArea = safe;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
