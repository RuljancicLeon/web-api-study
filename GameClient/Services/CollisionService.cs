using GameClient.Entities;

namespace GameClient.Services;

/// <summary>
/// Single Responsibility: handles all collision detection and damage resolution between entities.
/// </summary>
public static class CollisionService
{
    /// <summary>
    /// Applies contact damage to the player from any living enemy within one cell (Manhattan distance ≤ 1).
    /// </summary>
    public static void HandlePlayerEnemyCollisions(Player player, IReadOnlyList<Enemy> enemies)
    {
        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;
            int distance = Math.Abs(enemy.Row - player.Row) + Math.Abs(enemy.Col - player.Col);
            if (distance <= 1)
                player.TakeDamage(enemy.AttackDamage);
        }
    }

    /// <summary>
    /// Checks each active projectile against living enemies.
    /// On hit: damages the enemy and deactivates the projectile.
    /// </summary>
    public static void HandleProjectileCollisions(List<Projectile> projectiles, IReadOnlyList<Enemy> enemies)
    {
        foreach (var projectile in projectiles)
        {
            if (!projectile.IsActive) continue;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead) continue;
                if (enemy.Row == projectile.Row && enemy.Col == projectile.Col)
                {
                    enemy.TakeDamage(projectile.Damage);
                    projectile.Deactivate();
                    break;
                }
            }
        }
    }
}
