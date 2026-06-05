# MazeGenerator.cs Breakdown

`MazeGenerator.cs` creates a random maze in Unity, then spawns wall prefabs to visually build it in the scene.

It uses a common maze algorithm called **depth-first search with backtracking**.

## Main Idea

The maze is made from a 2D grid of `Cell` objects:

```csharp
private Cell[,] _grid;
```

Each `Cell` starts with all four walls:

```csharp
WallTop = true;
WallBottom = true;
WallLeft = true;
WallRight = true;
```

The generator walks through the grid, randomly choosing unvisited neighboring cells. Whenever it moves from one cell to another, it removes the wall between them. This creates connected maze paths.

## What Happens When The Game Starts

```csharp
private void Start()
{
    GenerateNewMaze();
}
```

As soon as the scene starts, it calls `GenerateNewMaze()`.

That function:

1. Checks that wall prefabs are assigned.
2. Stops any old maze generation coroutine.
3. Clears old wall GameObjects.
4. Creates a fresh grid of cells.
5. Starts at cell `(0, 0)`.
6. Runs the maze generation algorithm.

## Grid Creation

```csharp
private void GenerateGrid()
{
    _grid = new Cell[width, height];

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            _grid[x, y] = new Cell(x, y);
        }
    }
}
```

This creates a maze of size `width x height`.

For example, if `width = 10` and `height = 10`, it creates 100 cells.

## The Maze Algorithm

The main algorithm is here:

```csharp
private IEnumerator Dfs()
```

It works like this:

1. Mark the current cell as visited.
2. Look for a random neighboring cell that has not been visited.
3. If one exists:
   - Save the current cell onto `_backtrackStack`.
   - Remove the wall between current cell and next cell.
   - Move to the next cell.
   - Mark it visited.
4. If no unvisited neighbor exists:
   - Go backward using the stack.
5. Repeat until there is nowhere left to go.

This part chooses a random unvisited neighbor:

```csharp
Cell next = GetUnvisitedNeighbour(_currentCell);
```

This part removes walls between two connected cells:

```csharp
RemoveWalls(_currentCell, next);
```

The stack is what lets the generator recover from dead ends:

```csharp
private readonly Stack<Cell> _backtrackStack = new Stack<Cell>();
```

So the maze keeps exploring until every cell has been visited.

## Why It Uses A Coroutine

```csharp
yield return new WaitForSeconds(generationDelay);
```

Because `Dfs()` is a coroutine, the maze can generate over time instead of instantly. The `generationDelay` controls how slow or visible the generation process is.

If `generationDelay = 0.02f`, you can see the maze being generated step-by-step.

There is also an instant version:

```csharp
private void GenerateMazeImmediate()
```

That is used when not in Play Mode, probably for editor tools.

## Entry And Exit

After the maze is generated:

```csharp
CreateEntryAndExit();
```

This removes:

```csharp
entryCell.RemoveLeftWall();
exitCell.RemoveRightWall();
```

So the start cell `(0, 0)` has an opening on the left, and the final cell `(width - 1, height - 1)` has an opening on the right.

## Rendering The Maze

The algorithm itself only changes data inside the `Cell` objects. It does not immediately create visible walls.

The visible walls are created here:

```csharp
private void RenderMaze()
```

For every cell, it checks which walls still exist:

```csharp
if (cell.WallTop)
if (cell.WallRight)
if (cell.WallBottom)
if (cell.WallLeft)
```

Then it spawns either:

```csharp
horizontalWallPrefab
verticalWallPrefab
```

at the correct position.

So the maze is first generated logically, then converted into actual Unity GameObjects.

## Wall Container

All spawned wall objects are placed under a child object named:

```csharp
MazeWalls
```

That is handled by:

```csharp
GetOrCreateWallContainer()
```

This keeps the hierarchy cleaner because all generated wall objects are grouped under one parent.

## Public Controls

The script also exposes helper methods:

```csharp
SetMazeSize(int newWidth, int newHeight)
```

Changes the maze size and regenerates it.

```csharp
IncreaseMazeSize(int amount)
```

Adds to both width and height.

```csharp
ResetMaze(int defaultWidth, int defaultHeight)
```

Restores the maze size.

```csharp
SetGenerationDelay(float newDelay)
```

Changes how fast the maze generation animation runs.

These are likely meant to be called by UI buttons or another script, maybe `MazeTool.cs`.

## Short Version

`MazeGenerator` builds a grid of cells, carves random paths using depth-first search, opens an entrance and exit, then spawns wall prefabs wherever walls still remain.
