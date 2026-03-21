using System;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;

    private Cell[,] grid;

    private void Start()
    {
        GenerateGrid();
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

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        Gizmos.color = Color.lawnGreen;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);
                Gizmos.DrawWireCube(pos, Vector3.one * cellSize);
            }
        }
    }
}