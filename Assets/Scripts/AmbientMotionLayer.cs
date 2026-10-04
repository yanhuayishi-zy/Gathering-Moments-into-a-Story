using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RebuildHighSchool
{
    public sealed class HotspotOutlineGraphic : MaskableGraphic
    {
        private Vector2[] points;
        private float thickness;
        private readonly List<Image> segments = new List<Image>();

        public void Configure(Vector2[] normalizedPoints, float lineThickness)
        {
            points = normalizedPoints;
            thickness = lineThickness;
            raycastTarget = false;
            BuildSegments();
            SetVerticesDirty();
        }

        public void SetTint(Color value)
        {
            color = value;
            for (int i = 0; i < segments.Count; i++)
                if (segments[i] != null) segments[i].color = value;
        }

        private void BuildSegments()
        {
            segments.Clear();
            if (points == null || points.Length < 3) return;
            Rect bounds = rectTransform.rect;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 start = Vector2.Scale(points[i], bounds.size);
                Vector2 end = Vector2.Scale(points[(i + 1) % points.Length], bounds.size);
                Vector2 delta = end - start;

                var lineObject = new GameObject("GlowEdge_" + i, typeof(RectTransform), typeof(Image));
                lineObject.transform.SetParent(transform, false);
                RectTransform lineRect = lineObject.GetComponent<RectTransform>();
                lineRect.anchorMin = lineRect.anchorMax = lineRect.pivot = new Vector2(.5f, .5f);
                lineRect.anchoredPosition = (start + end) * .5f;
                lineRect.sizeDelta = new Vector2(delta.magnitude + thickness, thickness);
                lineRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                Image line = lineObject.GetComponent<Image>();
                line.sprite = UiKit.WhiteSprite;
                line.color = color;
                line.raycastTarget = false;
                segments.Add(line);

                var jointObject = new GameObject("GlowJoint_" + i, typeof(RectTransform), typeof(Image));
                jointObject.transform.SetParent(transform, false);
                RectTransform jointRect = jointObject.GetComponent<RectTransform>();
                jointRect.anchorMin = jointRect.anchorMax = jointRect.pivot = new Vector2(.5f, .5f);
                jointRect.anchoredPosition = start;
                jointRect.sizeDelta = Vector2.one * thickness;
                Image joint = jointObject.GetComponent<Image>();
                joint.sprite = UiKit.CircleSprite;
                joint.color = color;
                joint.raycastTarget = false;
                segments.Add(joint);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }

    public sealed class HotspotGlow : MonoBehaviour
    {
        private Graphic glow;
        private Graphic innerGlow;
        private Color tint;
        private bool solved;
        private bool available;
        private bool hintVisible;
        private float phase;

        public void Configure(Graphic glowImage, Graphic innerGlowImage, Color color)
        {
            glow = glowImage;
            innerGlow = innerGlowImage;
            tint = color;
            phase = Mathf.Abs(transform.GetSiblingIndex() * 1.73f);
            if (glow != null) glow.gameObject.SetActive(false);
            if (innerGlow != null) innerGlow.gameObject.SetActive(false);
        }

        public void SetState(bool isSolved, bool isAvailable, Color color)
        {
            solved = isSolved;
            available = isAvailable;
            tint = color;
            bool visible = hintVisible && !solved && available;
            if (glow != null) glow.gameObject.SetActive(visible);
            if (innerGlow != null) innerGlow.gameObject.SetActive(visible);
        }

        public void Emphasize()
        {
            if (solved || !available) return;
            hintVisible = true;
            if (glow != null) glow.gameObject.SetActive(true);
            if (innerGlow != null) innerGlow.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (glow == null || solved || !available || !hintVisible) return;
            float wave = .5f + .5f * Mathf.Sin(Time.unscaledTime * 3.2f + phase);
            glow.color = new Color(1f, .72f + wave * .18f, .16f, .34f + wave * .62f);
            float scale = .82f + wave * .36f;
            glow.rectTransform.localScale = Vector3.one * scale;
            if (innerGlow != null)
            {
                innerGlow.color = new Color(1f, .94f, .76f, .28f + wave * .42f);
                innerGlow.rectTransform.localScale = Vector3.one * (.94f + wave * .12f);
            }
        }
    }

    public sealed class AmbientMotionLayer : MonoBehaviour
    {
        private sealed class Waver
        {
            public RectTransform Rect;
            public Vector2 Origin;
            public float X;
            public float Y;
            public float Rotation;
            public float Speed;
            public float Phase;
        }

        private sealed class Drifter
        {
            public RectTransform Rect;
            public Vector2 Origin;
            public float FallSpeed;
            public float Drift;
            public float HorizontalSpeed;
            public float Phase;
            public float MinX;
            public float MaxX;
            public float HalfHeight;
        }

        private readonly List<Waver> wavers = new List<Waver>();
        private readonly List<Drifter> drifters = new List<Drifter>();
        private RectTransform clockMinute;
        private RectTransform clockSecond;
        private RectTransform picture;
        private Vector3 pictureBaseScale = Vector3.one;
        private bool memoryMode;
        private Material sceneMotionMaterial;

        public static AmbientMotionLayer Attach(Transform parent, int chapterIndex, Vector2 canvasSize, bool isMemory)
        {
            RectTransform root = UiKit.Stretch("AmbientMotion", parent);
            var motion = root.gameObject.AddComponent<AmbientMotionLayer>();
            motion.memoryMode = isMemory;
            motion.picture = parent as RectTransform;
            if (motion.picture != null) motion.pictureBaseScale = motion.picture.localScale;
            motion.Build(chapterIndex, canvasSize);
            return motion;
        }

        private void Build(int chapterIndex, Vector2 size)
        {
            // This component is only attached to the completed memory. Motion is
            // deliberately restrained so the original painting remains intact.
            if (!memoryMode) return;
            switch (chapterIndex)
            {
                case 0:
                    // The blue curtain, not the people in front of it.
                    ConfigureSceneMotion(new Rect(.17f, .35f, .075f, .59f), .0038f);
                    AddClock(size, new Vector2(size.x * .207f, size.y * .455f));
                    break;
                case 1:
                    // A restrained flutter on the raised exam paper.
                    ConfigureSceneMotion(new Rect(.49f, .34f, .16f, .25f), .0024f);
                    AddClock(size, new Vector2(size.x * .04f, size.y * .41f));
                    break;
                case 2:
                    AddConfetti(size, 24);
                    break;
                case 3:
                    AddRain(size, 34);
                    break;
                case 4:
                    AddSnow(size, 34);
                    ConfigureSceneMotion(new Rect(.02f, .55f, .72f, .42f), .0026f);
                    break;
                default:
                    ConfigureSceneMotion(new Rect(.02f, .55f, .72f, .43f), .0032f);
                    AddLeaves(size, new Vector2(-size.x * .34f, size.y * .25f), 12);
                    break;
            }
        }

        private Image Shape(string name, Vector2 position, Vector2 shapeSize, Color color, Sprite sprite = null)
        {
            Image image = UiKit.Panel(name, transform, position, shapeSize, color);
            image.sprite = sprite == null ? UiKit.WhiteSprite : sprite;
            image.raycastTarget = false;
            return image;
        }

        private void AddWaver(RectTransform rect, float x, float y, float rotation, float speed, float phase)
        {
            wavers.Add(new Waver
            {
                Rect = rect,
                Origin = rect.anchoredPosition,
                X = x,
                Y = y,
                Rotation = rotation,
                Speed = speed,
                Phase = phase
            });
        }

        private void ConfigureSceneMotion(Rect region, float strength)
        {
            if (!memoryMode || picture == null) return;
            RawImage source = picture.GetComponent<RawImage>();
            Shader shader = Shader.Find("UI/MemorySceneMotion");
            if (source == null || shader == null) return;
            sceneMotionMaterial = new Material(shader) { name = "SceneMotionMaterial" };
            sceneMotionMaterial.SetVector("_MotionRegion", new Vector4(region.x, region.y, region.width, region.height));
            sceneMotionMaterial.SetFloat("_MotionStrength", strength);
            source.material = sceneMotionMaterial;
        }

        private void OnDestroy()
        {
            if (sceneMotionMaterial != null) Destroy(sceneMotionMaterial);
        }

        private void AddPages(Vector2 size, Vector2 position)
        {
            for (int i = 0; i < 2; i++)
            {
                Image page = Shape("TurningPage" + i, position + new Vector2(i * size.x * .025f, i * 4f),
                    new Vector2(size.x * .10f, size.y * .058f), new Color(.98f, .93f, .75f, memoryMode ? .22f : .17f));
                page.rectTransform.pivot = new Vector2(0f, .5f);
                AddWaver(page.rectTransform, 2f, 3f, 3.8f + i, 1.15f + i * .17f, i * 1.9f);
            }
        }

        private void AddClock(Vector2 size, Vector2 position)
        {
            Image minute = Shape("ClockMinute", position, new Vector2(4f, size.y * .052f), new Color(.92f, .87f, .72f, .66f));
            minute.rectTransform.pivot = new Vector2(.5f, 0f);
            Image second = Shape("ClockSecond", position, new Vector2(2f, size.y * .068f), new Color(.80f, .36f, .27f, .72f));
            second.rectTransform.pivot = new Vector2(.5f, 0f);
            clockMinute = minute.rectTransform;
            clockSecond = second.rectTransform;
        }

        private void AddLeaves(Vector2 size, Vector2 center, int count)
        {
            Color[] colors =
            {
                new Color(.55f, .64f, .33f, .30f),
                new Color(.78f, .69f, .31f, .29f),
                new Color(.35f, .52f, .31f, .27f)
            };
            for (int i = 0; i < count; i++)
            {
                float px = center.x + ((i * 47) % 120 - 60) * size.x / 900f;
                float py = center.y + ((i * 31) % 100 - 50) * size.y / 600f;
                Image leaf = Shape("Leaf" + i, new Vector2(px, py), new Vector2(12 + i % 4 * 3, 6 + i % 3 * 2), colors[i % colors.Length]);
                leaf.rectTransform.localEulerAngles = new Vector3(0, 0, i * 37f);
                AddWaver(leaf.rectTransform, 5f + i % 3 * 2f, 3f, 7f, .55f + i % 4 * .09f, i * .71f);
            }
        }

        private void AddSnow(Vector2 size, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = -size.x * .5f + ((i * 73) % 1000) / 1000f * size.x;
                float y = -size.y * .5f + ((i * 193) % 1000) / 1000f * size.y;
                float dot = 4f + i % 4 * 2.2f;
                Image flake = Shape("Snow" + i, new Vector2(x, y), new Vector2(dot, dot),
                    new Color(.94f, .97f, 1f, .30f + (i % 3) * .07f), UiKit.CircleSprite);
                drifters.Add(new Drifter
                {
                    Rect = flake.rectTransform,
                    Origin = flake.rectTransform.anchoredPosition,
                    FallSpeed = 18f + i % 7 * 5f,
                    Drift = 4f + i % 5 * 1.5f,
                    HorizontalSpeed = 8f + i % 6 * 2.5f,
                    Phase = i * .83f,
                    MinX = -size.x * .5f,
                    MaxX = size.x * .5f,
                    HalfHeight = size.y * .5f
                });
            }
        }

        private void AddRain(Vector2 size, int count)
        {
            // The farewell scene is a covered corridor. Keep the animated rain
            // over the open/outdoor side instead of letting it cross the hallway.
            float rainLeft = -size.x * .5f;
            float rainRight = -size.x * .14f;
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Lerp(rainLeft, rainRight, ((i * 67) % 1000) / 1000f);
                float y = -size.y * .5f + ((i * 137) % 1000) / 1000f * size.y;
                Image drop = Shape("Rain" + i, new Vector2(x, y), new Vector2(2f, 24f + i % 4 * 7f),
                    new Color(.76f, .88f, .92f, .12f + (i % 3) * .035f));
                drop.rectTransform.localEulerAngles = new Vector3(0, 0, 12f);
                drifters.Add(new Drifter
                {
                    Rect = drop.rectTransform,
                    Origin = drop.rectTransform.anchoredPosition,
                    FallSpeed = 130f + i % 6 * 18f,
                    Drift = 1.5f,
                    HorizontalSpeed = -28f - i % 4 * 5f,
                    Phase = i * .41f,
                    MinX = rainLeft,
                    MaxX = rainRight,
                    HalfHeight = size.y * .5f
                });
            }
        }

        private void AddConfetti(Vector2 size, int count)
        {
            Color[] colors =
            {
                new Color(.89f, .34f, .24f, .34f),
                new Color(.95f, .73f, .22f, .34f),
                new Color(.24f, .55f, .68f, .30f)
            };
            for (int i = 0; i < count; i++)
            {
                float x = -size.x * .5f + ((i * 83) % 1000) / 1000f * size.x;
                float y = -size.y * .15f + ((i * 151) % 1000) / 1000f * size.y * .65f;
                Image paper = Shape("Confetti" + i, new Vector2(x, y), new Vector2(8f + i % 3 * 3f, 15f), colors[i % colors.Length]);
                drifters.Add(new Drifter
                {
                    Rect = paper.rectTransform,
                    Origin = paper.rectTransform.anchoredPosition,
                    FallSpeed = 24f + i % 6 * 6f,
                    Drift = 18f,
                    HorizontalSpeed = 0f,
                    Phase = i * .77f,
                    MinX = -size.x * .5f,
                    MaxX = size.x * .5f,
                    HalfHeight = size.y * .5f
                });
            }
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            if (sceneMotionMaterial != null) sceneMotionMaterial.SetFloat("_MotionTime", time);
            if (memoryMode && picture != null)
            {
                float breathe = 1f + Mathf.Sin(time * .62f) * .006f;
                picture.localScale = pictureBaseScale * breathe;
            }

            for (int i = 0; i < wavers.Count; i++)
            {
                Waver item = wavers[i];
                if (item.Rect == null) continue;
                float wave = Mathf.Sin(time * item.Speed + item.Phase);
                item.Rect.anchoredPosition = item.Origin + new Vector2(item.X * wave, item.Y * Mathf.Sin(time * item.Speed * .73f + item.Phase));
                item.Rect.localEulerAngles = new Vector3(0, 0, item.Rotation * wave);
            }

            for (int i = 0; i < drifters.Count; i++)
            {
                Drifter item = drifters[i];
                if (item.Rect == null) continue;
                Vector2 position = item.Rect.anchoredPosition;
                position.y -= item.FallSpeed * Time.unscaledDeltaTime;
                position.x += item.HorizontalSpeed * Time.unscaledDeltaTime;
                position.x += Mathf.Sin(time * .72f + item.Phase) * item.Drift * Time.unscaledDeltaTime;
                if (position.y < -item.HalfHeight - 30f) position.y = item.HalfHeight + 30f;
                if (position.x < item.MinX - 30f) position.x = item.MaxX;
                else if (position.x > item.MaxX + 30f) position.x = item.MinX;
                item.Rect.anchoredPosition = position;
                float baseAngle = item.HorizontalSpeed < -20f ? 12f : 0f;
                item.Rect.localEulerAngles = new Vector3(0, 0, baseAngle + Mathf.Sin(time * 1.1f + item.Phase) * 8f);
            }

            if (clockSecond != null) clockSecond.localEulerAngles = new Vector3(0, 0, -time * 36f);
            if (clockMinute != null) clockMinute.localEulerAngles = new Vector3(0, 0, -time * .6f);
        }
    }
}
