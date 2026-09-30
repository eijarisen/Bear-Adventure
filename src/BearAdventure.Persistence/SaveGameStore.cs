using System.Text.Json;
using BearAdventure.Domain.Gameplay;

namespace BearAdventure.Persistence;

public enum SaveLoadStatus { NewWorld, Loaded, RecoveredBackup, Failed }
public sealed record SaveLoadResult(SaveLoadStatus Status, GameSessionState? Session, string Message, int SourceFormat = 0);
public enum SaveWriteStage { BeforeWrite, AfterFlush, BeforeCommit }

public sealed class SaveGameStore
{
    private static readonly JsonSerializerOptions Options=new() {WriteIndented=false,PropertyNameCaseInsensitive=false,MaxDepth=64};
    // Used by regression tests to inject failures. Production leaves it null.
    public Action<SaveWriteStage>? FaultInjector {get;set;}
    public SaveLoadResult LoadSession(string path)
    {
        if(!File.Exists(path) && !File.Exists(path+".bak"))
            return new(SaveLoadStatus.NewWorld,null,"No save exists.");
        if(TryRead(path,out var primary,out var primaryError))
            return new(SaveLoadStatus.Loaded,primary!.ToSession(),primary.FormatVersion switch {
                1=>"Legacy save loaded; next save retains pre-v2/pre-v3/pre-v4 copies.",
                2=>"Schema-2 save loaded; next save retains pre-v3/pre-v4 copies.",
                3=>"Schema-3 save loaded; next save retains a pre-v4 copy.",
                _=>"Save loaded." },primary.FormatVersion);
        if(TryRead(path+".bak",out var backup,out var backupError))
            return new(SaveLoadStatus.RecoveredBackup,backup!.ToSession(),"Primary could not be loaded: "+primaryError+" Recovered the verified backup.",backup.FormatVersion);
        return new(SaveLoadStatus.Failed,null,$"No valid save could be loaded. Primary: {primaryError}. Backup: {backupError}. Existing files were not changed.");
    }
    public void Save(string path, GameSaveData data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path); ArgumentNullException.ThrowIfNull(data);
        if(data.FormatVersion!=GameSaveData.CurrentFormatVersion) throw new InvalidDataException("Only the current schema may be written.");
        _=data.ToSession(); // Validate the complete candidate, not just JSON syntax.
        string full=Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        string temporary=full+".tmp-"+Guid.NewGuid().ToString("N");
        try
        {
            FaultInjector?.Invoke(SaveWriteStage.BeforeWrite);
            using(var stream=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None))
            { JsonSerializer.Serialize(stream,data,Options); stream.Flush(flushToDisk:true); }
            FaultInjector?.Invoke(SaveWriteStage.AfterFlush);
            if(!TryRead(temporary,out _,out var error)) throw new InvalidDataException("New save did not validate: "+error);
            FaultInjector?.Invoke(SaveWriteStage.BeforeCommit);
            if(File.Exists(full))
            {
                bool verified=TryRead(full,out var previous,out _);
                if(verified && previous!.WorldSeed!=data.WorldSeed)
                    throw new InvalidDataException("Refusing to overwrite a different world. Use a separate save path.");
                if(verified)
                {
                    PreserveLegacy(full,full,previous!);
                    // Same-directory replacement; the valid previous generation becomes the backup.
                    File.Replace(temporary,full,full+".bak",ignoreMetadataErrors:true);
                }
                else
                {
                    if(!TryRead(full+".bak",out var backup,out _) || backup!.WorldSeed!=data.WorldSeed)
                        throw new InvalidDataException("No matching verified recovery backup. The original save is protected.");
                    PreserveLegacy(full,full+".bak",backup!);
                    string evidence=full+".rejected-"+DateTime.UtcNow.ToString("yyyyMMddHHmmss")+"-"+Guid.NewGuid().ToString("N");
                    File.Copy(full,evidence,overwrite:false);
                    // Never copy a corrupt primary over the good recovery backup.
                    File.Move(temporary,full,overwrite:true);
                }
            }
            else
            {
                if(File.Exists(full+".bak"))
                {
                    if(!TryRead(full+".bak",out var recovery,out _) || recovery!.WorldSeed!=data.WorldSeed)
                        throw new InvalidDataException("An unreadable or different-world backup protects this path.");
                    PreserveLegacy(full,full+".bak",recovery!);
                }
                File.Move(temporary,full,overwrite:false);
            }
        }
        finally { if(File.Exists(temporary)) { try { File.Delete(temporary); } catch(IOException) { } } }
    }
    private static void PreserveLegacy(string primaryPath,string sourcePath,GameSaveData source)
    {
        if(source.FormatVersion==1)
        {
            string legacyV2=primaryPath+".pre-v2";
            if(!File.Exists(legacyV2)) File.Copy(sourcePath,legacyV2,overwrite:false);
        }
        if(source.FormatVersion<3)
        {
            string legacyV3=primaryPath+".pre-v3";
            if(!File.Exists(legacyV3)) File.Copy(sourcePath,legacyV3,overwrite:false);
        }
        if(source.FormatVersion<4)
        {
            string legacyV4=primaryPath+".pre-v4";
            if(!File.Exists(legacyV4)) File.Copy(sourcePath,legacyV4,overwrite:false);
        }
    }
    public static string NewSeparateWorldPath(string originalPath) => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(originalPath))!,
        "bear-adventure-recovery-world-"+Guid.NewGuid().ToString("N")+".json");
    private static bool TryRead(string path,out GameSaveData? data,out string error)
    {
        data=null; error="missing";
        if(!File.Exists(path)) return false;
        try
        {
            if(new FileInfo(path).Length>256L*1024*1024) throw new InvalidDataException("Save exceeds the supported read size.");
            using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
            data=JsonSerializer.Deserialize<GameSaveData>(stream,Options);
            if(data is null) throw new InvalidDataException("Empty document.");
            _=data.ToSession();
            error=string.Empty; return true;
        }
        catch(Exception e) when(e is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or ArgumentException or InvalidOperationException or OverflowException)
        { data=null; error=e.Message; return false; }
    }
}
