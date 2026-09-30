namespace BearAdventure.Domain.World;

public readonly record struct WorldPoint(double X, double Y)
{
    public double DistanceSquared(WorldPoint other) => (X-other.X)*(X-other.X)+(Y-other.Y)*(Y-other.Y);
}
public readonly record struct WorldRect(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;
    public WorldPoint Center => new(X + Width / 2, Y + Height / 2);
    public bool Intersects(WorldRect b) => X < b.Right && Right > b.X && Y < b.Bottom && Bottom > b.Y;
    public bool Contains(WorldPoint p) => p.X >= X && p.X < Right && p.Y >= Y && p.Y < Bottom;
}
public static class WorldGrid
{
    public const double CellSize = 48;
    public const int WaterMarginCells = 7;
    public const int ShoreCells = 2;
    public const double LandLeft = WaterMarginCells * CellSize;
    public static UndergroundCell CellAt(WorldPoint point) => new(
        (int)Math.Floor((point.X - LandLeft) / CellSize), -(int)Math.Floor(point.Y / CellSize));
    public static WorldRect CellRect(UndergroundCell c) => new(LandLeft + c.CellX * CellSize, -c.LogicalLevel * CellSize, CellSize, CellSize);
    public static WorldPoint Center(int x, int level) => CellRect(new(x, level)).Center;
    public static WorldPoint Feet(int x, int surfaceLevel) => new(LandLeft+(x+0.5)*CellSize, -surfaceLevel*CellSize);
    public static WorldRect BearRect(WorldPoint center) => new(center.X-17, center.Y-29, 34, 58);
    public static WorldPoint BoatCenter(int width, BoatSide side) => new(
        side == BoatSide.Left ? LandLeft - 54 : LandLeft + width*CellSize + 54, 4);
    public static WorldRect BoatHull(int width, BoatSide side)
    {
        var c = BoatCenter(width,side); return new(c.X-48,c.Y+7,96,18);
    }
    public static bool IsInBounds(IslandDefinition d, UndergroundCell c) => c.CellX >= 0 && c.CellX < d.WidthCells &&
        c.LogicalLevel >= IslandGenerationSettings.DeepestLogicalLevel && c.LogicalLevel <= IslandGenerationSettings.HighestLogicalLevel;
}
