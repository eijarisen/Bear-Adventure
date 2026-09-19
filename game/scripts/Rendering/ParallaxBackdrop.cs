using BearAdventure.Domain.World;
using Godot;

namespace BearAdventure.Rendering;

public partial class ParallaxBackdrop : Control
{
    private BiomePalette _palette =
        BiomePalette.For(BiomeType.Forest);

    private BiomeType _biome =
        BiomeType.Forest;

    private int _islandId;
    private float _cameraX;
    private double _elapsedSeconds;
    private Vector2 _lastViewportSize;

    public override void _Ready()
    {
        MouseFilter =
            MouseFilterEnum.Ignore;

        Position = Vector2.Zero;
        _lastViewportSize =
            GetViewportRect().Size;
        Size = _lastViewportSize;

        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        _elapsedSeconds += delta;

        Vector2 viewportSize =
            GetViewportRect().Size;

        if (viewportSize != _lastViewportSize)
        {
            _lastViewportSize =
                viewportSize;
            Size =
                viewportSize;
        }

        QueueRedraw();
    }

    public void Configure(
        BiomeType biome,
        int islandId)
    {
        _biome = biome;
        _islandId = islandId;
        _palette =
            BiomePalette.For(biome);

        QueueRedraw();
    }

    public void SetCameraX(float cameraX)
    {
        _cameraX = cameraX;
    }

    public override void _Draw()
    {
        Vector2 viewport =
            GetViewportRect().Size;

        if (viewport.X <= 0.0f
            || viewport.Y <= 0.0f)
        {
            return;
        }

        DrawRect(
            new Rect2(
                Vector2.Zero,
                viewport),
            _palette.Sky);

        DrawCloudLayer(
            viewport,
            factor: 0.055f);

        DrawHillLayer(
            viewport,
            Blend(
                _palette.Distant,
                _palette.Sky,
                0.28f),
            factor: 0.10f,
            spacing: 500.0f,
            radius: _biome == BiomeType.Mountain
                ? 260.0f
                : 210.0f,
            baseline: viewport.Y * 0.62f,
            salt: 83);

        DrawHillLayer(
            viewport,
            _palette.Distant,
            factor: 0.22f,
            spacing: 390.0f,
            radius: _biome == BiomeType.Mountain
                ? 220.0f
                : 175.0f,
            baseline: viewport.Y * 0.72f,
            salt: 211);

        DrawHillLayer(
            viewport,
            Blend(
                _palette.Distant,
                _palette.Ground,
                0.35f),
            factor: 0.38f,
            spacing: 310.0f,
            radius: _biome == BiomeType.Mountain
                ? 175.0f
                : 135.0f,
            baseline: viewport.Y * 0.82f,
            salt: 347);
    }

    private void DrawCloudLayer(
        Vector2 viewport,
        float factor)
    {
        Color cloud =
            _biome == BiomeType.Jungle
                ? new Color(
                    0.75f,
                    0.84f,
                    0.76f,
                    0.22f)
                : new Color(
                    1.0f,
                    1.0f,
                    1.0f,
                    0.30f);

        float spacing =
            430.0f;

        float drift =
            (float)(_elapsedSeconds * 6.0);

        float scroll =
            PositiveModulo(
                _cameraX * factor
                - drift
                + IslandPhase(149),
                spacing);

        for (float x = -spacing - scroll;
             x < viewport.X + spacing;
             x += spacing)
        {
            float y =
                90.0f
                + PositiveModulo(
                    x * 0.13f
                    + IslandPhase(29),
                    95.0f);

            DrawCircle(
                new Vector2(x, y),
                24.0f,
                cloud);
            DrawCircle(
                new Vector2(x + 23.0f, y - 9.0f),
                31.0f,
                cloud);
            DrawCircle(
                new Vector2(x + 53.0f, y),
                25.0f,
                cloud);
        }
    }

    private void DrawHillLayer(
        Vector2 viewport,
        Color color,
        float factor,
        float spacing,
        float radius,
        float baseline,
        int salt)
    {
        float scroll =
            PositiveModulo(
                _cameraX * factor
                + IslandPhase(salt),
                spacing);

        for (float x = -spacing - scroll;
             x < viewport.X + spacing;
             x += spacing)
        {
            float radiusVariation =
                PositiveModulo(
                    x * 0.17f
                    + IslandPhase(salt + 17),
                    48.0f)
                - 24.0f;

            float r =
                Math.Max(
                    70.0f,
                    radius + radiusVariation);

            DrawCircle(
                new Vector2(
                    x + spacing * 0.5f,
                    baseline + r),
                r,
                color);
        }

        DrawRect(
            new Rect2(
                0.0f,
                baseline,
                viewport.X,
                Math.Max(
                    0.0f,
                    viewport.Y - baseline)),
            color);
    }

    private float IslandPhase(int salt)
    {
        unchecked
        {
            int value =
                (_islandId * 73856093)
                ^ (salt * 19349663);

            return PositiveModulo(
                value,
                10_000);
        }
    }

    private static Color Blend(
        Color from,
        Color to,
        float amount)
    {
        amount =
            Mathf.Clamp(
                amount,
                0.0f,
                1.0f);

        return new Color(
            Mathf.Lerp(
                from.R,
                to.R,
                amount),
            Mathf.Lerp(
                from.G,
                to.G,
                amount),
            Mathf.Lerp(
                from.B,
                to.B,
                amount),
            Mathf.Lerp(
                from.A,
                to.A,
                amount));
    }

    private static float PositiveModulo(
        float value,
        float modulus)
    {
        float result =
            value % modulus;

        return result < 0.0f
            ? result + modulus
            : result;
    }

    private static float PositiveModulo(
        int value,
        int modulus)
    {
        int result =
            value % modulus;

        return result < 0
            ? result + modulus
            : result;
    }
}
