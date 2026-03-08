using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameClient.Entities;

/// <summary>
/// Abstract base for all entities that live on the grid.
/// Provides shared position, movement, health, and rendering (DRY).
/// Open/Closed: extend via inheritance, override Update and Draw without modifying this class.
/// </summary>
public abstract class GameEntity : IMovable, IDamageable
{
    public int Row { get; protected set; }
    public int Col { get; protected set; }
    public Color Color { get; protected set; }
    public Health Health { get; }
    public bool IsDead => Health.IsDead;

    protected GameEntity(int row, int col, Color color, int maxHealth)
    {
        Row = row;
        Col = col;
        Color = color;
        Health = new Health(maxHealth);
    }

    public bool TryMove(int dRow, int dCol, Grid.Grid grid)
    {
        int newRow = Row + dRow;
        int newCol = Col + dCol;

        if (!grid.IsWalkable(newRow, newCol))
            return false;

        grid.Vacate(Row, Col);
        Row = newRow;
        Col = newCol;
        grid.Occupy(Row, Col);
        return true;
    }

    public virtual void TakeDamage(int amount) => Health.TakeDamage(amount);

    /// <summary>Per-frame logic. Subclasses define their own behaviour (Template Method).</summary>
    public abstract void Update(GameTime gameTime, Grid.Grid grid);

    /// <summary>Draws the entity as a padded rectangle inside its grid cell.</summary>
    public virtual void Draw(SpriteBatch spriteBatch, Texture2D pixel, Grid.Grid grid)
    {
        Rectangle cellRect = grid.GetCellRect(Row, Col);

        int padding = 4;
        Rectangle entityRect = new(
            cellRect.X + padding,
            cellRect.Y + padding,
            cellRect.Width - padding * 2,
            cellRect.Height - padding * 2);

        spriteBatch.Draw(pixel, entityRect, Color);
    }
}
