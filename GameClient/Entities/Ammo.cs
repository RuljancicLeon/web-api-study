namespace GameClient.Entities;

/// <summary>
/// Encapsulates ammunition management. Mirrors the <see cref="Health"/> pattern (SRP).
/// </summary>
public class Ammo(int max)
{
    public int Current { get; private set; } = max;
    public int Max { get; } = max;
    public bool IsEmpty => Current <= 0;

    /// <summary>
    /// Atomically checks and consumes one round. Returns true if a round was available.
    /// </summary>
    public bool TryConsume()
    {
        if (Current <= 0) return false;
        Current--;
        return true;
    }

    public void Refill() => Current = Max;

    public void Add(int amount) => Current = Math.Min(Max, Current + amount);
}