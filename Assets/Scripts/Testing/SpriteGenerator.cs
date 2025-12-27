using UnityEngine;
using System.Collections.Generic;

namespace Rumbax.Testing
{
    /// <summary>
    /// Generates modern placeholder sprites at runtime for testing without art assets.
    /// </summary>
    public static class SpriteGenerator
    {
        private static Dictionary<string, Sprite> _cachedSprites = new Dictionary<string, Sprite>();

        // Modern defender tier colors (tier 1-5) - vibrant gradients
        public static readonly Color[] DefenderColors = new Color[]
        {
            new Color(0.3f, 0.85f, 0.5f),   // Tier 1: Emerald Green
            new Color(0.3f, 0.6f, 1f),      // Tier 2: Ocean Blue
            new Color(0.7f, 0.3f, 0.9f),    // Tier 3: Violet Purple
            new Color(1f, 0.6f, 0.1f),      // Tier 4: Golden Orange
            new Color(1f, 0.2f, 0.3f),      // Tier 5: Ruby Red (Legendary)
        };

        public static readonly Color[] DefenderGlowColors = new Color[]
        {
            new Color(0.5f, 1f, 0.7f, 0.5f),
            new Color(0.5f, 0.8f, 1f, 0.5f),
            new Color(0.85f, 0.5f, 1f, 0.5f),
            new Color(1f, 0.8f, 0.3f, 0.5f),
            new Color(1f, 0.4f, 0.5f, 0.6f),
        };

        // Enemy type colors - darker, menacing
        public static readonly Color[] EnemyColors = new Color[]
        {
            new Color(0.4f, 0.35f, 0.45f),  // Basic: Dark Gray-Purple
            new Color(0.5f, 0.35f, 0.25f),  // Tank: Brown
            new Color(0.25f, 0.55f, 0.5f),  // Fast: Teal
            new Color(0.7f, 0.15f, 0.2f),   // Boss: Crimson
        };

