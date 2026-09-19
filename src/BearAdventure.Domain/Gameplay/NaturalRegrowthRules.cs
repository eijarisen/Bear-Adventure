using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class NaturalRegrowthRules
{
    public static double? GetRegrowSeconds(NaturalFeatureKind kind)
    {
        return kind switch
        {
            NaturalFeatureKind.Tree
                or NaturalFeatureKind.Pine
                or NaturalFeatureKind.Palm
                    => 300.0,

            NaturalFeatureKind.Flower
                or NaturalFeatureKind.Grass
                or NaturalFeatureKind.Mushroom
                    => 90.0,

            NaturalFeatureKind.Bush
                    => 150.0,

            _ => null,
        };
    }
}
