using System.Security.Cryptography;
using BearAdventure.Rendering.PixelArt;
using BearAdventure.Domain.Gameplay;

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

try
{
    var buffers = new Dictionary<string, byte[]>();
    foreach ((string key, PixelRecipe recipe) in PixelRecipes.All)
    {
        byte[] first = PixelRasterizer.RenderRgba(recipe);
        byte[] second = PixelRasterizer.RenderRgba(recipe);
        Require(first.Length == recipe.Width * recipe.Height * 4, $"Bad byte size: {key}");
        Require(first.SequenceEqual(second), $"Nondeterministic art: {key}");
        Require(Enumerable.Range(0, recipe.Width * recipe.Height).Any(i => first[i * 4 + 3] != 0), $"Empty art: {key}");
        buffers.Add(key, first);
    }

    foreach (ItemType item in Enum.GetValues<ItemType>())
    {
        Require(PixelRecipes.All.TryGetValue($"icon/{item}", out PixelRecipe? icon), $"Missing icon: {item}");
        Require(icon!.Width == 24 && icon.Height == 24, $"Icon is not a 24px cell: {item}");
    }

    int mirrors = 0;
    foreach ((string key, byte[] mirrored) in buffers.Where(pair => pair.Key.EndsWith("/left", StringComparison.Ordinal)))
    {
        string originalKey = key[..^5];
        PixelRecipe recipe = PixelRecipes.All[originalKey];
        byte[] original = buffers[originalKey];
        for (int y = 0; y < recipe.Height; y++)
            for (int x = 0; x < recipe.Width; x++)
                for (int channel = 0; channel < 4; channel++)
                    Require(mirrored[(y * recipe.Width + x) * 4 + channel]
                        == original[(y * recipe.Width + recipe.Width - 1 - x) * 4 + channel], $"Mirror mismatch: {key}");
        mirrors++;
    }

    IReadOnlyList<PixelAtlasRegion> regions = PixelAtlasPacker.Pack(PixelRecipes.All);
    Require(regions.Count == PixelRecipes.All.Count, "Missing atlas region.");
    Require(regions.Select(r => r.Key).Distinct().Count() == regions.Count, "Duplicate atlas key.");
    for (int i = 0; i < regions.Count; i++)
    {
        PixelAtlasRegion a = regions[i];
        Require(a.X >= 2 && a.Y >= 2 && a.X + a.Width + 2 <= 1024 && a.Y + a.Height + 2 <= 1024,
            $"Out-of-bounds atlas region: {a.Key}");
        for (int j = 0; j < i; j++)
        {
            PixelAtlasRegion b = regions[j];
            bool overlap = a.Page == b.Page && a.X < b.X + b.Width && a.X + a.Width > b.X
                && a.Y < b.Y + b.Height && a.Y + a.Height > b.Y;
            Require(!overlap, $"Overlapping sprites: {a.Key}, {b.Key}");
        }
    }

    // Keep every pre-existing atlas key and dimension; game draw anchors are unchanged.
    foreach ((string key, int width, int height) in PixelCatalogFixtures.Legacy)
    {
        Require(PixelRecipes.All.TryGetValue(key, out PixelRecipe? recipe), $"Removed existing art key: {key}");
        Require(recipe!.Width == width && recipe.Height == height, $"Changed established sprite bounds: {key}");
    }
    foreach (string key in PixelCatalogFixtures.ReferenceKeys)
        Require(PixelRecipes.All.ContainsKey(key), $"Missing reference-family recipe: {key}");

    // Geometry fingerprints are deliberately updated after a visual style change.
    // These were cross-checked with an independent implementation of the rasterizer.
    var expected = new Dictionary<string, string>
    {
        ["bear/idle/0"] = "f3b72475388c8c9084156eba1e7a263506c75ee35b037ff45120b0defde8a3a6",
        ["bear/walk/3/left"] = "ad5291b3632cedb1a9e192f16ca27a471c054698d0223bc5a6f998dea023621c",
        ["bear/climb/0"] = "226190333427faf6b7d6023f0f1e3250ce623afeb6d3597fe9ce6bbd6b54f4ee",
        ["tree/1/0"] = "ee1cd444bf12299a3ae78e7429b6750036df8f4350603f367d6e479e674f5489",
        ["pine/1/0"] = "754ed955f0e6588a1e3fd526a5bcfc2180754dfb99c1c7acbc4663435e61a4bd",
        ["palm/1/0"] = "5a73cd4392000b25fa608be917129691ebe337cc745294031f89bb8b8606e96f",
        ["boat"] = "dc689453d9a207e74b0d0bf79c3c410b6b2cd1d5ca6b306edb26f8d1a76bdb2f",
        ["house/Forest"] = "4d47d82704b6d174c0ab2f1299ef7840264b516a56afaa38462d7eecf6c79666",
        ["icon/IronAxe"] = "88296092a4e098f556bb36a24c273a44554a6b0c671237b780d46c0450e58e4a",
        ["prop/Beehive/0"] = "f65b1c9ba9439a9e0d0c9d2f37698368656aa1b7fee69d532e85dde8e28d3c5e",
        ["resident/cat"] = "819a50c84ad61eb81da3b1754e0c9bd448d0e5e9d10f5638de45f5baabe64285",
        ["resident/monkey"] = "52105e713474ee71da1065bd9cfdbd7fd659ff9f46447feb0ced450bd843026f",
        ["prop/IceChest/open"] = "fdd779f7fb728fd652886584a1f08fc097d9bbfa2eb5f4715de8b4bd19cb85cc",
    };
    foreach ((string key, string digest) in expected)
        Require(Convert.ToHexString(SHA256.HashData(buffers[key])).Equals(digest, StringComparison.OrdinalIgnoreCase),
            $"Visual regression: {key}. Update the fixture deliberately when editing its geometry/palette.");

    Console.WriteLine($"PASS: {buffers.Count} pixel recipes, {Enum.GetValues<ItemType>().Length} item icons, {mirrors} mirrored poses, " +
        $"{regions.Max(r => r.Page) + 1} padded atlas pages.");
    Console.WriteLine("Pure renderer checks passed. This does not replace a Godot build or visual playtest.");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine($"PIXEL ART CHECK FAILED: {error.Message}");
    return 1;
}
