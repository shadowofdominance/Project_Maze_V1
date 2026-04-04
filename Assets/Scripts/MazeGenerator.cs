using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Generates a maze with depth-first search and then renders the remaining walls.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    private Cell _currentCell;
    private readonly Stack<Cell> _backtrackStack = new Stack<Cell>();
    private Cell[,] _grid;

    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject horizontalWallPrefab;
    [SerializeField] private GameObject verticalWallPrefab;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    /// <summary>
    /// Builds the cell grid, selects the starting cell, and launches generation.
    /// </summary>
    private void Start()
    {
        GenerateGrid();
        _currentCell = _grid[0, 0];
        GenerateMaze();
    }

    /// <summary>
    /// Creates the 2D array of cells that defines the maze layout.
    /// </summary>
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

    /// <summary>
    /// Starts the coroutine that carves the maze paths.
    /// </summary>
    private void GenerateMaze()
    {
        StartCoroutine(Dfs());
    }

    /// <summary>
    /// Carves the maze using depth-first search with stack-based backtracking.
    /// </summary>
    private IEnumerator Dfs()
    {
        _currentCell.MarkVisited();

        while (true)
        {
            Cell next = GetUnvisitedNeighbour(_currentCell);

            if (next != null)
            {
                // Save the current path so the algorithm can step backward after a dead end.
                _backtrackStack.Push(_currentCell);

                RemoveWalls(_currentCell, next);
                _currentCell = next;

                // Mark immediately so this cell cannot be selected again by another branch.
                _currentCell.MarkVisited();
            }
            else if (_backtrackStack.Count > 0)
            {
                _currentCell = _backtrackStack.Pop();
            }
            else
            {
                break;
            }

            // The delay makes the generation sequence visible while the game is running.
            yield return new WaitForSeconds(0.02f);
        }

        CreateEntryAndExit();
        RenderMaze();
    }

    /// <summary>
    /// Opens the maze at the first and last cells to create a start and finish.
    /// </summary>
    private void CreateEntryAndExit()
    {
        if (_grid == null || width <= 0 || height <= 0)
        {
            return;
        }

        Cell entryCell = _grid[0, 0];
        Cell exitCell = _grid[width - 1, height - 1];

        // These two openings define the playable entrance and exit.
        entryCell.RemoveLeftWall();
        exitCell.RemoveRightWall();
    }

    /// <summary>
    /// Returns a random unvisited neighbour of the supplied cell, if one exists.
    /// </summary>
    private Cell GetUnvisitedNeighbour(Cell cell)
    {
        List<Cell> neighbours = new List<Cell>();

        int x = cell.X;
        int y = cell.Y;

        if (x > 0 && !_grid[x - 1, y].IsVisited)
            neighbours.Add(_grid[x - 1, y]);
        if (x < width - 1 && !_grid[x + 1, y].IsVisited)
            neighbours.Add(_grid[x + 1, y]);
        if (y > 0 && !_grid[x, y - 1].IsVisited)
            neighbours.Add(_grid[x, y - 1]);
        if (y < height - 1 && !_grid[x, y + 1].IsVisited)
            neighbours.Add(_grid[x, y + 1]);

        if (neighbours.Count > 0)
        {
            // Random choice is what keeps the maze different on each generation.
            return neighbours[Random.Range(0, neighbours.Count)];
        }

        return null;
    }

    /// <summary>
    /// Removes the shared wall between two adjacent cells.
    /// </summary>
    private void RemoveWalls(Cell a, Cell b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;

        if (dx == 1)
        {
            a.RemoveLeftWall();
            b.RemoveRightWall();
        }
        else if (dx == -1)
        {
            a.RemoveRightWall();
            b.RemoveLeftWall();
        }

        if (dy == 1)
        {
            a.RemoveBottomWall();
            b.RemoveTopWall();
        }
        else if (dy == -1)
        {
            a.RemoveTopWall();
            b.RemoveBottomWall();
        }
    }

    // Useful for debugging the generated layout without instantiating prefabs.
    /*private void OnDrawGizmos()
    {
        if (_grid == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = _grid[x, y];
                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);

                Gizmos.color = Color.white;
                if (cell.WallTop)
                    Gizmos.DrawLine(pos + new Vector3(0, cellSize, 0), pos + new Vector3(cellSize, cellSize, 0));
                if (cell.WallRight)
                    Gizmos.DrawLine(pos + new Vector3(cellSize, 0, 0), pos + new Vector3(cellSize, cellSize, 0));
                if (cell.WallBottom)
                    Gizmos.DrawLine(pos, pos + new Vector3(cellSize, 0, 0));
                if (cell.WallLeft)
                    Gizmos.DrawLine(pos, pos + new Vector3(0, cellSize, 0));
            }
        }
    }*/

    /// <summary>
    /// Spawns wall prefabs for every wall that remains after generation.
    /// </summary>
    private void RenderMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = _grid[x, y];
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);

                if (cell.WallTop)
                {
                    Instantiate(horizontalWallPrefab, position + new Vector3(cellSize / 2f, cellSize, 0), Quaternion.identity);
                }

                if (cell.WallRight)
                {
                    Instantiate(verticalWallPrefab, position + new Vector3(cellSize, cellSize / 2f, 0), Quaternion.identity);
                }

                if (cell.WallBottom)
                {
                    Instantiate(horizontalWallPrefab, position + new Vector3(cellSize / 2f, 0, 0), Quaternion.identity);
                }

                if (cell.WallLeft)
                {
                    Instantiate(verticalWallPrefab, position + new Vector3(0, cellSize / 2f, 0), Quaternion.identity);
                }
            }
        }
    }
}