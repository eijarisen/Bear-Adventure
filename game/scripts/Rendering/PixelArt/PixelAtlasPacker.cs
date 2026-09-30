using System;
using System.Collections.Generic;
using System.Linq;

namespace BearAdventure.Rendering.PixelArt;

public readonly record struct PixelAtlasRegion(string Key, int Page, int X, int Y, int Width, int Height);

/// <summary>Deterministic padded shelf packing, independent of Godot and game state.</summary>
public static class PixelAtlasPacker
{
    public const int PageSize = 1024;
    public const int Padding = 2;

    public static IReadOnlyList<PixelAtlasRegion> Pack(IReadOnlyDictionary<string, PixelRecipe> catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var regions = new List<PixelAtlasRegion>();
        int page = 0, x = Padding, y = Padding, rowHeight = 0;
        foreach (var pair in catalog.OrderByDescending(p => p.Value.Height)
            .ThenByDescending(p => p.Value.Width).ThenBy(p => p.Key, StringComparer.Ordinal))
        {
            PixelRecipe recipe = pair.Value;
            if (recipe.Width + 2 * Padding > PageSize || recipe.Height + 2 * Padding > PageSize)
                throw new ArgumentException($"Sprite too large for the pixel atlas: {pair.Key}");
            if (x + recipe.Width + Padding > PageSize)
            {
                x = Padding;
                y += rowHeight + Padding * 2;
                rowHeight = 0;
            }
            if (y + recipe.Height + Padding > PageSize)
            {
                page++;
                x = Padding;
                y = Padding;
                rowHeight = 0;
            }
            regions.Add(new PixelAtlasRegion(pair.Key, page, x, y, recipe.Width, recipe.Height));
            x += recipe.Width + Padding * 2;
            rowHeight = Math.Max(rowHeight, recipe.Height);
        }
        return regions;
    }
}
