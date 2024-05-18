using System.Numerics;
using System.Runtime.InteropServices;

namespace DungeonInternals;

public class Generator
{
   private Map Map;
   private List<Vector2> walls;
   private int numAttempts = 0;
   private List<Room> rooms = new();
   private List<List<Vector2>> paths;
   private bool genDone = false;
   
   public Generator(ref Map map, int numRoomAttempts)
   {
       Map = map;
       walls = new();
       numAttempts = numRoomAttempts;
   }
   
   public void Generate()
   {
      if (genDone)
      {
         //reset map;
         for (var x = 0; x < Map.Width; x++)
         {
            for (var y = 0; y < Map.Height; y++)
            {
               Map[x, y] = 0;
            }
         }

         genDone = false;
      }
      
      // First, we place a series of rooms randomly in the map, making sure they do not overlap 
      Random rng = new();
      for (var i = 0; i < numAttempts; i++)
      {
         int roomWidth = rng.Next(3, 6);
         int roomHeight = rng.Next(3, 6);
         int roomX = rng.Next(1, Map.Width);
         int roomY = rng.Next(1, Map.Height);
         var success = TryPlaceRoom(roomX, roomY, roomWidth, roomHeight);
         if (success)
         {
            Room newRoom = new(roomX, roomY, roomWidth, roomHeight);
            rooms.Add(newRoom);
         }
      }
      
      // next, we need to generate our passageways.
      /*
       * 1. pick random point not in (walls, rooms, paths)
       * 2. run DFS/flood fill algorithm to generate a path.
       * 3. check if there exists more points not in (walls, rooms, paths)
       */
      while (CheckValidPathsLeft())
      {
         int rX = rng.Next(1, Map.Width-1);
         int rY = rng.Next(1, Map.Height-1);
         if(Map[rX, rY] != 0)continue;
         
         // dfs time
         var path = BuildPath(rX, rY);
         foreach (var node in path)
         {
            Map[node] = 3;
            var cardinals = GetCardinals(node);
            foreach (var neighbor in cardinals)
            {
               if (Map[neighbor] == 0) Map[neighbor] = 2;
            }
         }

         //break;
      }
      
      // now, we do doors
      List<Vector2> potentialDoorsVertical = new();
      var state = 0;
      for (var x = 0; x < Map.Width; x++)
      {
         for (var y = 0; y < Map.Height; y++)
         {
            var current = Map[x, y];
            switch (state)
            {
               case 0:
                  if (current == 3 || current == 1) state++;
                  break;
               case 1:
                  if (current == 2) state++;
                  else state = 0;
                  break;
               case 2:
                  if (current == 3 || current == 1)
                  {
                     potentialDoorsVertical.Add(new Vector2(x, y-1));
                     state = 1;
                  }
                  else state = 0; 
                  break;
            }
         }
      }

      potentialDoorsVertical.Sort(((pointA, pointB) => (int)pointA.X - (int)pointB.X));
      List<List<Vector2>> doorGroups = new();
      if (potentialDoorsVertical.Count > 0)
      {
         int currentGroup = 0;
         doorGroups.Add(new() { potentialDoorsVertical[0] });
         for (var i = 1; i < potentialDoorsVertical.Count; i++)
         {
            if (potentialDoorsVertical[i].X - potentialDoorsVertical[i - 1].X <= 1)
            {
               doorGroups[currentGroup].Add(potentialDoorsVertical[i]);
            }
            else
            {
               currentGroup++;
               doorGroups.Add(new() { potentialDoorsVertical[i] });
            }
         }   
      }

      foreach (var doorGroup in doorGroups)
      {
         var door = doorGroup[rng.Next(doorGroup.Count)];
         Map[door] = 4;
      }
      
      genDone = true;
   }

   private List<Vector2> GetCardinals(Vector2 point)
   {
      var ret = new List<Vector2>() { (point with {X = point.X+1}), point with {X = point.X-1}, point with {Y = point.Y-1}, point with {Y = point.Y+1} };
      ret.RemoveAll(n => !BoundsCheck(n));
      return ret;
   }
   
   private List<Vector2> GetSurrounding(Vector2 point)
   {
      var ret = new List<Vector2>()
      {
         point with { X = point.X + 1 }, point with { X = point.X - 1 }, point with { Y = point.Y - 1 },
         point with { Y = point.Y + 1 }, new(point.X+1, point.Y+1), new(point.X+1, point.Y-1),
         new(point.X-1, point.Y+1), new(point.X-1, point.Y-1)
      };
      ret.RemoveAll(n => !BoundsCheck(n));
      return ret;
   }
   
