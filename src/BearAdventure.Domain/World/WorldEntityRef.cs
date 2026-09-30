namespace BearAdventure.Domain.World;

public enum WorldEntityKind { Placed, GeneratedChest }
public readonly record struct WorldEntityRef(int IslandId, WorldEntityKind Kind, int EntityId);
public sealed record PlayerLocation(int IslandId, double X, double Y);
