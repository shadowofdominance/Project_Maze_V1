/// <summary>
/// Stores the state of a single maze cell, including its grid position,
/// visit state, and which surrounding walls are still intact.
/// </summary>
public class Cell
{
    public int X { get; }
    public int Y { get; }
    public bool IsVisited { get; private set; }
    public bool WallTop { get; private set; } = true;
    public bool WallBottom { get; private set; } = true;
    public bool WallLeft { get; private set; } = true;
    public bool WallRight { get; private set; } = true;

    /// <summary>
    /// Creates a cell at the given grid coordinates.
    /// </summary>
    public Cell(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Marks this cell as visited so the DFS algorithm does not process it twice.
    /// </summary>
    public void MarkVisited()
    {
        IsVisited = true;
    }

    /// <summary>
    /// Removes the top wall of the cell.
    /// </summary>
    public void RemoveTopWall()
    {
        WallTop = false;
    }

    /// <summary>
    /// Removes the bottom wall of the cell.
    /// </summary>
    public void RemoveBottomWall()
    {
        WallBottom = false;
    }

    /// <summary>
    /// Removes the left wall of the cell.
    /// </summary>
    public void RemoveLeftWall()
    {
        WallLeft = false;
    }

    /// <summary>
    /// Removes the right wall of the cell.
    /// </summary>
    public void RemoveRightWall()
    {
        WallRight = false;
    }
}
