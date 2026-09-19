using BearAdventure.Domain.World;
using Godot;

namespace BearAdventure.Rendering;

public readonly record struct BiomePalette(
    Color Sky,
    Color Ground,
    Color Surface,
    Color Water,
    Color Accent,
    Color Distant)
{
    public static BiomePalette For(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Forest => new(
                new Color(0.48f, 0.73f, 0.90f),
                new Color(0.30f, 0.20f, 0.13f),
                new Color(0.18f, 0.52f, 0.28f),
                new Color(0.08f, 0.43f, 0.78f),
                new Color(0.94f, 0.72f, 0.18f),
                new Color(0.29f, 0.48f, 0.39f)),
            BiomeType.Desert => new(
                new Color(0.94f, 0.76f, 0.56f),
                new Color(0.70f, 0.43f, 0.19f),
                new Color(0.92f, 0.71f, 0.35f),
                new Color(0.08f, 0.45f, 0.72f),
                new Color(0.84f, 0.34f, 0.18f),
                new Color(0.77f, 0.55f, 0.34f)),
            BiomeType.Jungle => new(
                new Color(0.13f, 0.33f, 0.21f),
                new Color(0.24f, 0.16f, 0.10f),
                new Color(0.08f, 0.38f, 0.16f),
                new Color(0.06f, 0.38f, 0.58f),
                new Color(0.98f, 0.75f, 0.20f),
                new Color(0.05f, 0.20f, 0.10f)),
            BiomeType.Snowy => new(
                new Color(0.78f, 0.86f, 0.94f),
                new Color(0.42f, 0.49f, 0.57f),
                new Color(0.94f, 0.97f, 1.00f),
                new Color(0.18f, 0.51f, 0.80f),
                new Color(0.40f, 0.66f, 0.82f),
                new Color(0.60f, 0.69f, 0.78f)),
            BiomeType.Mountain => new(
                new Color(0.38f, 0.43f, 0.50f),
                new Color(0.24f, 0.25f, 0.27f),
                new Color(0.47f, 0.49f, 0.52f),
                new Color(0.10f, 0.38f, 0.62f),
                new Color(0.81f, 0.63f, 0.26f),
                new Color(0.23f, 0.27f, 0.31f)),
            _ => throw new ArgumentOutOfRangeException(nameof(biome), biome, null),
        };
    }
}
