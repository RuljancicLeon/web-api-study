namespace GameClient.Entities;

/// <summary>
/// Encapsulates hit-point management for any damageable entity.
/// Single Responsibility: only manages health state.
/// </summary>
public class Health
{
    public int Current { get; private set; }
    public int Max { get; }
    public bool IsDead => Current <= 0;

    public Health(int maxHealth)
    {
        Max = maxHealth;
        Current = maxHealth;
    }

    public void TakeDamage(int amount) =>
        Current = Math.Max(0, Current - amount);

    public void Heal(int amount) =>
        Current = Math.Min(Max, Current + amount);

    public void Reset() => Current = Max;
}
