using System.Collections.Generic;

namespace BearAdventure.Rendering.PixelArt;

// Authored geometry interpreted by PixelRasterizer at runtime. No source images are read.
// Shared palette; fixed two-screen-pixel grid; upper-left highlights. Artwork only.
public static partial class PixelRecipes
{
    private static void AddTiles(Dictionary<string, PixelRecipe> recipes)
    {
        recipes.Add("terrain/Forest/soil/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            P(PxColor.Bark0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Bark2, 6, 4, 3, 2),
            R(PxColor.Bark0, 6, 6, 3, 1),
            R(PxColor.Bark2, 18, 10, 2, 1),
            R(PxColor.Bark0, 17, 11, 4, 1),
            S(PxColor.Bark2, 4, 400, 8, 0, 0, 24, 24),
            S(PxColor.Bark0, 4, 800, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Forest/rock/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 101, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Forest/0", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Green0, 0, 0, 24, 5),
            R(PxColor.Green2, 0, 0, 24, 3),
            R(PxColor.Green3, 0, 0, 24, 1),
            R(PxColor.Green1, 0, 3, 3, 2),
            R(PxColor.Green4, 1, 0, 2, 1),
            R(PxColor.Green1, 4, 3, 3, 3),
            R(PxColor.Green4, 5, 0, 2, 1),
            R(PxColor.Green1, 8, 3, 3, 2),
            R(PxColor.Green4, 9, 0, 2, 1),
            R(PxColor.Green1, 12, 3, 3, 2),
            R(PxColor.Green4, 13, 0, 2, 1),
            R(PxColor.Green1, 16, 3, 3, 3),
            R(PxColor.Green4, 17, 0, 2, 1),
            R(PxColor.Green1, 20, 3, 3, 2),
            R(PxColor.Green4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Forest/soil/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            P(PxColor.Bark0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Bark2, 6, 4, 3, 2),
            R(PxColor.Bark0, 6, 6, 3, 1),
            R(PxColor.Bark2, 18, 10, 2, 1),
            R(PxColor.Bark0, 17, 11, 4, 1),
            S(PxColor.Bark2, 4, 401, 8, 0, 0, 24, 24),
            S(PxColor.Bark0, 4, 801, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Forest/rock/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 102, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Forest/1", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Green0, 0, 0, 24, 5),
            R(PxColor.Green2, 0, 0, 24, 3),
            R(PxColor.Green3, 0, 0, 24, 1),
            R(PxColor.Green1, 0, 3, 3, 2),
            R(PxColor.Green4, 1, 0, 2, 1),
            R(PxColor.Green1, 4, 3, 3, 2),
            R(PxColor.Green4, 5, 0, 2, 1),
            R(PxColor.Green1, 8, 3, 3, 3),
            R(PxColor.Green4, 9, 0, 2, 1),
            R(PxColor.Green1, 12, 3, 3, 2),
            R(PxColor.Green4, 13, 0, 2, 1),
            R(PxColor.Green1, 16, 3, 3, 2),
            R(PxColor.Green4, 17, 0, 2, 1),
            R(PxColor.Green1, 20, 3, 3, 3),
            R(PxColor.Green4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Forest/soil/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            P(PxColor.Bark0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Bark2, 6, 4, 3, 2),
            R(PxColor.Bark0, 6, 6, 3, 1),
            R(PxColor.Bark2, 18, 10, 2, 1),
            R(PxColor.Bark0, 17, 11, 4, 1),
            S(PxColor.Bark2, 4, 402, 8, 0, 0, 24, 24),
            S(PxColor.Bark0, 4, 802, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Forest/rock/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 103, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Forest/2", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Green0, 0, 0, 24, 5),
            R(PxColor.Green2, 0, 0, 24, 3),
            R(PxColor.Green3, 0, 0, 24, 1),
            R(PxColor.Green1, 0, 3, 3, 3),
            R(PxColor.Green4, 1, 0, 2, 1),
            R(PxColor.Green1, 4, 3, 3, 2),
            R(PxColor.Green4, 5, 0, 2, 1),
            R(PxColor.Green1, 8, 3, 3, 2),
            R(PxColor.Green4, 9, 0, 2, 1),
            R(PxColor.Green1, 12, 3, 3, 3),
            R(PxColor.Green4, 13, 0, 2, 1),
            R(PxColor.Green1, 16, 3, 3, 2),
            R(PxColor.Green4, 17, 0, 2, 1),
            R(PxColor.Green1, 20, 3, 3, 2),
            R(PxColor.Green4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Forest/soil/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            P(PxColor.Bark0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Bark2, 6, 4, 3, 2),
            R(PxColor.Bark0, 6, 6, 3, 1),
            R(PxColor.Bark2, 18, 10, 2, 1),
            R(PxColor.Bark0, 17, 11, 4, 1),
            S(PxColor.Bark2, 4, 403, 8, 0, 0, 24, 24),
            S(PxColor.Bark0, 4, 803, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Forest/rock/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 104, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Forest/3", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Green0, 0, 0, 24, 5),
            R(PxColor.Green2, 0, 0, 24, 3),
            R(PxColor.Green3, 0, 0, 24, 1),
            R(PxColor.Green1, 0, 3, 3, 2),
            R(PxColor.Green4, 1, 0, 2, 1),
            R(PxColor.Green1, 4, 3, 3, 3),
            R(PxColor.Green4, 5, 0, 2, 1),
            R(PxColor.Green1, 8, 3, 3, 2),
            R(PxColor.Green4, 9, 0, 2, 1),
            R(PxColor.Green1, 12, 3, 3, 2),
            R(PxColor.Green4, 13, 0, 2, 1),
            R(PxColor.Green1, 16, 3, 3, 3),
            R(PxColor.Green4, 17, 0, 2, 1),
            R(PxColor.Green1, 20, 3, 3, 2),
            R(PxColor.Green4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Desert/soil/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Sand1, 0, 0, 24, 24),
            P(PxColor.Sand0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Sand2, 6, 4, 3, 2),
            R(PxColor.Sand0, 6, 6, 3, 1),
            R(PxColor.Sand2, 18, 10, 2, 1),
            R(PxColor.Sand0, 17, 11, 4, 1),
            S(PxColor.Sand2, 41, 400, 8, 0, 0, 24, 24),
            S(PxColor.Sand0, 41, 800, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Desert/rock/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 101, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Desert/0", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Sand0, 0, 0, 24, 5),
            R(PxColor.Sand2, 0, 0, 24, 3),
            R(PxColor.Sand3, 0, 0, 24, 1),
            R(PxColor.Sand1, 0, 3, 3, 2),
            R(PxColor.Sand4, 1, 0, 2, 1),
            R(PxColor.Sand1, 4, 3, 3, 3),
            R(PxColor.Sand4, 5, 0, 2, 1),
            R(PxColor.Sand1, 8, 3, 3, 2),
            R(PxColor.Sand4, 9, 0, 2, 1),
            R(PxColor.Sand1, 12, 3, 3, 2),
            R(PxColor.Sand4, 13, 0, 2, 1),
            R(PxColor.Sand1, 16, 3, 3, 3),
            R(PxColor.Sand4, 17, 0, 2, 1),
            R(PxColor.Sand1, 20, 3, 3, 2),
            R(PxColor.Sand4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Desert/soil/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Sand1, 0, 0, 24, 24),
            P(PxColor.Sand0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Sand2, 6, 4, 3, 2),
            R(PxColor.Sand0, 6, 6, 3, 1),
            R(PxColor.Sand2, 18, 10, 2, 1),
            R(PxColor.Sand0, 17, 11, 4, 1),
            S(PxColor.Sand2, 41, 401, 8, 0, 0, 24, 24),
            S(PxColor.Sand0, 41, 801, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Desert/rock/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 102, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Desert/1", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Sand0, 0, 0, 24, 5),
            R(PxColor.Sand2, 0, 0, 24, 3),
            R(PxColor.Sand3, 0, 0, 24, 1),
            R(PxColor.Sand1, 0, 3, 3, 2),
            R(PxColor.Sand4, 1, 0, 2, 1),
            R(PxColor.Sand1, 4, 3, 3, 2),
            R(PxColor.Sand4, 5, 0, 2, 1),
            R(PxColor.Sand1, 8, 3, 3, 3),
            R(PxColor.Sand4, 9, 0, 2, 1),
            R(PxColor.Sand1, 12, 3, 3, 2),
            R(PxColor.Sand4, 13, 0, 2, 1),
            R(PxColor.Sand1, 16, 3, 3, 2),
            R(PxColor.Sand4, 17, 0, 2, 1),
            R(PxColor.Sand1, 20, 3, 3, 3),
            R(PxColor.Sand4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Desert/soil/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Sand1, 0, 0, 24, 24),
            P(PxColor.Sand0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Sand2, 6, 4, 3, 2),
            R(PxColor.Sand0, 6, 6, 3, 1),
            R(PxColor.Sand2, 18, 10, 2, 1),
            R(PxColor.Sand0, 17, 11, 4, 1),
            S(PxColor.Sand2, 41, 402, 8, 0, 0, 24, 24),
            S(PxColor.Sand0, 41, 802, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Desert/rock/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 103, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Desert/2", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Sand0, 0, 0, 24, 5),
            R(PxColor.Sand2, 0, 0, 24, 3),
            R(PxColor.Sand3, 0, 0, 24, 1),
            R(PxColor.Sand1, 0, 3, 3, 3),
            R(PxColor.Sand4, 1, 0, 2, 1),
            R(PxColor.Sand1, 4, 3, 3, 2),
            R(PxColor.Sand4, 5, 0, 2, 1),
            R(PxColor.Sand1, 8, 3, 3, 2),
            R(PxColor.Sand4, 9, 0, 2, 1),
            R(PxColor.Sand1, 12, 3, 3, 3),
            R(PxColor.Sand4, 13, 0, 2, 1),
            R(PxColor.Sand1, 16, 3, 3, 2),
            R(PxColor.Sand4, 17, 0, 2, 1),
            R(PxColor.Sand1, 20, 3, 3, 2),
            R(PxColor.Sand4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Desert/soil/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Sand1, 0, 0, 24, 24),
            P(PxColor.Sand0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Sand2, 6, 4, 3, 2),
            R(PxColor.Sand0, 6, 6, 3, 1),
            R(PxColor.Sand2, 18, 10, 2, 1),
            R(PxColor.Sand0, 17, 11, 4, 1),
            S(PxColor.Sand2, 41, 403, 8, 0, 0, 24, 24),
            S(PxColor.Sand0, 41, 803, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Desert/rock/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 104, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Desert/3", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Sand0, 0, 0, 24, 5),
            R(PxColor.Sand2, 0, 0, 24, 3),
            R(PxColor.Sand3, 0, 0, 24, 1),
            R(PxColor.Sand1, 0, 3, 3, 2),
            R(PxColor.Sand4, 1, 0, 2, 1),
            R(PxColor.Sand1, 4, 3, 3, 3),
            R(PxColor.Sand4, 5, 0, 2, 1),
            R(PxColor.Sand1, 8, 3, 3, 2),
            R(PxColor.Sand4, 9, 0, 2, 1),
            R(PxColor.Sand1, 12, 3, 3, 2),
            R(PxColor.Sand4, 13, 0, 2, 1),
            R(PxColor.Sand1, 16, 3, 3, 3),
            R(PxColor.Sand4, 17, 0, 2, 1),
            R(PxColor.Sand1, 20, 3, 3, 2),
            R(PxColor.Sand4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Jungle/soil/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            P(PxColor.Bark0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Bark2, 6, 4, 3, 2),
            R(PxColor.Bark0, 6, 6, 3, 1),
            R(PxColor.Bark2, 18, 10, 2, 1),
            R(PxColor.Bark0, 17, 11, 4, 1),
            S(PxColor.Bark2, 4, 400, 8, 0, 0, 24, 24),
            S(PxColor.Bark0, 4, 800, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Jungle/rock/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 101, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Jungle/0", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Jungle0, 0, 0, 24, 5),
            R(PxColor.Jungle2, 0, 0, 24, 3),
            R(PxColor.Jungle3, 0, 0, 24, 1),
            R(PxColor.Jungle1, 0, 3, 3, 2),
            R(PxColor.Jungle4, 1, 0, 2, 1),
            R(PxColor.Jungle1, 4, 3, 3, 3),
            R(PxColor.Jungle4, 5, 0, 2, 1),
            R(PxColor.Jungle1, 8, 3, 3, 2),
            R(PxColor.Jungle4, 9, 0, 2, 1),
            R(PxColor.Jungle1, 12, 3, 3, 2),
            R(PxColor.Jungle4, 13, 0, 2, 1),
            R(PxColor.Jungle1, 16, 3, 3, 3),
            R(PxColor.Jungle4, 17, 0, 2, 1),
            R(PxColor.Jungle1, 20, 3, 3, 2),
            R(PxColor.Jungle4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Jungle/soil/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            P(PxColor.Bark0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Bark2, 6, 4, 3, 2),
            R(PxColor.Bark0, 6, 6, 3, 1),
            R(PxColor.Bark2, 18, 10, 2, 1),
            R(PxColor.Bark0, 17, 11, 4, 1),
            S(PxColor.Bark2, 4, 401, 8, 0, 0, 24, 24),
            S(PxColor.Bark0, 4, 801, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Jungle/rock/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 102, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Jungle/1", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Jungle0, 0, 0, 24, 5),
            R(PxColor.Jungle2, 0, 0, 24, 3),
            R(PxColor.Jungle3, 0, 0, 24, 1),
            R(PxColor.Jungle1, 0, 3, 3, 2),
            R(PxColor.Jungle4, 1, 0, 2, 1),
            R(PxColor.Jungle1, 4, 3, 3, 2),
            R(PxColor.Jungle4, 5, 0, 2, 1),
            R(PxColor.Jungle1, 8, 3, 3, 3),
            R(PxColor.Jungle4, 9, 0, 2, 1),
            R(PxColor.Jungle1, 12, 3, 3, 2),
            R(PxColor.Jungle4, 13, 0, 2, 1),
            R(PxColor.Jungle1, 16, 3, 3, 2),
            R(PxColor.Jungle4, 17, 0, 2, 1),
            R(PxColor.Jungle1, 20, 3, 3, 3),
            R(PxColor.Jungle4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Jungle/soil/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            P(PxColor.Bark0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Bark2, 6, 4, 3, 2),
            R(PxColor.Bark0, 6, 6, 3, 1),
            R(PxColor.Bark2, 18, 10, 2, 1),
            R(PxColor.Bark0, 17, 11, 4, 1),
            S(PxColor.Bark2, 4, 402, 8, 0, 0, 24, 24),
            S(PxColor.Bark0, 4, 802, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Jungle/rock/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 103, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Jungle/2", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Jungle0, 0, 0, 24, 5),
            R(PxColor.Jungle2, 0, 0, 24, 3),
            R(PxColor.Jungle3, 0, 0, 24, 1),
            R(PxColor.Jungle1, 0, 3, 3, 3),
            R(PxColor.Jungle4, 1, 0, 2, 1),
            R(PxColor.Jungle1, 4, 3, 3, 2),
            R(PxColor.Jungle4, 5, 0, 2, 1),
            R(PxColor.Jungle1, 8, 3, 3, 2),
            R(PxColor.Jungle4, 9, 0, 2, 1),
            R(PxColor.Jungle1, 12, 3, 3, 3),
            R(PxColor.Jungle4, 13, 0, 2, 1),
            R(PxColor.Jungle1, 16, 3, 3, 2),
            R(PxColor.Jungle4, 17, 0, 2, 1),
            R(PxColor.Jungle1, 20, 3, 3, 2),
            R(PxColor.Jungle4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Jungle/soil/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            P(PxColor.Bark0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Bark2, 6, 4, 3, 2),
            R(PxColor.Bark0, 6, 6, 3, 1),
            R(PxColor.Bark2, 18, 10, 2, 1),
            R(PxColor.Bark0, 17, 11, 4, 1),
            S(PxColor.Bark2, 4, 403, 8, 0, 0, 24, 24),
            S(PxColor.Bark0, 4, 803, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Jungle/rock/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 104, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Jungle/3", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Jungle0, 0, 0, 24, 5),
            R(PxColor.Jungle2, 0, 0, 24, 3),
            R(PxColor.Jungle3, 0, 0, 24, 1),
            R(PxColor.Jungle1, 0, 3, 3, 2),
            R(PxColor.Jungle4, 1, 0, 2, 1),
            R(PxColor.Jungle1, 4, 3, 3, 3),
            R(PxColor.Jungle4, 5, 0, 2, 1),
            R(PxColor.Jungle1, 8, 3, 3, 2),
            R(PxColor.Jungle4, 9, 0, 2, 1),
            R(PxColor.Jungle1, 12, 3, 3, 2),
            R(PxColor.Jungle4, 13, 0, 2, 1),
            R(PxColor.Jungle1, 16, 3, 3, 3),
            R(PxColor.Jungle4, 17, 0, 2, 1),
            R(PxColor.Jungle1, 20, 3, 3, 2),
            R(PxColor.Jungle4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Snowy/soil/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Stone2, 6, 4, 3, 2),
            R(PxColor.Stone0, 6, 6, 3, 1),
            R(PxColor.Stone2, 18, 10, 2, 1),
            R(PxColor.Stone0, 17, 11, 4, 1),
            S(PxColor.Stone2, 32, 400, 8, 0, 0, 24, 24),
            S(PxColor.Stone0, 32, 800, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Snowy/rock/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 101, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Snowy/0", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Snow0, 0, 0, 24, 5),
            R(PxColor.Snow1, 0, 0, 24, 3),
            R(PxColor.Snow2, 0, 0, 24, 1),
            R(PxColor.Snow0, 0, 3, 3, 2),
            R(PxColor.Snow3, 1, 0, 2, 1),
            R(PxColor.Snow0, 4, 3, 3, 3),
            R(PxColor.Snow3, 5, 0, 2, 1),
            R(PxColor.Snow0, 8, 3, 3, 2),
            R(PxColor.Snow3, 9, 0, 2, 1),
            R(PxColor.Snow0, 12, 3, 3, 2),
            R(PxColor.Snow3, 13, 0, 2, 1),
            R(PxColor.Snow0, 16, 3, 3, 3),
            R(PxColor.Snow3, 17, 0, 2, 1),
            R(PxColor.Snow0, 20, 3, 3, 2),
            R(PxColor.Snow3, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Snowy/soil/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Stone2, 6, 4, 3, 2),
            R(PxColor.Stone0, 6, 6, 3, 1),
            R(PxColor.Stone2, 18, 10, 2, 1),
            R(PxColor.Stone0, 17, 11, 4, 1),
            S(PxColor.Stone2, 32, 401, 8, 0, 0, 24, 24),
            S(PxColor.Stone0, 32, 801, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Snowy/rock/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 102, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Snowy/1", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Snow0, 0, 0, 24, 5),
            R(PxColor.Snow1, 0, 0, 24, 3),
            R(PxColor.Snow2, 0, 0, 24, 1),
            R(PxColor.Snow0, 0, 3, 3, 2),
            R(PxColor.Snow3, 1, 0, 2, 1),
            R(PxColor.Snow0, 4, 3, 3, 2),
            R(PxColor.Snow3, 5, 0, 2, 1),
            R(PxColor.Snow0, 8, 3, 3, 3),
            R(PxColor.Snow3, 9, 0, 2, 1),
            R(PxColor.Snow0, 12, 3, 3, 2),
            R(PxColor.Snow3, 13, 0, 2, 1),
            R(PxColor.Snow0, 16, 3, 3, 2),
            R(PxColor.Snow3, 17, 0, 2, 1),
            R(PxColor.Snow0, 20, 3, 3, 3),
            R(PxColor.Snow3, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Snowy/soil/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Stone2, 6, 4, 3, 2),
            R(PxColor.Stone0, 6, 6, 3, 1),
            R(PxColor.Stone2, 18, 10, 2, 1),
            R(PxColor.Stone0, 17, 11, 4, 1),
            S(PxColor.Stone2, 32, 402, 8, 0, 0, 24, 24),
            S(PxColor.Stone0, 32, 802, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Snowy/rock/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 103, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Snowy/2", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Snow0, 0, 0, 24, 5),
            R(PxColor.Snow1, 0, 0, 24, 3),
            R(PxColor.Snow2, 0, 0, 24, 1),
            R(PxColor.Snow0, 0, 3, 3, 3),
            R(PxColor.Snow3, 1, 0, 2, 1),
            R(PxColor.Snow0, 4, 3, 3, 2),
            R(PxColor.Snow3, 5, 0, 2, 1),
            R(PxColor.Snow0, 8, 3, 3, 2),
            R(PxColor.Snow3, 9, 0, 2, 1),
            R(PxColor.Snow0, 12, 3, 3, 3),
            R(PxColor.Snow3, 13, 0, 2, 1),
            R(PxColor.Snow0, 16, 3, 3, 2),
            R(PxColor.Snow3, 17, 0, 2, 1),
            R(PxColor.Snow0, 20, 3, 3, 2),
            R(PxColor.Snow3, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Snowy/soil/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Stone2, 6, 4, 3, 2),
            R(PxColor.Stone0, 6, 6, 3, 1),
            R(PxColor.Stone2, 18, 10, 2, 1),
            R(PxColor.Stone0, 17, 11, 4, 1),
            S(PxColor.Stone2, 32, 403, 8, 0, 0, 24, 24),
            S(PxColor.Stone0, 32, 803, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Snowy/rock/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 104, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Snowy/3", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Snow0, 0, 0, 24, 5),
            R(PxColor.Snow1, 0, 0, 24, 3),
            R(PxColor.Snow2, 0, 0, 24, 1),
            R(PxColor.Snow0, 0, 3, 3, 2),
            R(PxColor.Snow3, 1, 0, 2, 1),
            R(PxColor.Snow0, 4, 3, 3, 3),
            R(PxColor.Snow3, 5, 0, 2, 1),
            R(PxColor.Snow0, 8, 3, 3, 2),
            R(PxColor.Snow3, 9, 0, 2, 1),
            R(PxColor.Snow0, 12, 3, 3, 2),
            R(PxColor.Snow3, 13, 0, 2, 1),
            R(PxColor.Snow0, 16, 3, 3, 3),
            R(PxColor.Snow3, 17, 0, 2, 1),
            R(PxColor.Snow0, 20, 3, 3, 2),
            R(PxColor.Snow3, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Mountain/soil/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Stone2, 6, 4, 3, 2),
            R(PxColor.Stone0, 6, 6, 3, 1),
            R(PxColor.Stone2, 18, 10, 2, 1),
            R(PxColor.Stone0, 17, 11, 4, 1),
            S(PxColor.Stone2, 32, 400, 8, 0, 0, 24, 24),
            S(PxColor.Stone0, 32, 800, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Mountain/rock/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 101, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Mountain/0", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Stone0, 0, 0, 24, 5),
            R(PxColor.Stone2, 0, 0, 24, 3),
            R(PxColor.Stone3, 0, 0, 24, 1),
            R(PxColor.Stone1, 0, 3, 3, 2),
            R(PxColor.Stone4, 1, 0, 2, 1),
            R(PxColor.Stone1, 4, 3, 3, 3),
            R(PxColor.Stone4, 5, 0, 2, 1),
            R(PxColor.Stone1, 8, 3, 3, 2),
            R(PxColor.Stone4, 9, 0, 2, 1),
            R(PxColor.Stone1, 12, 3, 3, 2),
            R(PxColor.Stone4, 13, 0, 2, 1),
            R(PxColor.Stone1, 16, 3, 3, 3),
            R(PxColor.Stone4, 17, 0, 2, 1),
            R(PxColor.Stone1, 20, 3, 3, 2),
            R(PxColor.Stone4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Mountain/soil/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Stone2, 6, 4, 3, 2),
            R(PxColor.Stone0, 6, 6, 3, 1),
            R(PxColor.Stone2, 18, 10, 2, 1),
            R(PxColor.Stone0, 17, 11, 4, 1),
            S(PxColor.Stone2, 32, 401, 8, 0, 0, 24, 24),
            S(PxColor.Stone0, 32, 801, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Mountain/rock/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 102, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Mountain/1", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Stone0, 0, 0, 24, 5),
            R(PxColor.Stone2, 0, 0, 24, 3),
            R(PxColor.Stone3, 0, 0, 24, 1),
            R(PxColor.Stone1, 0, 3, 3, 2),
            R(PxColor.Stone4, 1, 0, 2, 1),
            R(PxColor.Stone1, 4, 3, 3, 2),
            R(PxColor.Stone4, 5, 0, 2, 1),
            R(PxColor.Stone1, 8, 3, 3, 3),
            R(PxColor.Stone4, 9, 0, 2, 1),
            R(PxColor.Stone1, 12, 3, 3, 2),
            R(PxColor.Stone4, 13, 0, 2, 1),
            R(PxColor.Stone1, 16, 3, 3, 2),
            R(PxColor.Stone4, 17, 0, 2, 1),
            R(PxColor.Stone1, 20, 3, 3, 3),
            R(PxColor.Stone4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Mountain/soil/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Stone2, 6, 4, 3, 2),
            R(PxColor.Stone0, 6, 6, 3, 1),
            R(PxColor.Stone2, 18, 10, 2, 1),
            R(PxColor.Stone0, 17, 11, 4, 1),
            S(PxColor.Stone2, 32, 402, 8, 0, 0, 24, 24),
            S(PxColor.Stone0, 32, 802, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Mountain/rock/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 103, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Mountain/2", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Stone0, 0, 0, 24, 5),
            R(PxColor.Stone2, 0, 0, 24, 3),
            R(PxColor.Stone3, 0, 0, 24, 1),
            R(PxColor.Stone1, 0, 3, 3, 3),
            R(PxColor.Stone4, 1, 0, 2, 1),
            R(PxColor.Stone1, 4, 3, 3, 2),
            R(PxColor.Stone4, 5, 0, 2, 1),
            R(PxColor.Stone1, 8, 3, 3, 2),
            R(PxColor.Stone4, 9, 0, 2, 1),
            R(PxColor.Stone1, 12, 3, 3, 3),
            R(PxColor.Stone4, 13, 0, 2, 1),
            R(PxColor.Stone1, 16, 3, 3, 2),
            R(PxColor.Stone4, 17, 0, 2, 1),
            R(PxColor.Stone1, 20, 3, 3, 2),
            R(PxColor.Stone4, 21, 0, 2, 1),
        }));
        recipes.Add("terrain/Mountain/soil/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 1, 15, 7, 13, 9, 15, 16, 14, 22, 16, 23, 18, 15, 17, 8, 18, 1, 17),
            R(PxColor.Stone2, 6, 4, 3, 2),
            R(PxColor.Stone0, 6, 6, 3, 1),
            R(PxColor.Stone2, 18, 10, 2, 1),
            R(PxColor.Stone0, 17, 11, 4, 1),
            S(PxColor.Stone2, 32, 403, 8, 0, 0, 24, 24),
            S(PxColor.Stone0, 32, 803, 6, 0, 0, 24, 24),
        }));
        recipes.Add("terrain/Mountain/rock/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            P(PxColor.Stone0, 0, 8, 6, 8, 9, 12, 9, 20, 15, 20, 17, 24, 18, 24, 16, 19, 10, 19, 10, 11, 7, 7, 0, 7),
            L(PxColor.Stone0, 15, 0, 14, 5, 1),
            L(PxColor.Stone0, 14, 5, 20, 9, 1),
            L(PxColor.Stone0, 20, 9, 24, 9, 1),
            L(PxColor.Stone2, 1, 6, 6, 6, 1),
            L(PxColor.Stone2, 11, 18, 15, 18, 1),
            S(PxColor.Stone2, 32, 104, 6, 0, 0, 24, 24),
        }));
        recipes.Add("cap/Mountain/3", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Stone0, 0, 0, 24, 5),
            R(PxColor.Stone2, 0, 0, 24, 3),
            R(PxColor.Stone3, 0, 0, 24, 1),
            R(PxColor.Stone1, 0, 3, 3, 2),
            R(PxColor.Stone4, 1, 0, 2, 1),
            R(PxColor.Stone1, 4, 3, 3, 3),
            R(PxColor.Stone4, 5, 0, 2, 1),
            R(PxColor.Stone1, 8, 3, 3, 2),
            R(PxColor.Stone4, 9, 0, 2, 1),
            R(PxColor.Stone1, 12, 3, 3, 2),
            R(PxColor.Stone4, 13, 0, 2, 1),
            R(PxColor.Stone1, 16, 3, 3, 3),
            R(PxColor.Stone4, 17, 0, 2, 1),
            R(PxColor.Stone1, 20, 3, 3, 2),
            R(PxColor.Stone4, 21, 0, 2, 1),
        }));
        recipes.Add("cave/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Cave0, 0, 0, 24, 24),
            P(PxColor.Cave1, 0, 8, 8, 7, 12, 9, 12, 19, 9, 20, 0, 19),
            P(PxColor.Cave1, 15, 2, 23, 1, 24, 8, 20, 10, 15, 8),
            L(PxColor.Cave2, 2, 8, 8, 8, 1),
            R(PxColor.Cave1, 19, 17, 5, 5),
            S(PxColor.Cave1, 71, 911, 4, 0, 0, 24, 24),
        }));
        recipes.Add("water/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Water1, 0, 0, 24, 24),
            R(PxColor.Water2, 0, 4, 7, 1),
            R(PxColor.Water0, 4, 6, 4, 1),
            R(PxColor.Water2, 7, 13, 7, 1),
            R(PxColor.Water0, 11, 15, 4, 1),
            R(PxColor.Water2, 14, 21, 7, 1),
            R(PxColor.Water0, 18, 23, 4, 1),
        }));
        recipes.Add("waterline/0", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Water2, 0, 2, 24, 4),
            R(PxColor.Water3, 0, 1, 6, 2),
            R(PxColor.Foam, 1, 0, 3, 1),
            R(PxColor.Water3, 6, 2, 6, 2),
            R(PxColor.Foam, 7, 1, 3, 1),
            R(PxColor.Water3, 12, 1, 6, 2),
            R(PxColor.Foam, 13, 0, 3, 1),
            R(PxColor.Water3, 18, 1, 6, 2),
            R(PxColor.Foam, 19, 0, 3, 1),
        }));
        recipes.Add("block/Wood/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            R(PxColor.Bark2, 1, 0, 5, 24),
            R(PxColor.Bark3, 1, 0, 1, 24),
            R(PxColor.Bark0, 0, 0, 1, 24),
            L(PxColor.Bark1, 4, 3, 4, 13, 1),
            R(PxColor.Bark2, 7, 0, 5, 24),
            R(PxColor.Bark3, 7, 0, 1, 24),
            R(PxColor.Bark0, 6, 0, 1, 24),
            L(PxColor.Bark1, 10, 4, 10, 16, 1),
            R(PxColor.Bark2, 13, 0, 5, 24),
            R(PxColor.Bark3, 13, 0, 1, 24),
            R(PxColor.Bark0, 12, 0, 1, 24),
            L(PxColor.Bark1, 16, 5, 16, 19, 1),
            R(PxColor.Bark2, 19, 0, 5, 24),
            R(PxColor.Bark3, 19, 0, 1, 24),
            R(PxColor.Bark0, 18, 0, 1, 24),
            L(PxColor.Bark1, 22, 6, 22, 15, 1),
            R(PxColor.Bark4, 1, 0, 22, 1),
            R(PxColor.Bark0, 0, 23, 24, 1),
            R(PxColor.Bark1, 0, 21, 24, 2),
        }));
        recipes.Add("block/Stone/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            R(PxColor.Stone0, 0, 7, 24, 1),
            R(PxColor.Stone2, 0, 0, 24, 1),
            R(PxColor.Stone0, 4, 0, 1, 8),
            R(PxColor.Stone2, 6, 3, 4, 1),
            R(PxColor.Stone0, 0, 15, 24, 1),
            R(PxColor.Stone2, 0, 8, 24, 1),
            R(PxColor.Stone0, 4, 8, 1, 8),
            R(PxColor.Stone2, 6, 11, 4, 1),
            R(PxColor.Stone0, 0, 23, 24, 1),
            R(PxColor.Stone2, 0, 16, 24, 1),
            R(PxColor.Stone0, 4, 16, 1, 8),
            R(PxColor.Stone2, 6, 19, 4, 1),
        }));
        recipes.Add("block/Sandstone/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Sand1, 0, 0, 24, 24),
            R(PxColor.Sand0, 0, 7, 24, 1),
            R(PxColor.Sand2, 0, 0, 24, 1),
            R(PxColor.Sand0, 4, 0, 1, 8),
            R(PxColor.Sand2, 6, 3, 4, 1),
            R(PxColor.Sand0, 0, 15, 24, 1),
            R(PxColor.Sand2, 0, 8, 24, 1),
            R(PxColor.Sand0, 4, 8, 1, 8),
            R(PxColor.Sand2, 6, 11, 4, 1),
            R(PxColor.Sand0, 0, 23, 24, 1),
            R(PxColor.Sand2, 0, 16, 24, 1),
            R(PxColor.Sand0, 4, 16, 1, 8),
            R(PxColor.Sand2, 6, 19, 4, 1),
        }));
        recipes.Add("block/Cactus/0", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Jungle1, 0, 0, 24, 24),
            R(PxColor.Jungle0, 0, 7, 24, 1),
            R(PxColor.Jungle2, 0, 0, 24, 1),
            R(PxColor.Jungle0, 4, 0, 1, 8),
            R(PxColor.Jungle2, 6, 3, 4, 1),
            R(PxColor.Jungle0, 0, 15, 24, 1),
            R(PxColor.Jungle2, 0, 8, 24, 1),
            R(PxColor.Jungle0, 4, 8, 1, 8),
            R(PxColor.Jungle2, 6, 11, 4, 1),
            R(PxColor.Jungle0, 0, 23, 24, 1),
            R(PxColor.Jungle2, 0, 16, 24, 1),
            R(PxColor.Jungle0, 4, 16, 1, 8),
            R(PxColor.Jungle2, 6, 19, 4, 1),
        }));
        recipes.Add("cave/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Cave0, 0, 0, 24, 24),
            P(PxColor.Cave1, 0, 8, 8, 7, 12, 9, 12, 19, 9, 20, 0, 19),
            P(PxColor.Cave1, 15, 2, 23, 1, 24, 8, 20, 10, 15, 8),
            L(PxColor.Cave2, 2, 8, 8, 8, 1),
            R(PxColor.Cave1, 19, 17, 5, 5),
            S(PxColor.Cave1, 71, 912, 4, 0, 0, 24, 24),
        }));
        recipes.Add("water/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Water1, 0, 0, 24, 24),
            R(PxColor.Water2, 3, 4, 7, 1),
            R(PxColor.Water0, 7, 6, 4, 1),
            R(PxColor.Water2, 10, 13, 7, 1),
            R(PxColor.Water0, 14, 15, 4, 1),
            R(PxColor.Water2, 17, 21, 7, 1),
            R(PxColor.Water0, 1, 23, 4, 1),
        }));
        recipes.Add("waterline/1", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Water2, 0, 2, 24, 4),
            R(PxColor.Water3, 0, 2, 6, 2),
            R(PxColor.Foam, 1, 1, 3, 1),
            R(PxColor.Water3, 6, 1, 6, 2),
            R(PxColor.Foam, 7, 0, 3, 1),
            R(PxColor.Water3, 12, 1, 6, 2),
            R(PxColor.Foam, 13, 0, 3, 1),
            R(PxColor.Water3, 18, 2, 6, 2),
            R(PxColor.Foam, 19, 1, 3, 1),
        }));
        recipes.Add("block/Wood/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            R(PxColor.Bark2, 1, 0, 5, 24),
            R(PxColor.Bark3, 1, 0, 1, 24),
            R(PxColor.Bark0, 0, 0, 1, 24),
            L(PxColor.Bark1, 4, 4, 4, 14, 1),
            R(PxColor.Bark2, 7, 0, 5, 24),
            R(PxColor.Bark3, 7, 0, 1, 24),
            R(PxColor.Bark0, 6, 0, 1, 24),
            L(PxColor.Bark1, 10, 5, 10, 17, 1),
            R(PxColor.Bark2, 13, 0, 5, 24),
            R(PxColor.Bark3, 13, 0, 1, 24),
            R(PxColor.Bark0, 12, 0, 1, 24),
            L(PxColor.Bark1, 16, 6, 16, 13, 1),
            R(PxColor.Bark2, 19, 0, 5, 24),
            R(PxColor.Bark3, 19, 0, 1, 24),
            R(PxColor.Bark0, 18, 0, 1, 24),
            L(PxColor.Bark1, 22, 3, 22, 16, 1),
            R(PxColor.Bark4, 1, 0, 22, 1),
            R(PxColor.Bark0, 0, 23, 24, 1),
            R(PxColor.Bark1, 0, 21, 24, 2),
        }));
        recipes.Add("block/Stone/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            R(PxColor.Stone0, 0, 7, 24, 1),
            R(PxColor.Stone2, 0, 0, 24, 1),
            R(PxColor.Stone0, 7, 0, 1, 8),
            R(PxColor.Stone2, 9, 3, 4, 1),
            R(PxColor.Stone0, 0, 15, 24, 1),
            R(PxColor.Stone2, 0, 8, 24, 1),
            R(PxColor.Stone0, 7, 8, 1, 8),
            R(PxColor.Stone2, 9, 11, 4, 1),
            R(PxColor.Stone0, 0, 23, 24, 1),
            R(PxColor.Stone2, 0, 16, 24, 1),
            R(PxColor.Stone0, 7, 16, 1, 8),
            R(PxColor.Stone2, 9, 19, 4, 1),
        }));
        recipes.Add("block/Sandstone/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Sand1, 0, 0, 24, 24),
            R(PxColor.Sand0, 0, 7, 24, 1),
            R(PxColor.Sand2, 0, 0, 24, 1),
            R(PxColor.Sand0, 7, 0, 1, 8),
            R(PxColor.Sand2, 9, 3, 4, 1),
            R(PxColor.Sand0, 0, 15, 24, 1),
            R(PxColor.Sand2, 0, 8, 24, 1),
            R(PxColor.Sand0, 7, 8, 1, 8),
            R(PxColor.Sand2, 9, 11, 4, 1),
            R(PxColor.Sand0, 0, 23, 24, 1),
            R(PxColor.Sand2, 0, 16, 24, 1),
            R(PxColor.Sand0, 7, 16, 1, 8),
            R(PxColor.Sand2, 9, 19, 4, 1),
        }));
        recipes.Add("block/Cactus/1", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Jungle1, 0, 0, 24, 24),
            R(PxColor.Jungle0, 0, 7, 24, 1),
            R(PxColor.Jungle2, 0, 0, 24, 1),
            R(PxColor.Jungle0, 7, 0, 1, 8),
            R(PxColor.Jungle2, 9, 3, 4, 1),
            R(PxColor.Jungle0, 0, 15, 24, 1),
            R(PxColor.Jungle2, 0, 8, 24, 1),
            R(PxColor.Jungle0, 7, 8, 1, 8),
            R(PxColor.Jungle2, 9, 11, 4, 1),
            R(PxColor.Jungle0, 0, 23, 24, 1),
            R(PxColor.Jungle2, 0, 16, 24, 1),
            R(PxColor.Jungle0, 7, 16, 1, 8),
            R(PxColor.Jungle2, 9, 19, 4, 1),
        }));
        recipes.Add("cave/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Cave0, 0, 0, 24, 24),
            P(PxColor.Cave1, 0, 8, 8, 7, 12, 9, 12, 19, 9, 20, 0, 19),
            P(PxColor.Cave1, 15, 2, 23, 1, 24, 8, 20, 10, 15, 8),
            L(PxColor.Cave2, 2, 8, 8, 8, 1),
            R(PxColor.Cave1, 19, 17, 5, 5),
            S(PxColor.Cave1, 71, 913, 4, 0, 0, 24, 24),
        }));
        recipes.Add("water/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Water1, 0, 0, 24, 24),
            R(PxColor.Water2, 6, 4, 7, 1),
            R(PxColor.Water0, 10, 6, 4, 1),
            R(PxColor.Water2, 13, 13, 7, 1),
            R(PxColor.Water0, 17, 15, 4, 1),
            R(PxColor.Water2, 20, 21, 4, 1),
            R(PxColor.Water2, 0, 21, 3, 1),
            R(PxColor.Water0, 4, 23, 4, 1),
        }));
        recipes.Add("waterline/2", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Water2, 0, 2, 24, 4),
            R(PxColor.Water3, 0, 1, 6, 2),
            R(PxColor.Foam, 1, 0, 3, 1),
            R(PxColor.Water3, 6, 1, 6, 2),
            R(PxColor.Foam, 7, 0, 3, 1),
            R(PxColor.Water3, 12, 2, 6, 2),
            R(PxColor.Foam, 13, 1, 3, 1),
            R(PxColor.Water3, 18, 1, 6, 2),
            R(PxColor.Foam, 19, 0, 3, 1),
        }));
        recipes.Add("block/Wood/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            R(PxColor.Bark2, 1, 0, 5, 24),
            R(PxColor.Bark3, 1, 0, 1, 24),
            R(PxColor.Bark0, 0, 0, 1, 24),
            L(PxColor.Bark1, 4, 5, 4, 15, 1),
            R(PxColor.Bark2, 7, 0, 5, 24),
            R(PxColor.Bark3, 7, 0, 1, 24),
            R(PxColor.Bark0, 6, 0, 1, 24),
            L(PxColor.Bark1, 10, 6, 10, 18, 1),
            R(PxColor.Bark2, 13, 0, 5, 24),
            R(PxColor.Bark3, 13, 0, 1, 24),
            R(PxColor.Bark0, 12, 0, 1, 24),
            L(PxColor.Bark1, 16, 3, 16, 14, 1),
            R(PxColor.Bark2, 19, 0, 5, 24),
            R(PxColor.Bark3, 19, 0, 1, 24),
            R(PxColor.Bark0, 18, 0, 1, 24),
            L(PxColor.Bark1, 22, 4, 22, 17, 1),
            R(PxColor.Bark4, 1, 0, 22, 1),
            R(PxColor.Bark0, 0, 23, 24, 1),
            R(PxColor.Bark1, 0, 21, 24, 2),
        }));
        recipes.Add("block/Stone/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            R(PxColor.Stone0, 0, 7, 24, 1),
            R(PxColor.Stone2, 0, 0, 24, 1),
            R(PxColor.Stone0, 10, 0, 1, 8),
            R(PxColor.Stone2, 12, 3, 4, 1),
            R(PxColor.Stone0, 0, 15, 24, 1),
            R(PxColor.Stone2, 0, 8, 24, 1),
            R(PxColor.Stone0, 10, 8, 1, 8),
            R(PxColor.Stone2, 12, 11, 4, 1),
            R(PxColor.Stone0, 0, 23, 24, 1),
            R(PxColor.Stone2, 0, 16, 24, 1),
            R(PxColor.Stone0, 10, 16, 1, 8),
            R(PxColor.Stone2, 12, 19, 4, 1),
        }));
        recipes.Add("block/Sandstone/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Sand1, 0, 0, 24, 24),
            R(PxColor.Sand0, 0, 7, 24, 1),
            R(PxColor.Sand2, 0, 0, 24, 1),
            R(PxColor.Sand0, 10, 0, 1, 8),
            R(PxColor.Sand2, 12, 3, 4, 1),
            R(PxColor.Sand0, 0, 15, 24, 1),
            R(PxColor.Sand2, 0, 8, 24, 1),
            R(PxColor.Sand0, 10, 8, 1, 8),
            R(PxColor.Sand2, 12, 11, 4, 1),
            R(PxColor.Sand0, 0, 23, 24, 1),
            R(PxColor.Sand2, 0, 16, 24, 1),
            R(PxColor.Sand0, 10, 16, 1, 8),
            R(PxColor.Sand2, 12, 19, 4, 1),
        }));
        recipes.Add("block/Cactus/2", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Jungle1, 0, 0, 24, 24),
            R(PxColor.Jungle0, 0, 7, 24, 1),
            R(PxColor.Jungle2, 0, 0, 24, 1),
            R(PxColor.Jungle0, 10, 0, 1, 8),
            R(PxColor.Jungle2, 12, 3, 4, 1),
            R(PxColor.Jungle0, 0, 15, 24, 1),
            R(PxColor.Jungle2, 0, 8, 24, 1),
            R(PxColor.Jungle0, 10, 8, 1, 8),
            R(PxColor.Jungle2, 12, 11, 4, 1),
            R(PxColor.Jungle0, 0, 23, 24, 1),
            R(PxColor.Jungle2, 0, 16, 24, 1),
            R(PxColor.Jungle0, 10, 16, 1, 8),
            R(PxColor.Jungle2, 12, 19, 4, 1),
        }));
        recipes.Add("cave/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Cave0, 0, 0, 24, 24),
            P(PxColor.Cave1, 0, 8, 8, 7, 12, 9, 12, 19, 9, 20, 0, 19),
            P(PxColor.Cave1, 15, 2, 23, 1, 24, 8, 20, 10, 15, 8),
            L(PxColor.Cave2, 2, 8, 8, 8, 1),
            R(PxColor.Cave1, 19, 17, 5, 5),
            S(PxColor.Cave1, 71, 914, 4, 0, 0, 24, 24),
        }));
        recipes.Add("water/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Water1, 0, 0, 24, 24),
            R(PxColor.Water2, 9, 4, 7, 1),
            R(PxColor.Water0, 13, 6, 4, 1),
            R(PxColor.Water2, 16, 13, 7, 1),
            R(PxColor.Water0, 0, 15, 4, 1),
            R(PxColor.Water2, 23, 21, 1, 1),
            R(PxColor.Water2, 0, 21, 6, 1),
            R(PxColor.Water0, 7, 23, 4, 1),
        }));
        recipes.Add("waterline/3", new PixelRecipe(24, 6, new PixelCommand[]
        {
            R(PxColor.Water2, 0, 2, 24, 4),
            R(PxColor.Water3, 0, 1, 6, 2),
            R(PxColor.Foam, 1, 0, 3, 1),
            R(PxColor.Water3, 6, 2, 6, 2),
            R(PxColor.Foam, 7, 1, 3, 1),
            R(PxColor.Water3, 12, 1, 6, 2),
            R(PxColor.Foam, 13, 0, 3, 1),
            R(PxColor.Water3, 18, 1, 6, 2),
            R(PxColor.Foam, 19, 0, 3, 1),
        }));
        recipes.Add("block/Wood/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Bark1, 0, 0, 24, 24),
            R(PxColor.Bark2, 1, 0, 5, 24),
            R(PxColor.Bark3, 1, 0, 1, 24),
            R(PxColor.Bark0, 0, 0, 1, 24),
            L(PxColor.Bark1, 4, 6, 4, 16, 1),
            R(PxColor.Bark2, 7, 0, 5, 24),
            R(PxColor.Bark3, 7, 0, 1, 24),
            R(PxColor.Bark0, 6, 0, 1, 24),
            L(PxColor.Bark1, 10, 3, 10, 19, 1),
            R(PxColor.Bark2, 13, 0, 5, 24),
            R(PxColor.Bark3, 13, 0, 1, 24),
            R(PxColor.Bark0, 12, 0, 1, 24),
            L(PxColor.Bark1, 16, 4, 16, 15, 1),
            R(PxColor.Bark2, 19, 0, 5, 24),
            R(PxColor.Bark3, 19, 0, 1, 24),
            R(PxColor.Bark0, 18, 0, 1, 24),
            L(PxColor.Bark1, 22, 5, 22, 18, 1),
            R(PxColor.Bark4, 1, 0, 22, 1),
            R(PxColor.Bark0, 0, 23, 24, 1),
            R(PxColor.Bark1, 0, 21, 24, 2),
        }));
        recipes.Add("block/Stone/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Stone1, 0, 0, 24, 24),
            R(PxColor.Stone0, 0, 7, 24, 1),
            R(PxColor.Stone2, 0, 0, 24, 1),
            R(PxColor.Stone0, 13, 0, 1, 8),
            R(PxColor.Stone2, 15, 3, 4, 1),
            R(PxColor.Stone0, 0, 15, 24, 1),
            R(PxColor.Stone2, 0, 8, 24, 1),
            R(PxColor.Stone0, 13, 8, 1, 8),
            R(PxColor.Stone2, 15, 11, 4, 1),
            R(PxColor.Stone0, 0, 23, 24, 1),
            R(PxColor.Stone2, 0, 16, 24, 1),
            R(PxColor.Stone0, 13, 16, 1, 8),
            R(PxColor.Stone2, 15, 19, 4, 1),
        }));
        recipes.Add("block/Sandstone/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Sand1, 0, 0, 24, 24),
            R(PxColor.Sand0, 0, 7, 24, 1),
            R(PxColor.Sand2, 0, 0, 24, 1),
            R(PxColor.Sand0, 13, 0, 1, 8),
            R(PxColor.Sand2, 15, 3, 4, 1),
            R(PxColor.Sand0, 0, 15, 24, 1),
            R(PxColor.Sand2, 0, 8, 24, 1),
            R(PxColor.Sand0, 13, 8, 1, 8),
            R(PxColor.Sand2, 15, 11, 4, 1),
            R(PxColor.Sand0, 0, 23, 24, 1),
            R(PxColor.Sand2, 0, 16, 24, 1),
            R(PxColor.Sand0, 13, 16, 1, 8),
            R(PxColor.Sand2, 15, 19, 4, 1),
        }));
        recipes.Add("block/Cactus/3", new PixelRecipe(24, 24, new PixelCommand[]
        {
            R(PxColor.Jungle1, 0, 0, 24, 24),
            R(PxColor.Jungle0, 0, 7, 24, 1),
            R(PxColor.Jungle2, 0, 0, 24, 1),
            R(PxColor.Jungle0, 13, 0, 1, 8),
            R(PxColor.Jungle2, 15, 3, 4, 1),
            R(PxColor.Jungle0, 0, 15, 24, 1),
            R(PxColor.Jungle2, 0, 8, 24, 1),
            R(PxColor.Jungle0, 13, 8, 1, 8),
            R(PxColor.Jungle2, 15, 11, 4, 1),
            R(PxColor.Jungle0, 0, 23, 24, 1),
            R(PxColor.Jungle2, 0, 16, 24, 1),
            R(PxColor.Jungle0, 13, 16, 1, 8),
            R(PxColor.Jungle2, 15, 19, 4, 1),
        }));
    }
}
