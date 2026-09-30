using System;
using BearAdventure.Domain.World;
using BearAdventure.Rendering.PixelArt;
using Godot;

namespace BearAdventure.Rendering;

/// <summary>Pixel scenery only. Camera/physics, time and island state are never written here.</summary>
public partial class ParallaxBackdrop : Control
{
    private BiomeType _biome = BiomeType.Forest;
    private int _islandId;
    private float _fallbackCameraX;
    private double _visualSeconds;
    private Vector2 _screenCenter;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        Position = Vector2.Zero;
        Size = GetViewportRect().Size;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        _visualSeconds += delta;
        Vector2 size = GetViewportRect().Size;
        if (Size != size) Size = size;
        Camera2D? camera = GetViewport().GetCamera2D();
        // Follow the actual smoothed, clamped camera, not an estimate from bear X.
        _screenCenter = camera is not null ? camera.GetScreenCenterPosition() : new Vector2(_fallbackCameraX, -100);
        QueueRedraw();
    }

    public void Configure(BiomeType biome, int islandId)
    {
        _biome = biome;
        _islandId = islandId;
        QueueRedraw();
    }

    public void SetCameraX(float cameraX) => _fallbackCameraX = cameraX;

    public override void _Draw()
    {
        Vector2 view = GetViewportRect().Size;
        if (view.X < 1 || view.Y < 1) return;
        Color high = PixelAtlas.ToColor(Enum.Parse<PxColor>($"Sky{_biome}Top"));
        Color low = PixelAtlas.ToColor(Enum.Parse<PxColor>($"Mist{_biome}"));
        // Discrete bands, not smooth gradients or a screen-wide blur filter.
        const int bands = 12;
        for (int band = 0; band < bands; band++)
        {
            float top = Mathf.Floor(view.Y * band / bands);
            float bottom = Mathf.Ceil(view.Y * (band + 1) / bands);
            DrawRect(new Rect2(0, top, view.X, bottom - top), high.Lerp(low, band / (float)(bands - 1)));
        }
        PixelAtlas.DrawBottom(this, "sun", new Vector2(view.X * 0.77f, view.Y * 0.27f), 2.0f,
            modulate: new Color(1, 1, 1, _biome == BiomeType.Jungle ? 0.55f : 0.8f));
        DrawClouds(view);
        DrawLandscape(view, 0, 0.10f, 0.44f);
        DrawLandscape(view, 1, 0.22f, 0.50f);
        DrawLandscape(view, 2, 0.38f, 0.57f);
    }

    private void DrawLandscape(Vector2 viewport, int layer, float factor, float screenBaseline)
    {
        string key = $"landscape/{_biome}/{layer}";
        Texture2D strip = PixelAtlas.Get(key);
        float width = strip.GetWidth() * 2.0f;
        float height = strip.GetHeight() * 2.0f;
        // Modulo moves fixed silhouettes; it never changes an individual hill's size.
        float phase = PixelAtlas.Variant(_islandId, layer, 384);
        float offset = Mathf.PosMod(_screenCenter.X * factor + phase, width);
        float top = Mathf.Floor(viewport.Y * screenBaseline - 150 - (_screenCenter.Y + 100) * factor);
        for (float x = -width - offset; x < viewport.X + width; x += width)
            PixelAtlas.DrawRect(this, key, new Rect2(Mathf.Floor(x), top, width, height));
        // Fill only BELOW the silhouette's bottom; never paint over the hills.
        float bottom = top + height;
        if (bottom < viewport.Y)
        {
            PxColor fill = Enum.Parse<PxColor>($"{(layer == 0 ? "Far" : layer == 1 ? "Mid" : "Near")}{_biome}");
            DrawRect(new Rect2(0, bottom, viewport.X, viewport.Y - bottom), PixelAtlas.ToColor(fill));
        }
    }

    private void DrawClouds(Vector2 viewport)
    {
        const float spacing = 320.0f;
        float drift = (float)(_visualSeconds % 3600.0) * 4.0f;
        float scroll = _screenCenter.X * 0.055f - drift;
        int first = Mathf.FloorToInt(scroll / spacing) - 1;
        int count = Mathf.CeilToInt(viewport.X / spacing) + 3;
        for (int n = first; n < first + count; n++)
        {
            int variant = PixelAtlas.Variant(n, _islandId, 3);
            float x = n * spacing - scroll;
            float y = 100 + PixelAtlas.Variant(n, _islandId + 149, 75) - (_screenCenter.Y + 100) * 0.025f;
            PixelAtlas.DrawBottom(this, $"cloud/{variant}", new Vector2(Mathf.Floor(x), Mathf.Floor(y)), 2.0f,
                modulate: new Color(1, 1, 1, 0.75f));
        }
    }
}
