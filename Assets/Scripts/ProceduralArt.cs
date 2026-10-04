using UnityEngine;

namespace RebuildHighSchool
{
    public static class ProceduralArt
    {
        private static readonly string[] GeneratedSceneNames =
        {
            "01-enrollment-classroom",
            "02-monthly-exam",
            "03-autumn-sports-day",
            "04-rainy-class-split-corridor",
            "05-senior-night-study",
            "06-graduation-classroom"
        };

        private static readonly string[] GeneratedPuzzleNames =
        {
            "01-new-friends",
            "02-good-exam-result",
            "03-gold-medal-cheer",
            "04-rainy-farewell",
            "05-moonlit-dorm-walk",
            "06-graduation-photo"
        };

        private sealed class Painter
        {
            private readonly int width;
            private readonly int height;
            private readonly Color32[] pixels;

            public Painter(int width, int height, Color top, Color bottom)
            {
                this.width = width;
                this.height = height;
                pixels = new Color32[width * height];
                for (int y = 0; y < height; y++)
                {
                    float t = y / (float)(height - 1);
                    Color row = Color.Lerp(bottom, top, t);
                    for (int x = 0; x < width; x++)
                    {
                        float grain = Hash(x, y) * .025f - .0125f;
                        pixels[y * width + x] = new Color(row.r + grain, row.g + grain, row.b + grain, 1f);
                    }
                }
            }

            private static float Hash(int x, int y)
            {
                uint value = (uint)(x * 374761393 + y * 668265263);
                value = (value ^ (value >> 13)) * 1274126177u;
                return (value & 0xffff) / 65535f;
            }

            private void Blend(int x, int y, Color color)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return;
                int index = y * width + x;
                Color current = pixels[index];
                pixels[index] = Color.Lerp(current, color, color.a);
            }

            public void Rect(float x, float y, float w, float h, Color color)
            {
                int minX = Mathf.Clamp(Mathf.RoundToInt(x * width), 0, width - 1);
                int minY = Mathf.Clamp(Mathf.RoundToInt(y * height), 0, height - 1);
                int maxX = Mathf.Clamp(Mathf.RoundToInt((x + w) * width), 0, width);
                int maxY = Mathf.Clamp(Mathf.RoundToInt((y + h) * height), 0, height);
                for (int py = minY; py < maxY; py++)
                for (int px = minX; px < maxX; px++) Blend(px, py, color);
            }

            public void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
            {
                Triangle(a, b, c, color);
                Triangle(a, c, d, color);
            }

