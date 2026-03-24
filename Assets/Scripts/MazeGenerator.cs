using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour
{
    private Cell currentCell;
    private Stack<Cell> stack = new Stack<Cell>();

    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;

    private Cell[,] grid;

    private void Start()
    {
        GenerateGrid();
        currentCell = grid[0, 0];
    }

    void GenerateGrid()
    {
        grid = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell(x, y);
            }
        }
    }

    void GenerateMaze()
    {
        StartCoroutine(DFS());
    }

    IEnumerator DFS()
    {
        currentCell.isVisited = true;

        while (true)
        {
            Cell next = GetUnvisitedNeighbour(currentCell);

            if (next != null)
            {
                stack.Push(currentCell);

                RemoveWalls(currentCell, next);

                currentCell = next;

                currentCell.isVisited = true;
            }
            else if (stack.Count == 0)
            {
                currentCell = stack.Pop();
            }
            else
            {
                break;
            }

            yield return new WaitForSeconds(0.02f);
        }
    }

    Cell GetUnvisitedNeighbour(Cell cell)
    {
        List<Cell> neighbours = new List<Cell>();

        int x = cell.x;
        int y = cell.y;

        if (x > 0 && !grid[x - 1, y].isVisited)
            neighbours.Add(grid[x - 1, y]);
        if (x < width - 1 && !grid[x + 1, y].isVisited)
            neighbours.Add(grid[x + 1, y]);
        if (y > 0 && !grid[x, y - 1].isVisited)
            neighbours.Add(grid[x, y - 1]);
        if (y < height - 1 && !grid[x, y + 1].isVisited)
            neighbours.Add(grid[x, y + 1]);

        if (neighbours.Count > 0)
        {
            return neighbours[Random.Range(0, neighbours.Count)];
        }

        return null;
    }

    void RemoveWalls(Cell a, Cell b)
    {
        int dx = a.x - b.x;
        int dy = a.y - b.y;

        if (dx == 1)
        {
            a.wallLeft = false;
            b.wallRight = false;
        }
        else if (dx == -1)
        {
            a.wallRight = false;
            b.wallLeft = false;
        }

        if (dy == 1)
        {
            a.wallBottom = false;
            b.wallTop = false;
        }
        else if (dy == -1)
        {
            a.wallTop = false;
            b.wallBottom = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        Gizmos.color = Color.lawnGreen;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);

                if (cell.wallTop)
                    Gizmos.DrawLine(pos, pos + new Vector3(cellSize, 0, 0));
            }
        }
    }
}