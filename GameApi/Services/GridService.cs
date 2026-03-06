using GameApi.Models;

namespace GameApi.Services;

/// <summary>
/// Holds the in-memory grid state shared across API requests.
/// In a real game this could be persisted to a database.
/// </summary>
public class GridService
{
    private const int DefaultRows = 20;
    private const int DefaultColumns = 20;

    private bool[][] _cells;

    public int Rows { get; private set; }
    public int Columns { get; private set; }

    public GridService()
    {
        Rows = DefaultRows;
        Columns = DefaultColumns;
        _cells = CreateEmpty(Rows, Columns);
    }

    public GridState GetState()
    {
        return new GridState
        {
            Rows = Rows,
            Columns = Columns,
            Cells = _cells.Select(row => row.ToArray()).ToArray()
        };
    }

    public bool SetCell(int row, int col, bool value)
    {
        if (!IsInBounds(row, col))
            return false;

        _cells[row][col] = value;
        return true;
    }

    public bool ToggleCell(int row, int col)
    {
        if (!IsInBounds(row, col))
            return false;

        _cells[row][col] = !_cells[row][col];
        return true;
    }

    public void Reset()
    {
        _cells = CreateEmpty(Rows, Columns);
    }

    private bool IsInBounds(int row, int col) =>
        row >= 0 && row < Rows && col >= 0 && col < Columns;

    private static bool[][] CreateEmpty(int rows, int cols) =>
        Enumerable.Range(0, rows)
                  .Select(_ => new bool[cols])
                  .ToArray();
}
