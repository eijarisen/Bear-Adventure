using BearAdventure.Domain.World;

namespace BearAdventure.Persistence;

/// <summary>Materialized generation: visited islands never depend on a future generator rerun.</summary>
public sealed class IslandBaselineData
{
    public ulong IslandSeed {get;set;}
    public int GeneratorVersion {get;set;}
    public string Biome {get;set;} = string.Empty;
    public int WidthCells {get;set;}
    public int BoatSiteWidthCells {get;set;}
    public int[] SurfaceLevels {get;set;} = Array.Empty<int>();
    public List<NaturalFeatureSpawn> NaturalFeatures {get;set;} = new();
    public List<UndergroundCell> UndergroundOpenCells {get;set;} = new();
    public List<UndergroundOreSpawn> UndergroundOres {get;set;} = new();
    public int MineShaftLeftCell {get;set;}
    public int MineShaftWidthCells {get;set;}
    public List<StructureSnapshot> Structures {get;set;} = new();

    public static IslandBaselineData From(IslandDefinition d) => new() {
        IslandSeed=d.IslandSeed,GeneratorVersion=d.GeneratorVersion,Biome=d.Biome.ToString(),WidthCells=d.WidthCells,
        BoatSiteWidthCells=d.BoatSiteWidthCells,SurfaceLevels=(int[])d.SurfaceLevels.Clone(),NaturalFeatures=d.NaturalFeatures.ToList(),
        UndergroundOpenCells=d.UndergroundOpenCells.OrderBy(c=>c.CellX).ThenBy(c=>c.LogicalLevel).ToList(),
        UndergroundOres=d.UndergroundOres.Values.OrderBy(o=>o.Cell.CellX).ThenBy(o=>o.Cell.LogicalLevel).ToList(),
        MineShaftLeftCell=d.MineShaftLeftCell,MineShaftWidthCells=d.MineShaftWidthCells,
        Structures=d.GeneratedStructures.Select(s=>new StructureSnapshot {StructureId=s.StructureId,Kind=s.Kind.ToString(),
            CenterCellX=s.CenterCellX,WidthCells=s.WidthCells,BaseSurfaceLevel=s.BaseSurfaceLevel,ChestId=s.Chest.ChestId,
            ChestCellX=s.Chest.CellX,ChestLogicalLevel=s.Chest.LogicalLevel,
            Loot=s.Chest.Loot.ToDictionary(p=>p.Key.ToString(),p=>p.Value)}).ToList() };
    public IslandDefinition ToDefinition(int islandId)
    {
        GameSaveData.Require(GeneratorVersion==WorldSeed.GeneratorVersion,"Unknown materialized generator version.");
        GameSaveData.Require(WidthCells>=24 && WidthCells<=4096 && SurfaceLevels is not null && SurfaceLevels.Length==WidthCells &&
            SurfaceLevels.All(l=>l>=-100 && l<=100),"Invalid island dimensions.");
        GameSaveData.Require(BoatSiteWidthCells>=2 && BoatSiteWidthCells<WidthCells/2 && MineShaftWidthCells>0 &&
            MineShaftLeftCell>=0 && MineShaftLeftCell+MineShaftWidthCells<=WidthCells,"Invalid access points.");
        GameSaveData.Require(NaturalFeatures is not null && UndergroundOpenCells is not null && UndergroundOres is not null && Structures is not null,"Missing baseline arrays.");
        bool InBounds(UndergroundCell c)=>c.CellX>=0 && c.CellX<WidthCells && c.LogicalLevel>=-100 && c.LogicalLevel<=100;
        GameSaveData.Require(NaturalFeatures!.Select(f=>f.FeatureId).Distinct().Count()==NaturalFeatures.Count && NaturalFeatures.All(f=>
            f.FeatureId>=0 && Enum.IsDefined(f.Kind) && f.Variant>=0 && f.Variant<=100 && InBounds(new(f.CellX,f.SurfaceLevel)) &&
            f.SurfaceLevel==SurfaceLevels![f.CellX]),"Invalid natural feature baseline.");
        GameSaveData.Require(UndergroundOpenCells!.Distinct().Count()==UndergroundOpenCells.Count && UndergroundOpenCells.All(InBounds),"Invalid caves.");
        GameSaveData.Require(UndergroundOres!.Select(o=>o.Cell).Distinct().Count()==UndergroundOres.Count && UndergroundOres.All(o=>
            InBounds(o.Cell) && Enum.IsDefined(o.Kind) && o.Richness>0 && o.Richness<=100 && !UndergroundOpenCells.Contains(o.Cell)),"Invalid ore baseline.");
        var structures=new List<GeneratedStructureDefinition>(); var chestIds=new HashSet<int>(); var structureIds=new HashSet<int>();
        foreach(var s in Structures!)
        {
            GameSaveData.Require(s is not null,"Null structure.");
            GameSaveData.Require(structureIds.Add(s!.StructureId) && chestIds.Add(s.ChestId) && s.ChestId>=0 && s.WidthCells>0 && s.WidthCells<=WidthCells &&
                InBounds(new(s.ChestCellX,s.ChestLogicalLevel)) && InBounds(new(s.CenterCellX,s.BaseSurfaceLevel)),"Invalid structure identity/location.");
            var loot=GameSaveData.ReadInventory(s.Loot).Snapshot();
            structures.Add(new(s.StructureId,GameSaveData.Parse<GeneratedStructureKind>(s.Kind),s.CenterCellX,s.WidthCells,s.BaseSurfaceLevel,
                new(s.ChestId,s.ChestCellX,s.ChestLogicalLevel,loot)));
        }
        return new IslandDefinition {IslandId=islandId,IslandSeed=IslandSeed,GeneratorVersion=GeneratorVersion,
            Biome=GameSaveData.Parse<BiomeType>(Biome),WidthCells=WidthCells,BoatSiteWidthCells=BoatSiteWidthCells,
            SurfaceLevels=(int[])SurfaceLevels!.Clone(),NaturalFeatures=NaturalFeatures.ToArray(),UndergroundOpenCells=UndergroundOpenCells.ToHashSet(),
            UndergroundOres=UndergroundOres.ToDictionary(o=>o.Cell),MineShaftLeftCell=MineShaftLeftCell,MineShaftWidthCells=MineShaftWidthCells,
            GeneratedStructures=structures};
    }
}
public sealed class StructureSnapshot
{
    public int StructureId {get;set;}
    public string Kind {get;set;} = string.Empty;
    public int CenterCellX {get;set;}
    public int WidthCells {get;set;}
    public int BaseSurfaceLevel {get;set;}
    public int ChestId {get;set;}
    public int ChestCellX {get;set;}
    public int ChestLogicalLevel {get;set;}
    public Dictionary<string,int> Loot {get;set;} = new();
}
