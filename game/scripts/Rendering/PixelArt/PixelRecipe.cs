using System;
using System.Collections.Generic;

namespace BearAdventure.Rendering.PixelArt;

public enum PixelOperation { Rectangle, Polygon, Ellipse, Line, Outline, Speckle, MirrorHorizontal }

public readonly record struct PixelCommand(PixelOperation Operation, PxColor Color, int[] Data);

/// <summary>Integer geometry and named palette entries. Never a bitmap or encoded image.</summary>
public sealed record PixelRecipe(int Width, int Height, PixelCommand[] Commands);

public static partial class PixelRecipes
{
    // One finite catalog. Entity IDs, world seed and simulation RNG never enter this cache.
    public static IReadOnlyDictionary<string, PixelRecipe> All { get; } = CreateCatalog();

    private static Dictionary<string, PixelRecipe> CreateCatalog()
    {
        var recipes = new Dictionary<string, PixelRecipe>(StringComparer.Ordinal);
        AddActors(recipes);
        AddNature(recipes);
        AddTiles(recipes);
        AddItems(recipes);
        AddWorld(recipes);
        AddScenery(recipes);
        AddReferenceExtras(recipes);
        return recipes;
    }

    private static PixelCommand R(PxColor color, int x, int y, int w, int h) =>
        new(PixelOperation.Rectangle, color, new[] { x, y, w, h });
    private static PixelCommand P(PxColor color, params int[] xy) =>
        new(PixelOperation.Polygon, color, xy);
    private static PixelCommand E(PxColor color, int x, int y, int w, int h) =>
        new(PixelOperation.Ellipse, color, new[] { x, y, w, h });
    private static PixelCommand L(PxColor color, int x0, int y0, int x1, int y1, int thickness) =>
        new(PixelOperation.Line, color, new[] { x0, y0, x1, y1, thickness });
    private static PixelCommand M(PxColor color) =>
        new(PixelOperation.MirrorHorizontal, color, Array.Empty<int>());
    private static PixelCommand O(PxColor color) =>
        new(PixelOperation.Outline, color, Array.Empty<int>());
    private static PixelCommand S(PxColor ink, int baseColor, int seed, int count,
        int x, int y, int width, int height) =>
        new(PixelOperation.Speckle, ink, new[] { baseColor, seed, count, x, y, width, height });
}
