using BearAdventure.Domain.Random;
using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class ResidentService
{
    private static readonly string[] BearNames = ["Rowan","Bramble","Hazel","Moss","Maple","Cedar"];
    private static readonly string[] CatNames = ["Ember","Poppy","Miso","Saffron","Pebble","Clover"];
    private static readonly string[] PolarNames = ["Lumi","Frost","Nanuq","Snow","Tundra","Aster"];
    private static readonly string[] MonkeyNames = ["Kiko","Mango","Tavi","Pip","Coco","Bongo"];

    public static ResidentState EnsureForIsland(GameSessionState session, IslandDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(session); ArgumentNullException.ThrowIfNull(definition);
        var existing = session.Residents.FirstOrDefault(r => r.OriginIslandId == definition.IslandId);
        if (existing is not null) return existing;
        var seed = new WorldSeed(session.Seed);
        var random = new DeterministicRandom(seed.Derive(definition.IslandId,"resident-v1"));
        ResidentKind kind = definition.IslandId == 0 ? ResidentKind.Bear : definition.Biome switch
        {
            BiomeType.Desert => ResidentKind.Cat,
            BiomeType.Snowy => ResidentKind.PolarBear,
            BiomeType.Jungle => ResidentKind.Monkey,
            BiomeType.Mountain => ResidentKind.Bear,
            _ => ResidentKind.Bear,
        };
        int residentId = StableResidentId(random,session);
        var world = new WorldQueries(definition,session.GetIslandState(definition.IslandId));
        int x = ChooseCell(world,random);
        int logicalLevel = definition.SurfaceLevels[x] + 1;
        string[] names = kind switch { ResidentKind.Cat=>CatNames, ResidentKind.PolarBear=>PolarNames, ResidentKind.Monkey=>MonkeyNames, _=>BearNames };
        string name = names[random.NextInt(0,names.Length)];
        var resident = new ResidentState(residentId,name,kind,definition.IslandId,definition.IslandId,x,logicalLevel,random.NextInt(0,4));
        if (kind == ResidentKind.Bear)
        {
            resident.Coins = random.NextInt(8,13);
            resident.MaxCoins = random.NextInt(Math.Max(10,resident.Coins),21);
            resident.OffersDiamondAxe = random.Chance(0.05);
            resident.OffersDiamondPickaxe = random.Chance(0.05);
        }
        session.AddResident(resident);
        return resident;
    }

    public static WorldPoint Feet(ResidentState resident, IslandDefinition definition) =>
        resident.Worker is not null && resident.Worker.Assigned && resident.Status==ResidentStatus.World
            ? WorkerService.Position(resident)
            : WorldGrid.Feet(resident.CellX,resident.LogicalLevel-1);

    public static WorldRect Rect(ResidentState resident, IslandDefinition definition)
    {
        var feet=Feet(resident,definition);
        double width=resident.Kind==ResidentKind.Cat?54:70;
        double height=resident.Kind==ResidentKind.Cat?64:resident.Kind==ResidentKind.Monkey?72:88;
        return new(feet.X-width/2,feet.Y-height,width,height);
    }
    public static WorldRect SpecialChestRect(ResidentState resident, IslandDefinition definition)
    {
        if(resident.Kind is not (ResidentKind.PolarBear or ResidentKind.Monkey)) return default;
        var feet=Feet(resident,definition); return new WorldRect(feet.X+26,feet.Y-48,48,48);
    }

    public static ResidentState? FindNearestWorldResident(GameSessionState session, IslandDefinition definition,
        WorldPoint actor, double range, WorldPoint? pointer=null)
    {
        ResidentState? best=null; double distance=double.PositiveInfinity;
        foreach(var resident in session.Residents.Where(r=>r.Status==ResidentStatus.World && r.IslandId==definition.IslandId))
        {
            var rect=Rect(resident,definition);
            if(pointer.HasValue && !rect.Contains(pointer.Value)) continue;
            double d=actor.DistanceSquared(rect.Center);
            if(d<=range*range && d<distance) { best=resident; distance=d; }
        }
        return best;
    }

    public static bool CanInteract(GameSessionState session, IslandDefinition definition, int currentIsland,
        WorldPoint actor, int residentId, out ResidentState? resident, out string reason)
    {
        resident=session.Residents.FirstOrDefault(r=>r.ResidentId==residentId);
        if(resident is null || resident.Status!=ResidentStatus.World || resident.IslandId!=currentIsland)
        { reason="That resident is no longer here."; return false; }
        if(actor.DistanceSquared(Rect(resident,definition).Center)>WorldActions.InteractionRange*WorldActions.InteractionRange)
        { reason="Move closer to the resident."; return false; }
        reason=string.Empty; return true;
    }

    public static bool TryMoveToRoster(GameSessionState session, IslandDefinition definition, int currentIsland,
        WorldPoint actor, int residentId, out string reason)
    {
        if(!CanInteract(session,definition,currentIsland,actor,residentId,out var resident,out reason) || resident is null) return false;
        if(!resident.Relocatable) { reason="This resident prefers to stay in their home biome."; return false; }
        if(!resident.Befriended) { reason="Reach 100 friendship before inviting this friend."; return false; }
        resident.Worker?.ClearTarget();
        if(resident.Worker is not null) resident.Worker.HomeChestPlacementId=0;
        resident.Status=ResidentStatus.Roster;
        reason=resident.Worker is not null && !resident.Worker.Cargo.IsEmpty
            ? $"{resident.Name} joined your friends roster with worker cargo preserved."
            : $"{resident.Name} joined your friends roster."; return true;
    }

    public static bool TryPlaceFromRoster(GameSessionState session, WorldQueries world, WorldPoint actor,
        int residentId, out string reason)
    {
        var resident=session.Residents.FirstOrDefault(r=>r.ResidentId==residentId);
        if(resident is null || resident.Status!=ResidentStatus.Roster || !resident.Relocatable)
        { reason="That friend is not available in the roster."; return false; }
        int actorX=Math.Clamp(WorldGrid.CellAt(actor).CellX,0,world.Definition.WidthCells-1);
        foreach(int x in Enumerable.Range(0,world.Definition.WidthCells).OrderBy(x=>Math.Abs(x-actorX)).ThenBy(x=>x))
        {
            if(Math.Abs(x-actorX)>8 || world.IsShore(x) || world.IsLadderColumn(x)) continue;
            int level=world.Definition.SurfaceLevels[x]+1;
            if(world.Definition.NaturalFeatures.Any(f=>f.CellX==x && world.NaturalPresent(f)) ||
                world.State.PlacedObjects.Any(p=>Math.Abs(p.CellX-x)<=1 && Math.Abs(p.LogicalLevel-level)<=1)) continue;
            var feet=WorldGrid.Feet(x,level-1);
            var center=new WorldPoint(feet.X,feet.Y-30);
            if(!world.InReach(actor,center,WorldActions.BuildRange) || !world.FreeForBear(center) || world.StructureIntersects(WorldGrid.BearRect(center))) continue;
            var candidateRect=WorldGrid.BearRect(center);
            bool occupied=session.Residents.Any(r=>r.ResidentId!=residentId && r.Status==ResidentStatus.World && r.IslandId==world.Definition.IslandId &&
                Rect(r,world.Definition).Intersects(candidateRect));
            if(occupied) continue;
            resident.Status=ResidentStatus.World; resident.IslandId=world.Definition.IslandId; resident.CellX=x; resident.LogicalLevel=level;
            WorkerService.OnPlacedFromRoster(resident);
            reason=$"{resident.Name} moved to island {world.Definition.IslandId}."; return true;
        }
        reason="No clear surface spot is available near the bear. The friend stayed safely in the roster."; return false;
    }

    private static int StableResidentId(DeterministicRandom random, GameSessionState session)
    {
        int candidate=(int)(random.NextUInt64()%2_000_000_000UL)+1;
        while(session.Residents.Any(r=>r.ResidentId==candidate)) candidate=candidate==2_000_000_000?1:candidate+1;
        return candidate;
    }

    private static int ChooseCell(WorldQueries world, DeterministicRandom random)
    {
        IslandDefinition d=world.Definition;
        var candidates=Enumerable.Range(4,Math.Max(1,d.WidthCells-8)).Where(x=>
        {
            if(x<WorldGrid.ShoreCells+2 || x>=d.WidthCells-WorldGrid.ShoreCells-2 || world.IsLadderColumn(x) ||
                d.NaturalFeatures.Any(f=>f.CellX==x && world.NaturalPresent(f)) ||
                d.GeneratedStructures.Any(s=>Math.Abs(s.CenterCellX-x)<=s.WidthCells/2+2)) return false;
            var feet=WorldGrid.Feet(x,d.SurfaceLevels[x]);
            var center=new WorldPoint(feet.X,feet.Y-30);
            return world.FreeForBear(center) && !world.StructureIntersects(WorldGrid.BearRect(center));
        }).ToArray();
        if(candidates.Length==0)
        {
            for(int x=2;x<d.WidthCells-2;x++)
            {
                var feet=WorldGrid.Feet(x,d.SurfaceLevels[x]); var center=new WorldPoint(feet.X,feet.Y-30);
                if(!world.IsShore(x) && !world.IsLadderColumn(x) && world.FreeForBear(center)) return x;
            }
            return Math.Clamp(d.SuggestedSpawnCell+6,2,d.WidthCells-3);
        }
        return candidates[random.NextInt(0,candidates.Length)];
    }
}
