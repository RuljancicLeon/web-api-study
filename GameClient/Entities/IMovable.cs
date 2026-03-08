namespace GameClient.Entities;

/// <summary>
/// Interface Segregation: focused contract for entities that occupy and move between grid cells.
/// </summary>
public interface IMovable
{
    int Row { get; }
    int Col { get; }
    bool TryMove(int dRow, int dCol, Grid.Grid grid);
}
