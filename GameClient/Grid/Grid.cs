using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameClient.Grid;

/// <summary>
/// Manages a 2D grid of <see cref="GridCell"/> values and handles rendering.
/// </summary>
public class Grid
{
    private readonly GridCell[,] _cells;

    public int Rows { get; }
    public int Columns { get; }
    public int CellSize { get; }

    // Top-left pixel origin of the grid on screen.
    public Vector2 Origin { get; set; }

    public Grid(int rows, int columns, int cellSize, Vector2 origin)
    {
        Rows = rows;
        Columns = columns;
        CellSize = cellSize;
        Origin = origin;
        _cells = new GridCell[rows, columns];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                _cells[r, c] = new GridCell();
    }

    // ── Data access ────────────────────────────────────────────────────────────

    public GridCell GetCell(int row, int col) => _cells[row, col];

    public bool IsAlive(int row, int col) => _cells[row, col].IsAlive;

    public void ToggleCell(int row, int col)
    {
        if (!InBounds(row, col)) return;
        _cells[row, col].Toggle();
    }

    public void SetCell(int row, int col, bool alive)
    {
        if (!InBounds(row, col)) return;
        _cells[row, col].SetAlive(alive);
    }

    public void Reset()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Columns; c++)
                _cells[r, c] = new GridCell();
    }

    // ── Coordinate helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Converts a screen pixel position to grid (row, col) indices.
    /// Returns false if the point is outside the grid bounds.
    /// </summary>
    public bool TryGetCellAt(Vector2 screenPos, out int row, out int col)
    {
        Vector2 local = screenPos - Origin;
        col = (int)(local.X / CellSize);
        row = (int)(local.Y / CellSize);
        return InBounds(row, col);
    }

    public bool InBounds(int row, int col) =>
        row >= 0 && row < Rows && col >= 0 && col < Columns;

    /// <summary>Returns the screen-space rectangle for a given cell.</summary>
    public Rectangle GetCellRect(int row, int col) =>
        new Rectangle(
            (int)Origin.X + col * CellSize,
            (int)Origin.Y + row * CellSize,
            CellSize,
            CellSize);

    // ── Rendering ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws the grid using a pre-loaded 1×1 white pixel texture.
    /// Alive cells are white; dead cells are drawn with a dark background and
    /// a lighter grid line.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                Rectangle rect = GetCellRect(r, c);
                Color fill = _cells[r, c].IsAlive ? new Color(80, 200, 120) : new Color(30, 30, 30);

                // Fill
                spriteBatch.Draw(pixel, rect, fill);

                // Grid lines (1-pixel border inside each cell)
                DrawBorder(spriteBatch, pixel, rect, new Color(60, 60, 60), 1);
            }
        }
    }

    private static void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel,
                                   Rectangle rect, Color color, int thickness)
    {
        // Top
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        // Left
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }
}
