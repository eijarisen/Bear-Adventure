using System;

namespace BearAdventure.Rendering.PixelArt;

// Single palette for all generated art. Values are sRGB RRGGBBAA, not linear RGB.
public enum PxColor
{
    Clear,
    Ink,
    Outline,
    Bark0,
    Bark1,
    Bark2,
    Bark3,
    Bark4,
    Fur0,
    Fur1,
    Fur2,
    Fur3,
    Fur4,
    Muzzle0,
    Muzzle1,
    Muzzle2,
    Green0,
    Green1,
    Green2,
    Green3,
    Green4,
    Pine0,
    Pine1,
    Pine2,
    Pine3,
    Pine4,
    Jungle0,
    Jungle1,
    Jungle2,
    Jungle3,
    Jungle4,
    Stone0,
    Stone1,
    Stone2,
    Stone3,
    Stone4,
    Snow0,
    Snow1,
    Snow2,
    Snow3,
    Sand0,
    Sand1,
    Sand2,
    Sand3,
    Sand4,
    Gold0,
    Gold1,
    Gold2,
    Gold3,
    Red0,
    Red1,
    Red2,
    Red3,
    Orange0,
    Orange1,
    Orange2,
    Blue0,
    Blue1,
    Blue2,
    Blue3,
    Purple0,
    Purple1,
    Purple2,
    Pink0,
    Pink1,
    Pink2,
    Water0,
    Water1,
    Water2,
    Water3,
    Foam,
    Cave0,
    Cave1,
    Cave2,
    SkyForest,
    SkyForestTop,
    MistForest,
    FarForest,
    MidForest,
    NearForest,
    SkyDesert,
    SkyDesertTop,
    MistDesert,
    FarDesert,
    MidDesert,
    NearDesert,
    SkyJungle,
    SkyJungleTop,
    MistJungle,
    FarJungle,
    MidJungle,
    NearJungle,
    SkySnowy,
    SkySnowyTop,
    MistSnowy,
    FarSnowy,
    MidSnowy,
    NearSnowy,
    SkyMountain,
    SkyMountainTop,
    MistMountain,
    FarMountain,
    MidMountain,
    NearMountain,
    CloudShade,
    Cloud,
    CloudLight,
    Panel0,
    Panel1,
    Panel2,
    PanelEdge,
    Text,
    DimText,
    Shadow,
}

public static class PixelPalette
{
    private static readonly uint[] Colors =
    {
        0x00000000u, // Clear
        0x28252dffu, // Ink
        0x3b2d2cffu, // Outline
        0x3d2a29ffu, // Bark0
        0x67412effu, // Bark1
        0x8b5936ffu, // Bark2
        0xb97d43ffu, // Bark3
        0xd8a461ffu, // Bark4
        0x583b2dffu, // Fur0
        0x805130ffu, // Fur1
        0xaa713cffu, // Fur2
        0xc58b4bffu, // Fur3
        0xdfaa61ffu, // Fur4
        0xac804affu, // Muzzle0
        0xdcad6effu, // Muzzle1
        0xf3cf8effu, // Muzzle2
        0x293c35ffu, // Green0
        0x3b5740ffu, // Green1
        0x55764bffu, // Green2
        0x799752ffu, // Green3
        0xb0bb71ffu, // Green4
        0x223a39ffu, // Pine0
        0x2e5149ffu, // Pine1
        0x467768ffu, // Pine2
        0x72a28affu, // Pine3
        0xa6c4a3ffu, // Pine4
        0x213936ffu, // Jungle0
        0x2c5744ffu, // Jungle1
        0x45834fffu, // Jungle2
        0x73a95affu, // Jungle3
        0xb4c772ffu, // Jungle4
        0x34383fffu, // Stone0
        0x535c64ffu, // Stone1
        0x76818bffu, // Stone2
        0xa0aab0ffu, // Stone3
        0xc9ccc3ffu, // Stone4
        0x718eaaffu, // Snow0
        0xa8bfccffu, // Snow1
        0xd9e1dfffu, // Snow2
        0xf4f1dbffu, // Snow3
        0x795544ffu, // Sand0
        0xa17850ffu, // Sand1
        0xc49d66ffu, // Sand2
        0xe2bf7fffu, // Sand3
        0xf3dba2ffu, // Sand4
        0x8b5b35ffu, // Gold0
        0xc38a35ffu, // Gold1
        0xe8b84affu, // Gold2
        0xf7dc7dffu, // Gold3
        0x723b40ffu, // Red0
        0xb24e48ffu, // Red1
        0xe07b60ffu, // Red2
        0xf4ad7fffu, // Red3
        0x945131ffu, // Orange0
        0xd2823cffu, // Orange1
        0xefb550ffu, // Orange2
        0x344e70ffu, // Blue0
        0x547ca4ffu, // Blue1
        0x90b6ceffu, // Blue2
        0xc9dce0ffu, // Blue3
        0x563e66ffu, // Purple0
        0x89548dffu, // Purple1
        0xba89b9ffu, // Purple2
        0x884f69ffu, // Pink0
        0xc57a98ffu, // Pink1
        0xedb0b8ffu, // Pink2
        0x294853ffu, // Water0
        0x376875ffu, // Water1
        0x538c96ffu, // Water2
        0x8cb7b7ffu, // Water3
        0xc2d7c4ffu, // Foam
        0x252c34ffu, // Cave0
        0x303b43ffu, // Cave1
        0x3d4a50ffu, // Cave2
        0xa2c3bdffu, // SkyForest
        0x82a9adffu, // SkyForestTop
        0xc2d4bfffu, // MistForest
        0x849f97ffu, // FarForest
        0x5f817bffu, // MidForest
        0x3e635affu, // NearForest
        0xddbe94ffu, // SkyDesert
        0xc89e83ffu, // SkyDesertTop
        0xedcda1ffu, // MistDesert
        0xc7a582ffu, // FarDesert
        0xac866effu, // MidDesert
        0x8b6d58ffu, // NearDesert
        0x8fb5a6ffu, // SkyJungle
        0x709890ffu, // SkyJungleTop
        0xb7cbb0ffu, // MistJungle
        0x789c8bffu, // FarJungle
        0x527b69ffu, // MidJungle
        0x335b4affu, // NearJungle
        0xb2c7d2ffu, // SkySnowy
        0x8faebbffu, // SkySnowyTop
        0xd8e0d7ffu, // MistSnowy
        0x94afbfffu, // FarSnowy
        0x6d8eaaffu, // MidSnowy
        0x4f738dffu, // NearSnowy
        0xb0bec0ffu, // SkyMountain
        0x91a3adffu, // SkyMountainTop
        0xd2d2c4ffu, // MistMountain
        0x929fa6ffu, // FarMountain
        0x6e7d8effu, // MidMountain
        0x4f616fffu, // NearMountain
        0xbdc8c1ffu, // CloudShade
        0xe3e4ceffu, // Cloud
        0xf5ebccffu, // CloudLight
        0x222d31ffu, // Panel0
        0x2d3a3cffu, // Panel1
        0x41504dffu, // Panel2
        0x786e51ffu, // PanelEdge
        0xefdfb6ffu, // Text
        0xa4b4a1ffu, // DimText
        0x18212768u, // Shadow
    };

    public static uint Rgba(PxColor color) => Colors[(int)color];
}
