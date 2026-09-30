using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Simulation;

public enum HiveStatus { InsufficientFlowers, Producing, Ready, Full }
public sealed record HiveInfo(HiveStatus Status, int Flowers, int Output, double Progress);

/// <summary>Fixed half-second logical ticks, persisted remainder, stable ordering. No wall-clock/offline time.</summary>
public sealed class WorldSimulationService
{
    public const double TickSeconds = 0.5;
    public const double SaplingGrowthSeconds = 60;
    public const double HoneyCycleSeconds = 45;
    public const int HoneyCapacity = 3;
    public const int HoneyFlowerRadiusCells = 5;
    public const int HoneyFlowerVerticalCells = 4;
    public const int MinimumFlowersForHoney = 3;
    public const double BlockedRegrowthRetrySeconds = 5;

    public bool Advance(GameSessionState session, WorldSeed seed, IslandGenerator generator, double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
        if (seed.Value != session.Seed) throw new InvalidOperationException("Simulation seed does not match session.");
        double accumulated=session.SimulationRemainder+deltaSeconds;
        long ticks=(long)Math.Floor((accumulated+1e-9)/TickSeconds);
        if (ticks > 1_000_000) throw new ArgumentOutOfRangeException(nameof(deltaSeconds),"Advance in bounded intervals.");
        session.SimulationRemainder=Math.Max(0,accumulated-ticks*TickSeconds);
        if(ticks==0) return false;
        bool changed=false;
        var islands=session.DiscoveredIslandIds.OrderBy(id=>id).Select(id =>
            new WorldQueries(session.GetDefinition(id,generator),session.GetIslandState(id))).ToArray();
        var worldByIsland=islands.ToDictionary(w=>w.Definition.IslandId);
        foreach(int id in session.DiscoveredIslandIds.OrderBy(id=>id)) ResidentService.EnsureForIsland(session,session.GetDefinition(id,generator));
        for(long t=0;t<ticks;t++)
        {
            foreach(var resident in session.Residents.OrderBy(r=>r.ResidentId))
            {
                if(resident.Kind!=ResidentKind.Bear || resident.Status!=ResidentStatus.World || resident.Coins>=resident.MaxCoins) continue;
                resident.CoinRegenSeconds+=TickSeconds;
                if(resident.CoinRegenSeconds+1e-9>=20.0)
                {
                    int gain=(int)Math.Floor((resident.CoinRegenSeconds+1e-9)/20.0);
                    resident.CoinRegenSeconds-=gain*20.0;
                    resident.Coins=Math.Min(resident.MaxCoins,resident.Coins+gain);
                    changed=true;
                }
            }
            foreach(var world in islands)
            {
                var state=world.State;
                // Production uses flowers available at the start of this tick. Regrowth at its end
                // becomes available on the next tick, regardless of how Advance calls were grouped.
                foreach(var p in state.PlacedObjects.OrderBy(p=>p.PlacementId))
                {
                    if(p.Item==ItemType.Beehive)
                    {
                        if(p.StoredOutput>=HoneyCapacity) { p.ProductionSeconds=0; continue; }
                        if(CountFlowers(world,p)<MinimumFlowersForHoney) continue;
                        p.ProductionSeconds+=TickSeconds;
                        if(p.ProductionSeconds+1e-9>=HoneyCycleSeconds)
                        {
                            p.ProductionSeconds=Math.Max(0,p.ProductionSeconds-HoneyCycleSeconds);
                            p.StoredOutput++; if(p.StoredOutput>=HoneyCapacity) p.ProductionSeconds=0;
                            changed=true; state.Touch();
                        }
                    }
                    else if(p.Item==ItemType.Sapling && p.GrowthSeconds<SaplingGrowthSeconds && world.PlantCanGrow(p))
                    {
                        double old=p.GrowthSeconds;
                        p.GrowthSeconds=Math.Min(SaplingGrowthSeconds,p.GrowthSeconds+TickSeconds);
                        if((int)(old/5)!=(int)(p.GrowthSeconds/5)) { changed=true; state.Touch(); }
                    }
                }
                foreach(var (id,remaining) in state.NaturalRegrowthSeconds.OrderBy(p=>p.Key).ToArray())
                {
                    double next=remaining-TickSeconds;
                    if(next>1e-9) { state.SetRegrowthRemaining(id,next); continue; }
                    var feature=world.Definition.NaturalFeatures.FirstOrDefault(f=>f.FeatureId==id);
                    if(world.Definition.NaturalFeatures.Any(f=>f.FeatureId==id) && world.CanRegrow(feature))
                    { state.CompleteRegrowth(id); changed=true; }
                    else state.SetRegrowthRemaining(id,BlockedRegrowthRetrySeconds);
                }
            }
            if(WorkerSimulation.AdvanceTick(session,worldByIsland,TickSeconds)) changed=true;
            session.SimulationTick=checked(session.SimulationTick+1);
        }
        return changed;
    }
    public static int CountFlowers(WorldQueries world, PlacedObjectState hive)
    {
        bool Near(int x,int l) => Math.Abs(x-hive.CellX)<=HoneyFlowerRadiusCells && Math.Abs(l-hive.LogicalLevel)<=HoneyFlowerVerticalCells;
        int natural=world.Definition.NaturalFeatures.Count(f=>f.Kind==NaturalFeatureKind.Flower &&
            Near(f.CellX,f.SurfaceLevel+1) && world.NaturalPresent(f));
        return natural+world.State.PlacedObjects.Count(p=>PlacementRules.IsFlower(p.Item) && Near(p.CellX,p.LogicalLevel) && world.PlantedFlowerValid(p));
    }
    public static HiveInfo Inspect(WorldQueries world, PlacedObjectState hive)
    {
        int flowers=CountFlowers(world,hive);
        var status=hive.StoredOutput>=HoneyCapacity?HiveStatus.Full:flowers<MinimumFlowersForHoney?HiveStatus.InsufficientFlowers:
            hive.StoredOutput>0?HiveStatus.Ready:HiveStatus.Producing;
        return new(status,flowers,hive.StoredOutput,hive.ProductionSeconds/HoneyCycleSeconds);
    }
    public static string Describe(WorldQueries world, PlacedObjectState hive)
    {
        var i=Inspect(world,hive);
        string ready=$"Honey {i.Output}/{HoneyCapacity}";
        return i.Status switch {
            HiveStatus.Full => ready+" — full; collect to resume production.",
            HiveStatus.InsufficientFlowers => ready+$" — needs {MinimumFlowersForHoney-i.Flowers} more flower(s) within 5 cells horizontally / 4 vertically.",
            _ => ready+$" — producing {i.Progress:P0}; {i.Flowers} flowers in reach."
        };
    }
}
