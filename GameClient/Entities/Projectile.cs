using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameClient.Entities;

/// <summary>
/// A grid-based projectile that travels in a cardinal direction until it
/// hits a wall, leaves the grid, or strikes an enemy.
/// Does not occupy cells or block movement.
/// </summary>
public class Projectile
{
    public int Row { get; private set; }
    public int Col { get; private set; }
    public int DRow { get; }
    public int DCol { get; }
    public int Damage { get; }
    public bool IsActive { get; private set; } = true;

    private double _moveTimer;
    private int _remainingRange;
    private const double MoveInterval = 0.06;
    private const int MaxRange = 10;

    public Projectile(int row, int col, int dRow, int dCol, int damage)
    {
        Row = row;
        Col = col;
        DRow = dRow;
        DCol = dCol;
        Damage = damage;
        _remainingRange = MaxRange;
    }

    public void Update(GameTime gameTime, Grid.Grid grid)
    {
        if (!IsActive) return;

        _moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_moveTimer < MoveInterval) return;
        _moveTimer -= MoveInterval;

        int newRow = Row + DRow;
        int newCol = Col + DCol;

        if (!grid.InBounds(newRow, newCol) || grid.IsAlive(newRow, newCol))
        {
            IsActive = false;
            return;
        }

        Row = newRow;
        Col = newCol;
        _remainingRange--;

        if (_remainingRange <= 0)
            IsActive = false;
    }

    public void Deactivate() => IsActive = false;

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, Grid.Grid grid)
    {
        if (!IsActive) return;

        Rectangle cellRect = grid.GetCellRect(Row, Col);

        int size = cellRect.Width / 4;
        int offset = (cellRect.Width - size) / 2;
        Rectangle projectileRect = new(
            cellRect.X + offset,
            cellRect.Y + offset,
            size,
            size);

        spriteBatch.Draw(pixel, projectileRect, new Color(255, 220, 80));
    }
}
