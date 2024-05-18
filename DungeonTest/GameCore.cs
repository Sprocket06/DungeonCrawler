using Chroma;
using Chroma.Graphics;
using Chroma.Input;
using DungeonInternals;

namespace DungeonTest;

public class GameCore : Game
{
    private Map TestLevel = new(25, 25);
    private Generator levelGenerator;
    private bool showWalls = false;
    
    public GameCore() : base(new GameStartupOptions(false, false))
    {
        Generator levelGen = new(ref TestLevel, 100);
        levelGen.Generate();
    }

    protected override void Draw(RenderContext context)
    {
        for (var x = 0; x < TestLevel.Width; x++)
        {
            for (var y = 0; y < TestLevel.Height; y++)
            {
                var cellColor = TestLevel[x, y] switch
                {
                    1 => Color.Black,
                    2 => showWalls ? Color.Blue : Color.White,
                    3 => Color.Black,
                    4 => Color.Red,
                    0 => Color.White
                };
                context.Rectangle(ShapeMode.Fill, x*10, y*10, 10, 10, cellColor);
            }
        }
    }

    protected override void KeyPressed(KeyEventArgs e)
    {
        if (e.KeyCode is KeyCode.Escape)
        {
            Quit();
        }

        if (e.KeyCode is KeyCode.Alpha1)
        {
            showWalls = !showWalls;
        }
    }
}