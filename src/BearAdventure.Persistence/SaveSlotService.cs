using BearAdventure.Domain.Gameplay;

namespace BearAdventure.Persistence;

public sealed record SaveSlotSummary(
    int Slot,
    string Path,
    bool Exists,
    bool Loadable,
    string Seed,
    int CurrentIslandId,
    DateTime? ModifiedUtc,
    string StatusText);

public sealed class SaveSlotService
{
    public const int SlotCount = 3;
    private readonly string _directory;

    public SaveSlotService(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = Path.GetFullPath(directory);
    }

    public string PathForSlot(int slot) => slot switch
    {
        1 => Path.Combine(_directory,"bear-adventure-save.json"),
        2 => Path.Combine(_directory,"bear-adventure-slot-2.json"),
        3 => Path.Combine(_directory,"bear-adventure-slot-3.json"),
        _ => throw new ArgumentOutOfRangeException(nameof(slot)),
    };

    public IReadOnlyList<SaveSlotSummary> InspectAll(SaveGameStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        var result = new List<SaveSlotSummary>(SlotCount);
        for(int slot=1;slot<=SlotCount;slot++) result.Add(Inspect(store,slot));
        return result;
    }

    public SaveSlotSummary Inspect(SaveGameStore store,int slot)
    {
        ArgumentNullException.ThrowIfNull(store);
        string path=PathForSlot(slot);
        bool exists=File.Exists(path) || File.Exists(path+".bak");
        if(!exists) return new(slot,path,false,false,string.Empty,0,null,"Empty slot");
        var loaded=store.LoadSession(path);
        if(loaded.Session is null)
            return new(slot,path,true,false,string.Empty,0,LatestModified(path),loaded.Message);
        return new(slot,path,true,true,loaded.Session.Seed,loaded.Session.CurrentIslandId,LatestModified(path),
            loaded.Status==SaveLoadStatus.RecoveredBackup?"Recovered backup available":"Ready");
    }

    public void Reset(int slot)
    {
        string path=PathForSlot(slot);
        string directory=Path.GetDirectoryName(path)!;
        string file=Path.GetFileName(path);
        string[] exact={path,path+".bak",path+".pre-v2",path+".pre-v3",path+".pre-v4",path+".pre-v5"};
        foreach(string candidate in exact) if(File.Exists(candidate)) File.Delete(candidate);
        if(Directory.Exists(directory))
        {
            foreach(string candidate in Directory.EnumerateFiles(directory,file+".rejected-*")) File.Delete(candidate);
            foreach(string candidate in Directory.EnumerateFiles(directory,file+".tmp-*")) File.Delete(candidate);
        }
    }

    public bool IsEmpty(int slot)
    {
        string path=PathForSlot(slot);
        return !File.Exists(path) && !File.Exists(path+".bak");
    }

    private static DateTime? LatestModified(string path)
    {
        DateTime? best=null;
        foreach(string candidate in new[]{path,path+".bak"})
        {
            if(!File.Exists(candidate)) continue;
            DateTime value=File.GetLastWriteTimeUtc(candidate);
            if(!best.HasValue || value>best.Value) best=value;
        }
        return best;
    }
}
