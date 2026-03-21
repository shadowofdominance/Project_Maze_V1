public class Cell
{
    public int x, y;
    public bool isVisited;
    public bool wallTop = true;
    public bool wallBottom = true;
    public bool wallLeft = true;
    public bool wallRight = true;
    
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        isVisited = false;
    }
}