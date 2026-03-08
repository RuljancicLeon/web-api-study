using Microsoft.Xna.Framework;
using GameClient.Entities;

namespace GameClient.AI;

/// <summary>
/// The enemy wanders in random directions at a relaxed pace.
/// Transitions to <see cref="ChaseState"/> when the player enters detection range.
/// </summary>
public class PatrolState : IEnemyState
{
    private static readonly Random _random = new();
    private double _moveTimer;

    private static readonly (int dRow, int dCol)[] Directions =
        [(-1, 0), (1, 0), (0, -1), (0, 1), (0, 0)];

    public IEnemyState Update(Enemy enemy, Player player, Grid.Grid grid, GameTime gameTime)
    {
        int distance = Math.Abs(enemy.Row - player.Row) + Math.Abs(enemy.Col - player.Col);
        if (distance <= enemy.DetectionRange)
            return new ChaseState();

        _moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_moveTimer >= enemy.PatrolSpeed)
        {
            _moveTimer = 0;
            var (dRow, dCol) = Directions[_random.Next(Directions.Length)];
            enemy.TryMove(dRow, dCol, grid);
        }

        return this;
    }
}
