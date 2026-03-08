namespace GameClient.Entities;

/// <summary>
/// Interface Segregation: focused contract for entities that have health and can receive damage.
/// </summary>
public interface IDamageable
{
    Health Health { get; }
    bool IsDead { get; }
    void TakeDamage(int amount);
}
