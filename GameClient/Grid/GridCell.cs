using Microsoft.Xna.Framework;

namespace GameClient.Grid;

/// <summary>
/// Represents a single cell in the 2D grid.
/// </summary>
public struct GridCell
{
    public bool IsAlive { get; set; }
    public Color Color { get; set; }

    public GridCell(bool isAlive = false)
    {
        IsAlive = isAlive;
        Color = isAlive ? Color.White : Color.Black;
    }

    public void Toggle()
    {
        IsAlive = !IsAlive;
        Color = IsAlive ? Color.White : Color.Black;
    }

    public void SetAlive(bool alive)
    {
        IsAlive = alive;
        Color = IsAlive ? Color.White : Color.Black;
    }
}