   private bool BoundsCheck(Vector2 point)
   {
      if (point.X < 0 || point.X >= Map.Width) return false;
      if (point.Y < 0 || point.Y >= Map.Height) return false;
      return true;
   }

   private bool CheckValidPathsLeft()
   {
      for (var x = 1; x < Map.Width-1; x++)
      {
         for (var y = 1; y < Map.Height-1; y++)
         {
            if (Map[x, y] == 0)
            {
               var surrounds = GetSurrounding(new(x, y));
               var check = true;
               foreach (var point in surrounds)
               {
                  if (Map[point] == 1) check = false;
               }

               if (check) return true;
            }
         }
      }

      return false;
   }
   
   private List<Vector2> BuildPath(int startX, int startY)
   {
      List<Vector2> path = new(); // all visited nodes.
      List<Vector2> finished = new(); // all fully explored nodes.
      Random rng = new();
      path.Add(new(startX, startY));
      var current = 0;
      while (true)
      {
         var pos = path[current];
         List<Vector2> nextNodes = new();
         Vector2 west = pos with { X = pos.X + 1 };
         if (pos.X + 1 < Map.Width-1 && Map[(int)west.X, (int)west.Y] == 0 &&
             !path.Contains(west)) nextNodes.Add(west);
         Vector2 east = pos with { X = pos.X - 1 };
         if (pos.X - 1 > 0 && Map[(int)east.X, (int)east.Y] == 0 && !path.Contains(east))
            nextNodes.Add(east);
         Vector2 north = pos with { Y = pos.Y - 1 };
         if (pos.Y - 1 > 0 && Map[(int)north.X, (int)north.Y] == 0 && !path.Contains(north))
            nextNodes.Add(north);
         Vector2 south = pos with { Y = pos.Y + 1 };
         if (pos.Y + 1 < Map.Height-1 && Map[(int)south.X, (int)south.Y] == 0 &&
             !path.Contains(south)) nextNodes.Add(south);
         // validation p.2
         // ensure any new nodes would not be adjacent to two other nodes.
         nextNodes.RemoveAll(node =>
         {
            var adjacentNodes = 0;
            if (path.Contains(node with { Y = node.Y + 1 })) adjacentNodes++;
            if (path.Contains(node with { Y = node.Y - 1 })) adjacentNodes++;
            if (path.Contains(node with { X = node.X + 1 })) adjacentNodes++;
            if (path.Contains(node with { X = node.X - 1 })) adjacentNodes++;
            if (adjacentNodes > 1) return true;
            return false;
         });

         if (nextNodes.Count != 0)
         {
            var nextNode = nextNodes[rng.Next(nextNodes.Count)];
            path.Add(nextNode);
            current++;
         }
         else
         {
            finished.Add(path[current]);
            if (current == 0) break; 
            while (finished.Contains(path[current]) && current > 0)
            {
               current--;
            }
         }   
      }
      
      return path;
   }

   private bool TryPlaceRoom(int x, int y, int width, int height)
   {
      //validate room will not go oob
      if (Map.Width <= x + width || Map.Height <= y + height) return false;
      
      // validate room does not overlap
      for (var cX = x; cX < (x + width); cX++)
      {
         for (var cY = y; cY < (y + height); cY++)
         {
            Vector2 currentNode = new(cX, cY);
            if (Map[cX, cY] != 0 || walls.Contains(currentNode))
            {
               // invalid placement. return false.
               return false;
            }
         }
      }
      
      // after passing validation, we're good to go.
      for (var cX = x; cX < (x + width); cX++)
      {
         for (var cY = y; cY < (y + height); cY++)
         {
            Map[cX, cY] = 1;
         }
      }
      
      // now we need to mark adjacent nodes as walls.
      //edges are:
      //(x-1, y-1) -> (x+width, y-1)
      for (var cX = x - 1; cX <= (x + width); cX++)
      {
         Vector2 currentNode = new(cX, y - 1);
         Map[currentNode] = 2; 
      }
      //(x-1, y-1) -> (x-1, y + height)
      //we skip the first point here as it overlaps
      for (var cY = y; cY <= (y + height); cY++)
      {
         Vector2 currentNode = new(x - 1, cY);
         Map[currentNode] = 2; 
      }
      //(x-1, y+height) -> (x+width, y+height)
      //again, first point overlaps
      for (var cX = x; cX <= (x + width); cX++)
      {
         Vector2 currentNode = new(cX, y + height);
         Map[currentNode] = 2; 
      }
      //(x+width, y-1) -> (x+width, y+height) 
      //we skip first *and* last point here because they both overlap.
      for (var cY = y; cY < (y + height); cY++)
      {
         Vector2 currentNode = new(x + width, cY);
         Map[currentNode] = 2; 
      }
      
      return true;
   }
}