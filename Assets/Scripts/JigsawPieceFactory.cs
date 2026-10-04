using UnityEngine;

namespace RebuildHighSchool
{
    public static class JigsawPieceFactory
    {
        private const float TabRatio = .22f;

        public static float GetTabSize(float cellWidth, float cellHeight)
        {
            return Mathf.Min(cellWidth, cellHeight) * TabRatio;
        }

        public static Texture2D Create(Texture2D source, int columns, int rows, int row, int column,
            int seed, out Vector2 displaySize)
        {
            int cellWidth = source.width / columns;
            int cellHeight = source.height / rows;
            int margin = Mathf.Max(10, Mathf.RoundToInt(GetTabSize(cellWidth, cellHeight)));
            int width = cellWidth + margin * 2;
            int height = cellHeight + margin * 2;
            displaySize = new Vector2(width, height);

            int left = column == 0 ? 0 : -Seam(seed, row, column - 1, false);
            int right = column == columns - 1 ? 0 : Seam(seed, row, column, false);
            int top = row == 0 ? 0 : -Seam(seed, row - 1, column, true);
            int bottom = row == rows - 1 ? 0 : Seam(seed, row, column, true);
            int sourceBottom = source.height - (row + 1) * cellHeight;
            Color32[] sourcePixels = source.GetPixels32();
            var pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float localX = x - margin + .5f;
                float localY = y - margin + .5f;
                float coverage = Coverage(localX, localY, cellWidth, cellHeight, margin, left, right, top, bottom);
                if (coverage <= 0f) continue;

                int sampleX = Mathf.Clamp(column * cellWidth + Mathf.FloorToInt(localX), 0, source.width - 1);
                int sampleY = Mathf.Clamp(sourceBottom + Mathf.FloorToInt(localY), 0, source.height - 1);
                Color color = sourcePixels[sampleY * source.width + sampleX];
                bool edge = !Inside(localX - 1.5f, localY, cellWidth, cellHeight, margin, left, right, top, bottom)
                    || !Inside(localX + 1.5f, localY, cellWidth, cellHeight, margin, left, right, top, bottom)
                    || !Inside(localX, localY - 1.5f, cellWidth, cellHeight, margin, left, right, top, bottom)
                    || !Inside(localX, localY + 1.5f, cellWidth, cellHeight, margin, left, right, top, bottom);
                if (edge) color = Color.Lerp(color, new Color(.10f, .12f, .12f, 1f), .34f);
                color.a = coverage;
                pixels[y * width + x] = color;
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Jigsaw_" + row + "_" + column,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static int Seam(int seed, int row, int column, bool horizontal)
        {
            int value = seed * 97 + row * 53 + column * 31 + (horizontal ? 17 : 0);
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            return (value & 1) == 0 ? 1 : -1;
        }

        private static float Coverage(float x, float y, float width, float height, float radius,
            int left, int right, int top, int bottom)
        {
            const float offset = .28f;
            int hits = 0;
            if (Inside(x - offset, y - offset, width, height, radius, left, right, top, bottom)) hits++;
            if (Inside(x + offset, y - offset, width, height, radius, left, right, top, bottom)) hits++;
            if (Inside(x - offset, y + offset, width, height, radius, left, right, top, bottom)) hits++;
            if (Inside(x + offset, y + offset, width, height, radius, left, right, top, bottom)) hits++;
            return hits * .25f;
        }

        private static bool Inside(float x, float y, float width, float height, float radius,
            int left, int right, int top, int bottom)
        {
            bool inside = x >= 0f && x <= width && y >= 0f && y <= height;
            ApplyEdge(ref inside, left, DistanceSquared(x, y, 0f, height * .5f), radius);
            ApplyEdge(ref inside, right, DistanceSquared(x, y, width, height * .5f), radius);
            ApplyEdge(ref inside, bottom, DistanceSquared(x, y, width * .5f, 0f), radius);
            ApplyEdge(ref inside, top, DistanceSquared(x, y, width * .5f, height), radius);
            return inside;
        }

        private static void ApplyEdge(ref bool inside, int edge, float distanceSquared, float radius)
        {
            if (edge > 0 && distanceSquared <= radius * radius) inside = true;
            else if (edge < 0 && distanceSquared <= radius * radius) inside = false;
        }

        private static float DistanceSquared(float x, float y, float centerX, float centerY)
        {
            float dx = x - centerX;
            float dy = y - centerY;
            return dx * dx + dy * dy;
        }
    }
}
