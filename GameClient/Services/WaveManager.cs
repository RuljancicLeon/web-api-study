using Microsoft.Xna.Framework;
using GameClient.Entities;

namespace GameClient.Services;

/// <summary>
/// Single Responsibility: manages wave progression, countdown timing, and difficulty scaling.
/// Outputs a spawn recipe (list of EnemyConfig) — Game1 stays dumb about composition.
/// </summary>
public class WaveManager
{
    private double _countdownTimer;

    private const double CountdownDuration = 5.0;
    private const int BaseEnemyCount = 3;
    private const int EnemiesPerWave = 1;
    private const double HpScalePerWave = 0.05;

    public int CurrentWave { get; private set; } = 1;
    public bool IsInCountdown { get; private set; }
    public double CountdownRemaining => _countdownTimer;

    /// <summary>
    /// Builds the enemy composition for the current wave.
    /// Progression tiers:
    ///   1–3  All Default, count grows
    ///   4+   Heavies introduced (1 more every 3 waves)
    ///   7+   Fast enemies introduced (1 more every 3 waves)
    ///   Every 10th wave adds a Boss
    ///   All enemy HP scales +5% per wave
    /// </summary>
    public List<EnemyConfig> GetWaveComposition()
    {
        int totalCount = BaseEnemyCount + (CurrentWave - 1) * EnemiesPerWave;

        int heavyCount = CurrentWave >= 4 ? 1 + (CurrentWave - 4) / 3 : 0;
        int fastCount  = CurrentWave >= 7 ? 1 + (CurrentWave - 7) / 3 : 0;
        bool hasBoss   = CurrentWave % 10 == 0;

        // Ensure special enemies don't exceed total; leave at least 1 Default
        int specialSlots = heavyCount + fastCount;
        if (specialSlots >= totalCount)
        {
            double ratio = (totalCount - 1.0) / specialSlots;
            heavyCount = (int)(heavyCount * ratio);
            fastCount  = (int)(fastCount * ratio);
        }

        int defaultCount = totalCount - heavyCount - fastCount;
        double hpScale = 1.0 + (CurrentWave - 1) * HpScalePerWave;

        var composition = new List<EnemyConfig>(totalCount + (hasBoss ? 1 : 0));

        if (hasBoss)
            composition.Add(Scale(EnemyConfig.Boss, hpScale));

        for (int i = 0; i < heavyCount; i++)
            composition.Add(Scale(EnemyConfig.Heavy, hpScale));

        for (int i = 0; i < fastCount; i++)
            composition.Add(Scale(EnemyConfig.Fast, hpScale));

        for (int i = 0; i < defaultCount; i++)
            composition.Add(Scale(EnemyConfig.Default, hpScale));

        return composition;
    }

    /// <summary>
    /// Call each frame with the number of living enemies.
    /// Returns true once when the countdown finishes and a new wave should spawn.
    /// </summary>
    public bool Update(GameTime gameTime, int aliveEnemies)
    {
        if (aliveEnemies > 0)
        {
            IsInCountdown = false;
            return false;
        }

        if (!IsInCountdown)
        {
            IsInCountdown = true;
            _countdownTimer = CountdownDuration;
        }

        _countdownTimer -= gameTime.ElapsedGameTime.TotalSeconds;

        if (_countdownTimer <= 0)
        {
            CurrentWave++;
            IsInCountdown = false;
            return true;
        }

        return false;
    }

    public void Reset()
    {
        CurrentWave = 1;
        IsInCountdown = false;
        _countdownTimer = 0;
    }

    private static EnemyConfig Scale(EnemyConfig config, double hpScale) =>
        config with { MaxHealth = (int)(config.MaxHealth * hpScale) };
}