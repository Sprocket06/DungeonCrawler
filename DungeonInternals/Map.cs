using System.Numerics;

namespace DungeonInternals;

public class Map
{
    private int[,] _tiles;

    public int Width;
    public int Height;
    public int this[int x, int y]
    {
        get => _tiles[x, y];
        set => _tiles[x, y] = value;
    }

    public int this[Vector2 point]
    {
        get => _tiles[(int)point.X, (int)point.Y];
        set => _tiles[(int)point.X, (int)point.Y] = value;
    }

    public Map(int width, int height)
    {
        _tiles = new int[width, height];
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                _tiles[x, y] = 0;
            }
        }
        Width = width;
        Height = height;
    }
}