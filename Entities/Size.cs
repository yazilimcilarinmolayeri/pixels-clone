namespace YmyPixels.Entities;

public class Size
{
    public int X { get; set; }
    public int Y { get; set; }

    public Size()
    {
        this.X = 0;
        this.Y = 0;
    }
    
    public Size(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}