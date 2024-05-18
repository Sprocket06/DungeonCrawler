namespace DungeonInternals;

public struct Room
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public Room(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}