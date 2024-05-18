using System.Numerics;
using DungeonInternals;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello World");

        Map testMap = new(50, 50);
        Generator testGen = new(ref testMap, 20);
        testGen.Generate();

        for (var y = 0; y < testMap.Height; y++)
        {
            for (var x = 0; x < testMap.Width; x++)
            {
                Console.Write($"{(testMap[x,y] == 1 ? ' ' : '*')}");
            }
            Console.Write('\n');
        }
    }
}