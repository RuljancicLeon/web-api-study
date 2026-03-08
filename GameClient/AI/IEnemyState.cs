using Microsoft.Xna.Framework;
using GameClient.Entities;

namespace GameClient.AI;

/// <summary>
/// State-pattern interface for enemy AI behaviour.
/// Dependency Inversion: <see cref="Enemy"/> depends on this abstraction, not concrete states.
/// Open/Closed: new behaviours are added by implementing this interface, not modifying existing code.
/// </summary>
public interface IEnemyState
{
    /// <summary>
    /// Executes one frame of AI logic and returns the next state
    /// (may return <c>this</c> to stay in the current state).
    /// </summary>
    IEnemyState Update(Enemy enemy, Player player, Grid.Grid grid, GameTime gameTime);
}
