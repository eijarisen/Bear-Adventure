using System.Text.Json;

namespace BearAdventure.Shell;

public sealed class GameSettingsStore
{
    private static readonly JsonSerializerOptions Options=new(){WriteIndented=true};
    public GameSettings Load(string path)
    {
        try
        {
            if(!File.Exists(path)) return new GameSettings();
            using var stream=File.OpenRead(path);
            var value=JsonSerializer.Deserialize<GameSettings>(stream,Options) ?? new GameSettings();
            value.Normalize(); return value;
        }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        { return new GameSettings(); }
    }
    public void Save(string path,GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings); settings.Normalize();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string tmp=path+".tmp";
        using(var stream=new FileStream(tmp,FileMode.Create,FileAccess.Write,FileShare.None))
        { JsonSerializer.Serialize(stream,settings,Options); stream.Flush(true); }
        File.Move(tmp,path,true);
    }
}
