using Microsoft.Xna.Framework;

namespace GameClient.Entities;

/// <summary>
/// Defines the stats for an enemy variant. New enemy types = new presets, not new classes.
/// Open/Closed: extend by adding presets, not modifying Enemy.
/// </summary>
public record EnemyConfig(
    int MaxHealth,
    Color Color,
    int AttackDamage,
    int DetectionRange,
    double PatrolSpeed,
    double ChaseSpeed)
{
    public static readonly EnemyConfig Default = new(
        MaxHealth: 50,
        Color: new Color(220, 50, 50),
        AttackDamage: 10,
        DetectionRange: 5,
        PatrolSpeed: 0.5,
        ChaseSpeed: 0.25);

    public static readonly EnemyConfig Heavy = new(
        MaxHealth: 100,
        Color: new Color(140, 50, 180),
        AttackDamage: 25,
        DetectionRange: 4,
        PatrolSpeed: 0.9,
        ChaseSpeed: 0.45);

    public static readonly EnemyConfig Fast = new(
        MaxHealth: 30,
        Color: new Color(50, 200, 220),
        AttackDamage: 8,
        DetectionRange: 6,
        PatrolSpeed: 0.3,
        ChaseSpeed: 0.15);

    public static readonly EnemyConfig Boss = new(
        MaxHealth: 300,
        Color: new Color(255, 180, 0),
        AttackDamage: 35,
        DetectionRange: 20,
        PatrolSpeed: 0.6,
        ChaseSpeed: 0.3);
}