            private void Triangle(Vector2 a, Vector2 b, Vector2 c, Color color)
            {
                int minX = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)) * width), 0, width - 1);
                int maxX = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x)) * width), 0, width - 1);
                int minY = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)) * height), 0, height - 1);
                int maxY = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y)) * height), 0, height - 1);
                float area = Edge(a, b, c);
                if (Mathf.Abs(area) < .00001f) return;
                for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 point = new Vector2(x / (float)width, y / (float)height);
                    float w0 = Edge(b, c, point);
                    float w1 = Edge(c, a, point);
                    float w2 = Edge(a, b, point);
                    if ((w0 >= 0 && w1 >= 0 && w2 >= 0) || (w0 <= 0 && w1 <= 0 && w2 <= 0)) Blend(x, y, color);
                }
            }

            private static float Edge(Vector2 a, Vector2 b, Vector2 c)
            {
                return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
            }

            public void Circle(float cx, float cy, float radius, Color color)
            {
                int minX = Mathf.Clamp(Mathf.RoundToInt((cx - radius) * width), 0, width - 1);
                int maxX = Mathf.Clamp(Mathf.RoundToInt((cx + radius) * width), 0, width - 1);
                int minY = Mathf.Clamp(Mathf.RoundToInt((cy - radius) * height), 0, height - 1);
                int maxY = Mathf.Clamp(Mathf.RoundToInt((cy + radius) * height), 0, height - 1);
                float radiusSquared = radius * radius;
                for (int py = minY; py <= maxY; py++)
                for (int px = minX; px <= maxX; px++)
                {
                    float dx = px / (float)width - cx;
                    float dy = py / (float)height - cy;
                    if (dx * dx + dy * dy <= radiusSquared) Blend(px, py, color);
                }
            }

            public void Glow(float cx, float cy, float radius, Color color)
            {
                int minX = Mathf.Clamp(Mathf.RoundToInt((cx - radius) * width), 0, width - 1);
                int maxX = Mathf.Clamp(Mathf.RoundToInt((cx + radius) * width), 0, width - 1);
                int minY = Mathf.Clamp(Mathf.RoundToInt((cy - radius) * height), 0, height - 1);
                int maxY = Mathf.Clamp(Mathf.RoundToInt((cy + radius) * height), 0, height - 1);
                for (int py = minY; py <= maxY; py++)
                for (int px = minX; px <= maxX; px++)
                {
                    float distance = Vector2.Distance(new Vector2(px / (float)width, py / (float)height), new Vector2(cx, cy));
                    float strength = Mathf.Pow(Mathf.Clamp01(1f - distance / radius), 2f) * color.a;
                    Blend(px, py, new Color(color.r, color.g, color.b, strength));
                }
            }

            public void Line(float x0, float y0, float x1, float y1, float thickness, Color color)
            {
                int steps = Mathf.Max(Mathf.Abs(Mathf.RoundToInt((x1 - x0) * width)), Mathf.Abs(Mathf.RoundToInt((y1 - y0) * height)));
                for (int i = 0; i <= steps; i++)
                {
                    float t = steps == 0 ? 0 : i / (float)steps;
                    Circle(Mathf.Lerp(x0, x1, t), Mathf.Lerp(y0, y1, t), thickness, color);
                }
            }

            public Texture2D Finish(string name)
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = name,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                return texture;
            }
        }

        public static Texture2D Create(LevelDefinition level, int width, int height, bool memoryVersion)
        {
            string folder = memoryVersion ? "PuzzleArt/" : "SceneArt/";
            string assetName = memoryVersion ? GeneratedPuzzleNames[level.Index] : GeneratedSceneNames[level.Index];
            Texture2D generated = Resources.Load<Texture2D>(folder + assetName);
            if (generated != null) return CopyCropped(generated, width, height, memoryVersion, level.Accent);

            Color top = memoryVersion ? Color.Lerp(level.BackgroundTop, Color.white, .08f) : level.BackgroundTop;
            Color bottom = memoryVersion ? Color.Lerp(level.BackgroundBottom, Color.black, .12f) : level.BackgroundBottom;
            var p = new Painter(width, height, top, bottom);
            switch (level.Index)
            {
                case 0: DrawClassroom(p, false, false); break;
                case 1: DrawClassroom(p, true, false); break;
                case 2: DrawSportsField(p); break;
                case 3: DrawCorridor(p); break;
                case 4: DrawClassroom(p, false, true); break;
                default: DrawGraduation(p); break;
            }
            AddAtmosphere(p, level, memoryVersion);
            return p.Finish(memoryVersion ? "MemoryArt" : "SceneArt");
        }

        private static Texture2D CopyCropped(Texture2D source, int width, int height, bool addFrame, Color accent)
        {
            float sourceAspect = source.width / (float)source.height;
            float targetAspect = width / (float)height;
            Vector2 scale = Vector2.one;
            Vector2 offset = Vector2.zero;
            if (sourceAspect > targetAspect)
            {
                scale.x = targetAspect / sourceAspect;
                offset.x = (1f - scale.x) * .5f;
            }
            else
            {
                scale.y = sourceAspect / targetAspect;
                offset.y = (1f - scale.y) * .5f;
            }

            RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(source, temporary, scale, offset);
            RenderTexture.active = temporary;
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = addFrame ? "GeneratedMemoryArt" : "GeneratedSceneArt",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);

            if (addFrame)
            {
                Color32[] pixels = result.GetPixels32();
                Color32 line = accent;
                int thickness = Mathf.Max(4, Mathf.RoundToInt(Mathf.Min(width, height) * .012f));
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    bool border = x < thickness || x >= width - thickness || y < thickness || y >= height - thickness;
                    if (border) pixels[y * width + x] = line;
                }
                result.SetPixels32(pixels);
            }
            result.Apply(false, false);
            return result;
        }

        private static void DrawClassroom(Painter p, bool exam, bool night)
        {
            Color wall = night ? new Color(.10f, .15f, .22f) : new Color(.88f, .84f, .72f);
            Color floor = night ? new Color(.08f, .10f, .14f) : new Color(.40f, .32f, .25f);
            Color desk = night ? new Color(.22f, .17f, .14f) : new Color(.51f, .32f, .18f);
            Color edge = night ? new Color(.09f, .08f, .08f) : new Color(.24f, .18f, .14f);
            p.Rect(0, .24f, 1, .76f, wall);
            p.Quad(new Vector2(0, 0), new Vector2(1, 0), new Vector2(.84f, .38f), new Vector2(.16f, .38f), floor);
            for (int i = 0; i < 8; i++)
            {
                float x = i / 7f;
                p.Line(.5f, .38f, x, 0, .0018f, new Color(1f, 1f, 1f, night ? .035f : .08f));
            }
            p.Rect(.055f, .50f, .49f, .37f, new Color(.055f, .18f, .16f));
            p.Rect(.064f, .51f, .472f, .34f, new Color(.08f, .29f, .25f));
            p.Rect(.055f, .485f, .49f, .018f, new Color(.68f, .54f, .36f));
            for (int i = 0; i < 7; i++) p.Line(.10f + i * .06f, .58f + (i % 2) * .05f, .14f + i * .055f, .57f + (i % 2) * .05f, .002f, new Color(.88f, .88f, .73f, .55f));
            Color sky = night ? new Color(.035f, .08f, .16f) : new Color(.55f, .78f, .88f);
            p.Rect(.63f, .45f, .31f, .45f, new Color(.93f, .89f, .78f));
            p.Rect(.65f, .48f, .27f, .38f, sky);
            p.Rect(.778f, .48f, .014f, .38f, new Color(.87f, .86f, .78f));
            p.Rect(.65f, .66f, .27f, .014f, new Color(.87f, .86f, .78f));
            if (night)
            {
                p.Circle(.84f, .77f, .055f, new Color(.95f, .89f, .68f));
                p.Circle(.866f, .79f, .055f, sky);
                p.Glow(.72f, .45f, .35f, new Color(.93f, .78f, .48f, .20f));
            }
            else
            {
                p.Glow(.82f, .70f, .30f, new Color(1f, .86f, .56f, .30f));
                p.Quad(new Vector2(.65f, .48f), new Vector2(.91f, .48f), new Vector2(.73f, .05f), new Vector2(.42f, .05f), new Color(1f, .84f, .48f, .12f));
            }
            for (int row = 0; row < 3; row++)
            for (int col = 0; col < 4; col++)
            {
                float scale = 1f - row * .13f;
                float x = .10f + col * .205f + row * .032f;
                float y = .10f + row * .118f;
                float w = .145f * scale;
                p.Quad(new Vector2(x, y + .045f), new Vector2(x + w, y + .045f), new Vector2(x + w - .012f, y), new Vector2(x + .012f, y), desk);
                p.Rect(x + .018f, y - .065f * scale, .012f, .07f * scale, edge);
                p.Rect(x + w - .026f, y - .065f * scale, .012f, .07f * scale, edge);
                p.Rect(x + .022f, y + .012f, w - .044f, .012f, new Color(.84f, .68f, .43f, .38f));
                if (exam)
                {
                    p.Quad(new Vector2(x + .035f, y + .025f), new Vector2(x + w - .025f, y + .026f), new Vector2(x + w - .032f, y + .072f), new Vector2(x + .04f, y + .068f), new Color(.95f, .93f, .84f));
                    p.Line(x + .047f, y + .054f, x + w - .045f, y + .054f, .0013f, new Color(.36f, .44f, .46f, .55f));
                }
                else if (!night && (row + col) % 3 == 0)
                {
                    p.Rect(x + .035f, y + .025f, w * .42f, .045f, new Color(.34f, .50f, .67f));
                    p.Rect(x + .043f, y + .031f, w * .30f, .005f, new Color(.92f, .89f, .70f, .7f));
                }
                else if (night) p.Rect(x + .035f, y + .025f, w * .50f, .04f, new Color(.82f, .76f, .57f));
            }
            p.Rect(.46f, .35f, .22f, .065f, desk);
            p.Rect(.48f, .23f, .018f, .13f, edge);
            p.Rect(.64f, .23f, .018f, .13f, edge);
            p.Circle(.59f, .91f, .045f, new Color(.92f, .90f, .79f));
            p.Circle(.59f, .91f, .034f, new Color(.25f, .29f, .28f));
            p.Line(.59f, .91f, .59f, .936f, .003f, Color.white);
            p.Line(.59f, .91f, .615f, .897f, .003f, Color.white);
            if (night)
            {
                p.Rect(.40f, .73f, .12f, .10f, new Color(.75f, .68f, .52f));
                p.Rect(.412f, .744f, .096f, .073f, new Color(.44f, .12f, .11f));
                for (int i = 0; i < 4; i++) p.Rect(.423f + i * .021f, .76f, .012f, .038f, new Color(.92f, .80f, .56f, .65f));
            }
        }

        private static void DrawSportsField(Painter p)
        {
            p.Rect(0, .44f, 1, .56f, new Color(.55f, .75f, .78f));
            p.Glow(.78f, .84f, .28f, new Color(1f, .84f, .49f, .34f));
            p.Quad(new Vector2(0, .06f), new Vector2(1, .02f), new Vector2(.90f, .50f), new Vector2(.08f, .48f), new Color(.66f, .18f, .15f));
            p.Quad(new Vector2(.19f, .15f), new Vector2(.80f, .13f), new Vector2(.72f, .44f), new Vector2(.26f, .44f), new Color(.31f, .57f, .33f));
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                p.Line(Mathf.Lerp(.02f, .19f, t), Mathf.Lerp(.06f, .15f, t), Mathf.Lerp(.10f, .26f, t), Mathf.Lerp(.48f, .44f, t), .0023f, new Color(1f, .86f, .65f, .82f));
                p.Line(Mathf.Lerp(.98f, .80f, t), Mathf.Lerp(.02f, .13f, t), Mathf.Lerp(.90f, .72f, t), Mathf.Lerp(.50f, .44f, t), .0023f, new Color(1f, .86f, .65f, .82f));
            }
            p.Quad(new Vector2(.03f, .51f), new Vector2(.48f, .54f), new Vector2(.43f, .82f), new Vector2(.06f, .80f), new Color(.60f, .63f, .60f));
            for (int i = 0; i < 5; i++) p.Quad(new Vector2(.05f, .54f + i * .05f), new Vector2(.46f - i * .008f, .56f + i * .05f), new Vector2(.45f - i * .008f, .588f + i * .05f), new Vector2(.05f, .568f + i * .05f), new Color(.78f, .77f, .70f));
            p.Rect(.74f, .55f, .026f, .31f, new Color(.22f, .22f, .20f));
            p.Line(.766f, .84f, .92f, .76f, .005f, new Color(.94f, .79f, .38f));
            p.Line(.766f, .84f, .92f, .89f, .005f, new Color(.94f, .79f, .38f));
            p.Quad(new Vector2(.766f, .77f), new Vector2(.91f, .77f), new Vector2(.91f, .88f), new Vector2(.766f, .84f), new Color(.75f, .20f, .18f));
            for (int i = 0; i < 7; i++)
            {
                float x = .53f + i * .06f;
                p.Circle(x, .47f + (i % 2) * .012f, .019f, new Color(.93f, .68f - i * .025f, .48f));
                p.Rect(x - .014f, .36f, .028f, .085f, new Color(.15f + i * .055f, .30f, .48f));
                p.Line(x - .006f, .36f, x - .024f, .30f, .005f, new Color(.15f, .16f, .19f));
                p.Line(x + .006f, .36f, x + .026f, .30f, .005f, new Color(.15f, .16f, .19f));
            }
        }

        private static void DrawCorridor(Painter p)
        {
            p.Rect(0, .30f, 1, .70f, new Color(.72f, .77f, .76f));
            p.Quad(new Vector2(0, 0), new Vector2(1, 0), new Vector2(.66f, .42f), new Vector2(.30f, .42f), new Color(.35f, .39f, .40f));
            p.Line(.30f, .42f, 0, 0, .002f, new Color(.82f, .87f, .84f, .22f));
            p.Line(.66f, .42f, 1, 0, .002f, new Color(.82f, .87f, .84f, .22f));
            p.Rect(.035f, .30f, .28f, .62f, new Color(.79f, .84f, .82f));
            p.Rect(.065f, .35f, .22f, .49f, new Color(.35f, .59f, .70f));
            for (int i = 1; i < 4; i++) p.Rect(.065f + i * .055f, .35f, .007f, .49f, new Color(.83f, .88f, .86f));
            p.Rect(.065f, .59f, .22f, .008f, new Color(.83f, .88f, .86f));
            for (int i = 0; i < 18; i++) p.Line(.075f + i * .045f, .92f, .04f + i * .045f, .83f, .0014f, new Color(.66f, .79f, .84f, .75f));
            p.Glow(.20f, .55f, .24f, new Color(.72f, .89f, .94f, .22f));
            p.Rect(.39f, .30f, .25f, .58f, new Color(.42f, .34f, .28f));
            p.Rect(.425f, .35f, .18f, .45f, new Color(.67f, .65f, .56f));
            p.Rect(.50f, .55f, .025f, .045f, new Color(.85f, .67f, .27f));
            p.Rect(.73f, .31f, .21f, .56f, new Color(.39f, .49f, .53f));
            for (int row = 0; row < 4; row++)
            for (int col = 0; col < 2; col++)
            {
                p.Rect(.755f + col * .083f, .36f + row * .112f, .065f, .083f, new Color(.56f, .64f, .64f));
                p.Circle(.808f + col * .083f, .40f + row * .112f, .006f, new Color(.82f, .75f, .46f));
            }
            p.Rect(.32f, .48f, .052f, .22f, new Color(.94f, .72f, .17f));
            p.Circle(.346f, .48f, .033f, new Color(.94f, .72f, .17f));
            p.Glow(.36f, .17f, .26f, new Color(.72f, .85f, .88f, .14f));
        }

        private static void DrawGraduation(Painter p)
        {
            DrawClassroom(p, false, false);
            p.Rect(.064f, .51f, .472f, .34f, new Color(.08f, .25f, .23f));
            p.Glow(.50f, .46f, .38f, new Color(1f, .71f, .38f, .20f));
            for (int i = 0; i < 8; i++)
            {
                float x = .17f + i * .087f;
                float y = .40f + (i % 2) * .012f;
                p.Circle(x, y, .023f, new Color(.94f, .70f - i * .018f, .56f));
                p.Rect(x - .020f, .27f, .040f, .105f, new Color(.10f, .18f, .28f));
                p.Quad(new Vector2(x - .042f, y + .022f), new Vector2(x + .042f, y + .022f), new Vector2(x + .025f, y + .046f), new Vector2(x - .025f, y + .046f), new Color(.055f, .08f, .12f));
            }
            p.Circle(.82f, .25f, .052f, new Color(.90f, .43f, .47f));
            p.Circle(.87f, .28f, .047f, new Color(.95f, .70f, .27f));
            p.Circle(.785f, .30f, .044f, new Color(.82f, .34f, .48f));
            p.Line(.82f, .23f, .84f, .08f, .006f, new Color(.19f, .43f, .23f));
            p.Line(.87f, .26f, .84f, .08f, .006f, new Color(.19f, .43f, .23f));
            for (int i = 0; i < 20; i++)
            {
                float x = .06f + ((i * 37) % 88) / 100f;
                float y = .56f + ((i * 53) % 38) / 100f;
                Color color = i % 2 == 0 ? new Color(.92f, .62f, .28f, .75f) : new Color(.88f, .84f, .65f, .72f);
                p.Quad(new Vector2(x, y), new Vector2(x + .012f, y + .004f), new Vector2(x + .008f, y + .020f), new Vector2(x - .003f, y + .015f), color);
            }
        }

        private static void AddAtmosphere(Painter p, LevelDefinition level, bool memoryVersion)
        {
            p.Rect(0, 0, 1, .025f, new Color(.02f, .025f, .03f, .32f));
            p.Rect(0, .975f, 1, .025f, new Color(.02f, .025f, .03f, .22f));
            p.Rect(0, 0, .018f, 1, new Color(.02f, .025f, .03f, .28f));
            p.Rect(.982f, 0, .018f, 1, new Color(.02f, .025f, .03f, .28f));
            for (int i = 0; i < 34; i++)
            {
                float x = ((i * 47 + level.Index * 13) % 97) / 100f + .015f;
                float y = ((i * 29 + level.Index * 19) % 91) / 100f + .03f;
                float r = .0015f + (i % 3) * .0007f;
                p.Circle(x, y, r, new Color(1f, .94f, .78f, memoryVersion ? .32f : .16f));
            }
            if (!memoryVersion) return;
            Color line = new Color(level.Accent.r, level.Accent.g, level.Accent.b, .88f);
            p.Rect(.026f, .028f, .948f, .010f, line);
            p.Rect(.026f, .962f, .948f, .010f, line);
            p.Rect(.026f, .028f, .007f, .944f, line);
            p.Rect(.967f, .028f, .007f, .944f, line);
            p.Circle(.075f, .90f, .018f, line);
            p.Line(.105f, .90f, .25f, .90f, .003f, line);
        }
    }
}
