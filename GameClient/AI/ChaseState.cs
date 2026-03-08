using Microsoft.Xna.Framework;
using GameClient.Entities;

namespace GameClient.AI;

/// <summary>
/// The enemy actively pursues the player. Once aggro'd, it never disengages.
/// </summary>
public class ChaseState : IEnemyState
{
    private double _moveTimer;

    public IEnemyState Update(Enemy enemy, Player player, Grid.Grid grid, GameTime gameTime)
    {
        _moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_moveTimer >= enemy.ChaseSpeed)
        {
            _moveTimer = 0;
            MoveTowardPlayer(enemy, player, grid);
        }

        return this;
    }

    private static void MoveTowardPlayer(Enemy enemy, Player player, Grid.Grid grid)
    {
        int dRow = Math.Sign(player.Row - enemy.Row);
        int dCol = Math.Sign(player.Col - enemy.Col);

        // Move along the axis with the greater distance; fall back to the other axis if blocked.
        if (Math.Abs(player.Row - enemy.Row) >= Math.Abs(player.Col - enemy.Col))
        {
            if (dRow != 0 && !enemy.TryMove(dRow, 0, grid))
                if (dCol != 0) enemy.TryMove(0, dCol, grid);
        }
        else
        {
            if (dCol != 0 && !enemy.TryMove(0, dCol, grid))
                if (dRow != 0) enemy.TryMove(dRow, 0, grid);
        }
    }
}