        /// <summary>
        /// Creates a simple square sprite.
        /// </summary>
        public static Sprite CreateSquare(int size, Color color, string name = null)
        {
            string key = $"square_{size}_{ColorToHex(color)}";
            if (_cachedSprites.TryGetValue(key, out Sprite cached))
                return cached;

            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels(pixels);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name ?? key;
            _cachedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates a modern grid cell with rounded corners and glow effect.
        /// </summary>
        public static Sprite CreateModernGridCell(int size = 64, bool highlighted = false)
        {
            string key = highlighted ? "modern_gridcell_highlight" : "modern_gridcell_normal";
            if (_cachedSprites.TryGetValue(key, out Sprite cached))
                return cached;

            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;

            Color fillColor = highlighted 
                ? new Color(0.25f, 0.45f, 0.35f, 0.9f) 
                : new Color(0.12f, 0.15f, 0.2f, 0.85f);
            Color borderColor = highlighted 
                ? new Color(0.4f, 0.9f, 0.5f, 1f) 
                : new Color(0.25f, 0.3f, 0.4f, 0.8f);
            Color innerGlow = highlighted
                ? new Color(0.3f, 0.6f, 0.4f, 0.5f)
                : new Color(0.2f, 0.25f, 0.35f, 0.3f);

            float radius = size * 0.15f; // Corner radius

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distFromEdge = GetRoundedRectDistance(x, y, size, size, radius);
                    
                    if (distFromEdge < -3)
                    {
                        // Inner area with subtle gradient
                        float gradientT = (float)y / size * 0.3f;
                        Color c = Color.Lerp(fillColor, innerGlow, gradientT);
                        tex.SetPixel(x, y, c);
                    }
                    else if (distFromEdge < 0)
                    {
                        // Border area
                        tex.SetPixel(x, y, borderColor);
                    }
                    else if (distFromEdge < 2)
                    {
                        // Soft edge
                        float alpha = 1f - (distFromEdge / 2f);
                        Color c = borderColor;
                        c.a *= alpha;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = key;
            _cachedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates a modern defender sprite with glow and depth.
        /// </summary>
        public static Sprite CreateDefender(int tier, int size = 64)
        {
            tier = Mathf.Clamp(tier, 1, 5);
            string key = $"defender_t{tier}";
            if (_cachedSprites.TryGetValue(key, out Sprite cached))
                return cached;

            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            
            Color mainColor = DefenderColors[tier - 1];
            Color darkColor = mainColor * 0.6f;
            darkColor.a = 1f;
            Color lightColor = mainColor * 1.3f;
            lightColor.a = 1f;

            // Draw defender shape (rounded square with details)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - size / 2f) / (size / 2f);
                    float ny = (y - size / 2f) / (size / 2f);
                    float dist = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny));

                    if (dist < 0.8f)
                    {
                        // Main body
                        if (dist < 0.3f)
                            tex.SetPixel(x, y, lightColor); // Center highlight
                        else if (dist < 0.6f)
                            tex.SetPixel(x, y, mainColor);  // Main color
                        else
                            tex.SetPixel(x, y, darkColor);  // Edge
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            // Add tier indicator (stars/dots at top)
            int dotY = size - 10;
            int startX = size / 2 - (tier - 1) * 6;
            for (int t = 0; t < tier; t++)
            {
                int dotX = startX + t * 12;
                for (int dy = -3; dy <= 3; dy++)
                {
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        if (dx * dx + dy * dy <= 9)
                        {
                            int px = dotX + dx;
                            int py = dotY + dy;
                            if (px >= 0 && px < size && py >= 0 && py < size)
                                tex.SetPixel(px, py, Color.yellow);
                        }
                    }
                }
            }

            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = key;
            _cachedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates an enemy sprite.
        /// </summary>
        public static Sprite CreateEnemy(int type, int size = 48)
        {
            type = Mathf.Clamp(type, 0, 3);
            string key = $"enemy_{type}";
            if (_cachedSprites.TryGetValue(key, out Sprite cached))
                return cached;

            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            
            Color mainColor = EnemyColors[type];
            Color eyeColor = Color.red;

            // Draw enemy shape (circle for basic, different shapes for others)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - size / 2f) / (size / 2f);
                    float ny = (y - size / 2f) / (size / 2f);
                    float dist = Mathf.Sqrt(nx * nx + ny * ny);

                    if (type == 1) // Tank - square
                    {
                        dist = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny));
                    }
                    else if (type == 2) // Fast - diamond
                    {
                        dist = Mathf.Abs(nx) + Mathf.Abs(ny);
                        dist *= 0.7f;
                    }

                    if (dist < 0.85f)
                    {
                        tex.SetPixel(x, y, mainColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            // Add eyes
            int eyeY = size / 2 + 5;
            DrawCircle(tex, size / 2 - 8, eyeY, 4, eyeColor);
            DrawCircle(tex, size / 2 + 8, eyeY, 4, eyeColor);

            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = key;
            _cachedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates a projectile sprite.
        /// </summary>
        public static Sprite CreateProjectile(Color color, int size = 16)
        {
            string key = $"projectile_{ColorToHex(color)}";
            if (_cachedSprites.TryGetValue(key, out Sprite cached))
                return cached;

            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - size / 2f) / (size / 2f);
                    float ny = (y - size / 2f) / (size / 2f);
                    float dist = Mathf.Sqrt(nx * nx + ny * ny);

                    if (dist < 0.7f)
                    {
                        float alpha = 1f - (dist / 0.7f) * 0.5f;
                        Color c = color;
                        c.a = alpha;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = key;
            _cachedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates a grid cell sprite.
        /// </summary>
        public static Sprite CreateGridCell(int size = 64, bool highlighted = false)
        {
            string key = highlighted ? "gridcell_highlight" : "gridcell_normal";
            if (_cachedSprites.TryGetValue(key, out Sprite cached))
                return cached;

            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;

            Color fillColor = highlighted 
                ? new Color(0.3f, 0.5f, 0.3f, 0.8f) 
                : new Color(0.2f, 0.25f, 0.3f, 0.8f);
            Color borderColor = highlighted 
                ? new Color(0.5f, 0.8f, 0.5f, 1f) 
                : new Color(0.3f, 0.35f, 0.4f, 1f);

            int border = 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x < border || x >= size - border || y < border || y >= size - border)
                        tex.SetPixel(x, y, borderColor);
                    else
                        tex.SetPixel(x, y, fillColor);
                }
            }

            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = key;
            _cachedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates a coin/gem icon.
        /// </summary>
        public static Sprite CreateCurrencyIcon(bool isGem, int size = 32)
        {
            string key = isGem ? "gem_icon" : "coin_icon";
            if (_cachedSprites.TryGetValue(key, out Sprite cached))
                return cached;

            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;

            Color mainColor = isGem ? new Color(0.2f, 0.8f, 0.9f) : new Color(1f, 0.85f, 0.2f);
            Color highlightColor = isGem ? new Color(0.6f, 1f, 1f) : new Color(1f, 0.95f, 0.6f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - size / 2f) / (size / 2f);
                    float ny = (y - size / 2f) / (size / 2f);
                    
                    float dist;
                    if (isGem)
                    {
                        // Diamond shape
                        dist = Mathf.Abs(nx) + Mathf.Abs(ny);
                    }
                    else
                    {
                        // Circle
                        dist = Mathf.Sqrt(nx * nx + ny * ny);
                    }

                    if (dist < 0.8f)
                    {
                        Color c = dist < 0.4f ? highlightColor : mainColor;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = key;
            _cachedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates an arrow sprite for path indicators.
        /// </summary>
        public static Sprite CreateArrow(int size, Color color)
        {
            string key = $"arrow_{ColorToHex(color)}";
            if (_cachedSprites.TryGetValue(key, out Sprite cached))
                return cached;

            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size - 0.5f;
                    
                    // Arrow pointing left
                    bool inArrow = nx < 0.7f && Mathf.Abs(ny) < (0.5f - nx * 0.5f);
                    inArrow = inArrow || (nx >= 0.3f && nx < 0.7f && Mathf.Abs(ny) < 0.15f);
                    
                    tex.SetPixel(x, y, inArrow ? color : Color.clear);
                }
            }

            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = key;
            _cachedSprites[key] = sprite;
            return sprite;
        }

        private static float GetRoundedRectDistance(int x, int y, int width, int height, float radius)
        {
            float halfW = width / 2f;
            float halfH = height / 2f;
            float px = Mathf.Abs(x - halfW);
            float py = Mathf.Abs(y - halfH);
            
            float dx = px - (halfW - radius);
            float dy = py - (halfH - radius);
            
            if (dx > 0 && dy > 0)
                return Mathf.Sqrt(dx * dx + dy * dy) - radius;
            else
                return Mathf.Max(px - halfW, py - halfH);
        }

        private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int px = cx + x;
                        int py = cy + y;
                        if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                            tex.SetPixel(px, py, color);
                    }
                }
            }
        }

        private static string ColorToHex(Color color)
        {
            return $"{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}";
        }
    }
}
