using System;
using BearAdventure.Domain.World;
using BearAdventure.Rendering.PixelArt;
using Godot;

namespace BearAdventure.Rendering;

public readonly record struct BiomePalette(Color Sky, Color Ground, Color Surface,
    Color Water, Color Accent, Color Distant)
{
    public static BiomePalette For(BiomeType biome)
    {
        PxColor ground = biome switch
        {
            BiomeType.Desert => PxColor.Sand1,
            BiomeType.Mountain or BiomeType.Snowy => PxColor.Stone1,
            _ => PxColor.Bark1,
        };
        PxColor surface = biome switch
        {
            BiomeType.Desert => PxColor.Sand3,
            BiomeType.Snowy => PxColor.Snow2,
            BiomeType.Mountain => PxColor.Stone2,
            BiomeType.Jungle => PxColor.Jungle2,
            _ => PxColor.Green2,
        };
        return new BiomePalette(
            PixelAtlas.ToColor(Enum.Parse<PxColor>($"Sky{biome}")), PixelAtlas.ToColor(ground),
            PixelAtlas.ToColor(surface), PixelAtlas.ToColor(PxColor.Water1),
            PixelAtlas.ToColor(PxColor.Gold2), PixelAtlas.ToColor(Enum.Parse<PxColor>($"Far{biome}")));
    }
}
