using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using BearAdventure.Domain.Gameplay;

namespace BearAdventure.Rendering.PixelArt;

/// <summary>
/// Runtime packed texture atlas, indexed by the finite recipe catalog. Textures are
/// built once on the Godot main thread, shared by all islands and reused on every draw.
/// Nothing is read from disk or written to user://. No gameplay state is owned here.
/// </summary>
public static class PixelAtlas
{
    public const float WorldPixelSize = 2.0f;
    private static readonly Dictionary<string, AtlasTexture> Textures = new(StringComparer.Ordinal);
    private static readonly HashSet<string> Missing = new(StringComparer.Ordinal);

    private static readonly List<ImageTexture> Pages = new();
    public static int CachedTextureCount => Textures.Count;
    public static int PageCount => Pages.Count;

    public static Texture2D Get(string key)
    {
        if (!PixelRecipes.All.ContainsKey(key))
        {
            if (Missing.Add(key)) GD.PushWarning($"Unknown pixel-art recipe: {key}");
            key = "missing";
        }
        EnsureBuilt();
        return Textures[key];
    }

    private static void EnsureBuilt()
    {
        if (Textures.Count != 0) return;
        IReadOnlyList<PixelAtlasRegion> layout = PixelAtlasPacker.Pack(PixelRecipes.All);
        int pageCount = layout.Max(region => region.Page) + 1;
        for (int page = 0; page < pageCount; page++)
        {
            byte[] pageRgba = new byte[PixelAtlasPacker.PageSize * PixelAtlasPacker.PageSize * 4];
            foreach (PixelAtlasRegion region in layout.Where(region => region.Page == page))
            {
                PixelRecipe recipe = PixelRecipes.All[region.Key];
                byte[] sprite = PixelRasterizer.RenderRgba(recipe);
                for (int row = 0; row < region.Height; row++)
                    Buffer.BlockCopy(sprite, row * region.Width * 4, pageRgba,
                        ((region.Y + row) * PixelAtlasPacker.PageSize + region.X) * 4, region.Width * 4);
            }
            using Image image = Image.CreateFromData(PixelAtlasPacker.PageSize,
                PixelAtlasPacker.PageSize, false, Image.Format.Rgba8, pageRgba);
            Pages.Add(ImageTexture.CreateFromImage(image));
        }
        foreach (PixelAtlasRegion region in layout)
            Textures.Add(region.Key, new AtlasTexture
            {
                Atlas = Pages[region.Page],
                Region = new Rect2(region.X, region.Y, region.Width, region.Height),
                FilterClip = true,
            });
        GD.Print($"Bear Adventure pixel renderer ready: {Textures.Count} sprites in {Pages.Count} runtime atlas pages.");
    }

    public static Texture2D Icon(ItemType item) => Get($"icon/{item}");

    public static Color ToColor(PxColor color)
    {
        uint value = PixelPalette.Rgba(color);
        return new Color((value >> 24) / 255.0f, ((value >> 16) & 255u) / 255.0f,
            ((value >> 8) & 255u) / 255.0f, (value & 255u) / 255.0f);
    }

    // Visual variants only. No .NET Random, string.GetHashCode or simulation RNG.
    public static int Variant(int x, int y, int count = 4)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        return (int)(PixelRasterizer.Mix(unchecked((uint)x * 747796405u ^ (uint)y * 2891336453u)) % (uint)count);
    }

    public static void DrawBottom(CanvasItem canvas, string key, Vector2 feet, float scale = WorldPixelSize,
        bool flip = false, Color? modulate = null)
    {
        Texture2D texture = Get(flip ? key + "/left" : key);
        Vector2 size = new(texture.GetWidth() * scale, texture.GetHeight() * scale);
        DrawRect(canvas, texture, new Rect2(new Vector2(Mathf.Round(feet.X - size.X / 2.0f),
            Mathf.Round(feet.Y - size.Y)), size), modulate);
    }

    public static void DrawRect(CanvasItem canvas, string key, Rect2 rectangle,
        bool flip = false, Color? modulate = null) => DrawRect(canvas, Get(flip ? key + "/left" : key), rectangle, modulate);

    private static void DrawRect(CanvasItem canvas, Texture2D texture, Rect2 rectangle,
        Color? modulate)
    {
        canvas.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        // Mirrored actor/boat recipes are generated in the atlas; negative-size
        // draw rectangles are intentionally avoided to preserve foot and hull anchors.
        canvas.DrawTextureRect(texture, rectangle, false, modulate ?? Colors.White);
    }

    public static void Bar(CanvasItem canvas, Vector2 topLeft, float width, float progress)
    {
        topLeft = new Vector2(Mathf.Round(topLeft.X), Mathf.Round(topLeft.Y));
        canvas.DrawRect(new Rect2(topLeft, new Vector2(width, 8)), ToColor(PxColor.Ink));
        canvas.DrawRect(new Rect2(topLeft + Vector2.One, new Vector2(width - 2, 6)), ToColor(PxColor.Panel2));
        float filled = Mathf.Floor((width - 4) * Mathf.Clamp(progress, 0, 1) / 2.0f) * 2.0f;
        if (filled > 0)
        {
            canvas.DrawRect(new Rect2(topLeft + new Vector2(2, 2), new Vector2(filled, 4)), ToColor(PxColor.Gold1));
            canvas.DrawRect(new Rect2(topLeft + new Vector2(2, 2), new Vector2(filled, 1)), ToColor(PxColor.Gold3));
        }
    }
}
