using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Generates a maze with depth-first search and then renders the remaining walls.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    private const string WallContainerName = "MazeWalls";

    private Cell _currentCell;
    private readonly Stack<Cell> _backtrackStack = new Stack<Cell>();
    private Cell[,] _grid;
    private readonly List<GameObject> _spawnedWalls = new List<GameObject>();
    private Coroutine _generationRoutine;
    private Transform _wallContainer;

    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float generationDelay = 0.02f;
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
        GenerateNewMaze();
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
        if (Application.isPlaying)
        {
            _generationRoutine = StartCoroutine(Dfs());
            return;
        }

        GenerateMazeImmediate();
    }

    /// <summary>
    /// Rebuilds the maze using the current generator settings.
    /// </summary>
    public void GenerateNewMaze()
    {
        if (!HasValidSetup())
        {
            return;
        }

        if (_generationRoutine != null)
        {
            StopCoroutine(_generationRoutine);
            _generationRoutine = null;
        }

        _backtrackStack.Clear();
        ClearRenderedMaze();
        GenerateGrid();
        _currentCell = _grid[0, 0];
        GenerateMaze();
    }

    /// <summary>
    /// Sets the maze dimensions and rebuilds it.
    /// </summary>
    public void SetMazeSize(int newWidth, int newHeight)
    {
        width = Mathf.Max(1, newWidth);
        height = Mathf.Max(1, newHeight);
        GenerateNewMaze();
    }

    /// <summary>
    /// Adds the same amount to both width and height, then rebuilds the maze.
    /// </summary>
    public void IncreaseMazeSize(int amount)
    {
        int delta = Mathf.Max(1, amount);
        SetMazeSize(width + delta, height + delta);
    }

    /// <summary>
    /// Restores the maze to the provided default size and rebuilds it.
    /// </summary>
    public void ResetMaze(int defaultWidth, int defaultHeight)
    {
        SetMazeSize(defaultWidth, defaultHeight);
    }

    /// <summary>
    /// Updates the delay used during play mode generation.
    /// </summary>
    public void SetGenerationDelay(float newDelay)
    {
        generationDelay = Mathf.Max(0f, newDelay);
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
            if (generationDelay > 0f)
            {
                yield return new WaitForSeconds(generationDelay);
            }
            else
            {
                yield return null;
            }
        }

        CreateEntryAndExit();
        RenderMaze();
        _generationRoutine = null;
    }

    /// <summary>
    /// Generates the maze instantly so it can be used in editor tools.
    /// </summary>
    private void GenerateMazeImmediate()
    {
        _currentCell.MarkVisited();

        while (true)
        {
            Cell next = GetUnvisitedNeighbour(_currentCell);

            if (next != null)
            {
                _backtrackStack.Push(_currentCell);
                RemoveWalls(_currentCell, next);
                _currentCell = next;
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
                    SpawnWall(horizontalWallPrefab, position + new Vector3(cellSize / 2f, cellSize, 0));
                }

                if (cell.WallRight)
                {
                    SpawnWall(verticalWallPrefab, position + new Vector3(cellSize, cellSize / 2f, 0));
                }

                if (cell.WallBottom)
                {
                    SpawnWall(horizontalWallPrefab, position + new Vector3(cellSize / 2f, 0, 0));
                }

                if (cell.WallLeft)
                {
                    SpawnWall(verticalWallPrefab, position + new Vector3(0, cellSize / 2f, 0));
                }
            }
        }
    }

    private bool HasValidSetup()
    {
        return horizontalWallPrefab != null && verticalWallPrefab != null;
    }

    private void SpawnWall(GameObject wallPrefab, Vector3 position)
    {
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, GetOrCreateWallContainer());
        _spawnedWalls.Add(wall);
    }

    private void ClearRenderedMaze()
    {
        for (int i = 0; i < _spawnedWalls.Count; i++)
        {
            GameObject wall = _spawnedWalls[i];
            if (wall == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(wall);
            }
            else
            {
#if UNITY_EDITOR
                DestroyImmediate(wall);
#endif
            }
        }

        _spawnedWalls.Clear();

        Transform wallContainer = GetOrCreateWallContainer();
        for (int i = wallContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = wallContainer.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
#if UNITY_EDITOR
                DestroyImmediate(child);
#endif
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private Transform GetOrCreateWallContainer()
    {
        if (_wallContainer != null)
        {
            return _wallContainer;
        }

        Transform existingContainer = transform.Find(WallContainerName);
        if (existingContainer != null)
        {
            _wallContainer = existingContainer;
            return _wallContainer;
        }

        GameObject container = new GameObject(WallContainerName);
        container.transform.SetParent(transform, false);
        _wallContainer = container.transform;
        return _wallContainer;
    }
}
