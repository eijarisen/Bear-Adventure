using System.Text.Json;

namespace BearAdventure.Persistence;

public sealed class SaveGameStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public GameSaveData? Load(
        string path,
        out string? warning)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        warning = null;

        if (TryRead(path, out GameSaveData? primary, out string? primaryError))
        {
            return primary;
        }

        string backupPath = path + ".bak";
        if (TryRead(backupPath, out GameSaveData? backup, out string? backupError))
        {
            warning =
                $"Primary save could not be loaded ({primaryError}). " +
                "Recovered from backup.";
            return backup;
        }

        if (File.Exists(path) || File.Exists(backupPath))
        {
            warning =
                $"No valid save could be loaded. Primary: {primaryError ?? "missing"}. " +
                $"Backup: {backupError ?? "missing"}.";
        }

        return null;
    }

    public void Save(string path, GameSaveData data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(data);

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string tempPath = path + ".tmp";
        string backupPath = path + ".bak";

        string json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(tempPath, json);

        // Verify that the newly written document can be parsed before it
        // replaces the current save.
        string verificationJson = File.ReadAllText(tempPath);
        GameSaveData? verification =
            JsonSerializer.Deserialize<GameSaveData>(
                verificationJson,
                JsonOptions);

        Validate(verification);

        if (File.Exists(path))
        {
            File.Copy(path, backupPath, overwrite: true);
        }

        File.Move(tempPath, path, overwrite: true);
    }

    private static bool TryRead(
        string path,
        out GameSaveData? data,
        out string? error)
    {
        data = null;
        error = null;

        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            data = JsonSerializer.Deserialize<GameSaveData>(
                json,
                JsonOptions);

            Validate(data);
            return true;
        }
        catch (Exception exception)
        {
            data = null;
            error = exception.Message;
            return false;
        }
    }

    private static void Validate(GameSaveData? data)
    {
        if (data is null)
        {
            throw new InvalidDataException("Save document is empty.");
        }

        if (data.FormatVersion != GameSaveData.CurrentFormatVersion)
        {
            throw new InvalidDataException(
                $"Unsupported save format version {data.FormatVersion}.");
        }

        if (data.GeneratorVersion
            != BearAdventure.Domain.World.WorldSeed.GeneratorVersion)
        {
            throw new InvalidDataException(
                $"Unsupported generator version {data.GeneratorVersion}.");
        }

        if (string.IsNullOrWhiteSpace(data.WorldSeed))
        {
            throw new InvalidDataException(
                "Save document has no world seed.");
        }

        data.DiscoveredIslandIds ??= new List<int> { 0 };
        data.Inventory ??= new Dictionary<string, int>();
        data.Islands ??= new Dictionary<int, IslandSaveData>();

        foreach (IslandSaveData island in data.Islands.Values)
        {
            island.HarvestedNaturalFeatureIds ??= new List<int>();
            island.NaturalRegrowthSeconds ??=
                new Dictionary<int, double>();
            island.PlacedObjects ??=
                new List<PlacedObjectSaveData>();
            island.MinedUndergroundCells ??=
                new List<UndergroundCellSaveData>();
            island.LootedGeneratedChestIds ??=
                new List<int>();
        }
    }
}
