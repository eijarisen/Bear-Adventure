using System;

namespace BearAdventure.Rendering.PixelArt;

/// <summary>
/// Tiny engine-independent rasterizer. All output pixels come from integer geometry;
/// no antialiasing, files, asset imports, framework RNG or gameplay state are used.
/// </summary>
public sealed class PixelRasterizer
{
    private readonly uint[] _pixels;
    public int Width { get; }
    public int Height { get; }

    private PixelRasterizer(int width, int height)
    {
        if (width < 1 || width > 2048 || height < 1 || height > 2048)
            throw new ArgumentOutOfRangeException(nameof(width), "Recipe dimensions must be 1..2048.");
        Width = width;
        Height = height;
        _pixels = new uint[checked(width * height)];
    }

    public static byte[] RenderRgba(PixelRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        var surface = new PixelRasterizer(recipe.Width, recipe.Height);
        foreach (PixelCommand command in recipe.Commands)
            surface.Execute(command);
        var rgba = new byte[surface._pixels.Length * 4];
        for (int i = 0; i < surface._pixels.Length; i++)
        {
            uint p = surface._pixels[i];
            int offset = i * 4;
            rgba[offset] = (byte)(p >> 24);
            rgba[offset + 1] = (byte)(p >> 16);
            rgba[offset + 2] = (byte)(p >> 8);
            rgba[offset + 3] = (byte)p;
        }
        return rgba;
    }

    private void Execute(PixelCommand command)
    {
        uint color = PixelPalette.Rgba(command.Color);
        int[] d = command.Data;
        switch (command.Operation)
        {
            case PixelOperation.Rectangle:
                RequireLength(d, 4);
                Rect(d[0], d[1], d[2], d[3], color);
                break;
            case PixelOperation.Polygon:
                if (d.Length < 6 || d.Length % 2 != 0)
                    throw new ArgumentException("Polygon needs at least three x/y pairs.");
                Polygon(d, color);
                break;
            case PixelOperation.Ellipse:
                RequireLength(d, 4);
                Ellipse(d[0], d[1], d[2], d[3], color);
                break;
            case PixelOperation.Line:
                RequireLength(d, 5);
                Line(d[0], d[1], d[2], d[3], d[4], color);
                break;
            case PixelOperation.MirrorHorizontal:
                RequireLength(d, 0);
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width / 2; x++)
                    {
                        int a = y * Width + x, b = y * Width + Width - 1 - x;
                        (_pixels[a], _pixels[b]) = (_pixels[b], _pixels[a]);
                    }
                break;
            case PixelOperation.Outline:
                RequireLength(d, 0);
                Outline(color);
                break;
            case PixelOperation.Speckle:
                RequireLength(d, 7);
                Speckle((PxColor)d[0], color, d[1], d[2], d[3], d[4], d[5], d[6]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command));
        }
    }

    private static void RequireLength(int[] values, int length)
    {
        if (values.Length != length)
            throw new ArgumentException("Malformed pixel drawing command.");
    }

    private void Set(int x, int y, uint color)
    {
        if ((uint)x < (uint)Width && (uint)y < (uint)Height)
            _pixels[y * Width + x] = color;
    }

    private uint Get(int x, int y) =>
        (uint)x < (uint)Width && (uint)y < (uint)Height ? _pixels[y * Width + x] : 0u;

    private void Rect(int x, int y, int width, int height, uint color)
    {
        for (int yy = Math.Max(0, y); yy < Math.Min(Height, y + height); yy++)
            for (int xx = Math.Max(0, x); xx < Math.Min(Width, x + width); xx++)
                Set(xx, yy, color);
    }

    private void Ellipse(int x, int y, int width, int height, uint color)
    {
        if (width <= 0 || height <= 0) return;
        double rx = width / 2.0, ry = height / 2.0;
        for (int yy = Math.Max(0, y); yy < Math.Min(Height, y + height); yy++)
            for (int xx = Math.Max(0, x); xx < Math.Min(Width, x + width); xx++)
            {
                double dx = (xx + 0.5 - x - rx) / rx;
                double dy = (yy + 0.5 - y - ry) / ry;
                if (dx * dx + dy * dy <= 1.0) Set(xx, yy, color);
            }
    }

    private void Line(int x, int y, int x1, int y1, int thickness, uint color)
    {
        if (thickness < 1 || thickness > 32)
            throw new ArgumentOutOfRangeException(nameof(thickness));
        int dx = Math.Abs(x1 - x), sx = x < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y), sy = y < y1 ? 1 : -1;
        int error = dx + dy;
        while (true)
        {
            Rect(x - thickness / 2, y - thickness / 2, thickness, thickness, color);
            if (x == x1 && y == y1) break;
            int twice = error * 2;
            if (twice >= dy) { error += dy; x += sx; }
            if (twice <= dx) { error += dx; y += sy; }
        }
    }

    private void Polygon(int[] xy, uint color)
    {
        int minX = Width, minY = Height, maxX = 0, maxY = 0;
        for (int i = 0; i < xy.Length; i += 2)
        {
            minX = Math.Min(minX, xy[i]); maxX = Math.Max(maxX, xy[i]);
            minY = Math.Min(minY, xy[i + 1]); maxY = Math.Max(maxY, xy[i + 1]);
        }
        for (int y = Math.Max(0, minY); y <= Math.Min(Height - 1, maxY); y++)
            for (int x = Math.Max(0, minX); x <= Math.Min(Width - 1, maxX); x++)
            {
                bool inside = false;
                double px = x + 0.5, py = y + 0.5;
                int j = xy.Length - 2;
                for (int i = 0; i < xy.Length; i += 2)
                {
                    int xi = xy[i], yi = xy[i + 1], xj = xy[j], yj = xy[j + 1];
                    if ((yi > py) != (yj > py) && px < (xj - xi) * (py - yi) / (yj - yi) + xi)
                        inside = !inside;
                    j = i;
                }
                if (inside) Set(x, y, color);
            }
    }

    private void Outline(uint color)
    {
        uint[] source = (uint[])_pixels.Clone();
        bool Opaque(int x, int y) => (uint)x < (uint)Width && (uint)y < (uint)Height
            && (source[y * Width + x] & 255u) != 0;
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (!Opaque(x, y) && (Opaque(x - 1, y) || Opaque(x + 1, y)
                    || Opaque(x, y - 1) || Opaque(x, y + 1)))
                    Set(x, y, color);
    }

    // A stable, stateless visual hash. It cannot advance WorldSeed/DeterministicRandom.
    public static uint Mix(uint value)
    {
        unchecked
        {
            value ^= value >> 16;
            value *= 0x7feb352du;
            value ^= value >> 15;
            value *= 0x846ca68bu;
            return value ^ (value >> 16);
        }
    }

    private void Speckle(PxColor baseColor, uint ink, int seed, int count,
        int x0, int y0, int width, int height)
    {
        if (width < 1 || height < 1 || count < 0 || count > 4096)
            throw new ArgumentOutOfRangeException(nameof(count));
        uint baseRgba = PixelPalette.Rgba(baseColor);
        for (int i = 0; i < count; i++)
        {
            uint z = Mix(unchecked((uint)seed + (uint)i * 747796405u));
            int x = x0 + (int)(z % (uint)width);
            int y = y0 + (int)(Mix(z) % (uint)height);
            int clusterWidth = 1 + (int)((z >> 16) % 2u);
            bool fits = true;
            for (int xx = x; xx < x + clusterWidth; xx++)
                fits &= Get(xx, y) == baseRgba;
            if (fits) Rect(x, y, clusterWidth, 1, ink);
        }
    }
}
