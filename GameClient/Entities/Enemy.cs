using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameClient.AI;

namespace GameClient.Entities;

/// <summary>
/// AI-controlled enemy that uses the State pattern to switch behaviour.
/// Dependency Inversion: delegates all decisions to an <see cref="IEnemyState"/> abstraction.
/// </summary>
public class Enemy(int row, int col, Player target, EnemyConfig config) : GameEntity(row, col, config.Color, config.MaxHealth)
{
    private IEnemyState _currentState = new PatrolState();
    private readonly Player _target = target;

    public int DetectionRange { get; } = config.DetectionRange;
    public int AttackDamage { get; } = config.AttackDamage;
    public double PatrolSpeed { get; } = config.PatrolSpeed;
    public double ChaseSpeed { get; } = config.ChaseSpeed;

    public override void Update(GameTime gameTime, Grid.Grid grid)
    {
        _currentState = _currentState.Update(this, _target, grid, gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel, Grid.Grid grid)
    {
        base.Draw(spriteBatch, pixel, grid);
        DrawHealthBar(spriteBatch, pixel, grid);
    }

    private void DrawHealthBar(SpriteBatch spriteBatch, Texture2D pixel, Grid.Grid grid)
    {
        Rectangle cellRect = grid.GetCellRect(Row, Col);

        int barWidth = cellRect.Width - 4;
        int barHeight = 4;
        int barX = cellRect.X + 2;
        int barY = cellRect.Y - barHeight - 2;

        // Background
        spriteBatch.Draw(pixel, new Rectangle(barX, barY, barWidth, barHeight), new Color(60, 0, 0));

        // Fill
        float ratio = (float)Health.Current / Health.Max;
        int fillWidth = (int)(barWidth * ratio);
        spriteBatch.Draw(pixel, new Rectangle(barX, barY, fillWidth, barHeight), new Color(220, 50, 50));
    }
    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        if (!IsDead)
            _currentState = new ChaseState();
    }
}
