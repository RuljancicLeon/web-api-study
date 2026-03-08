using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameClient.Entities;
using GameClient.Services;

namespace GameClient;
//IDEJA: Svaka runda vise neprijatelja(gotov), ili jaci neprijatelji, stvore se powerup, moze se postaviti neki broj zidova nakon runde, 
//zidovi se ruse kad ih dodiruju enemy, boss fight nakon 10 runde. Sve to u mulitplayeru bude zabavno.
//ideja za kasnije je full inventory sistem.
public class Game1 : Game
{
    // ── MonoGame boilerplate ───────────────────────────────────────────────────
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    // A 1×1 white pixel used to draw filled rectangles.
    private Texture2D _pixel = null!;

    // ── Grid ───────────────────────────────────────────────────────────────────
    private const int Rows = 20;
    private const int Cols = 20;
    private const int CellSize = 28;
    private Grid.Grid _grid = null!;

    // ── Entities ───────────────────────────────────────────────────────────────
    private Player _player = null!;
    private readonly List<Enemy> _enemies = [];
    private readonly List<Projectile> _projectiles = [];

    // ── Wave management ───────────────────────────────────────────────────────
    private readonly WaveManager _waveManager = new();

    // ── Input state ───────────────────────────────────────────────────────────
    private MouseState _prevMouse;
    private KeyboardState _prevKeys;

    // ── UI ────────────────────────────────────────────────────────────────────
    private SpriteFont? _font;
    private string _statusMessage = "WASD = Move | Click = Shoot | Tab = Place walls | R = Reset";

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth  = Cols * CellSize + 200,
            PreferredBackBufferHeight = Rows * CellSize + 60
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _grid = new Grid.Grid(Rows, Cols, CellSize, new Vector2(10, 40));
        _player = new Player(Rows / 2, Cols / 2);
        _grid.Occupy(_player.Row, _player.Col);
        SpawnEnemies();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        try { _font = Content.Load<SpriteFont>("Font"); }
        catch { _font = null; }
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keys = Keyboard.GetState();
        MouseState mouse  = Mouse.GetState();

        if (keys.IsKeyDown(Keys.Escape))
            Exit();

        // R: reset game (always available)
        if (keys.IsKeyDown(Keys.R) && !_prevKeys.IsKeyDown(Keys.R))
        {
            ResetGame();
            _prevKeys  = keys;
            _prevMouse = mouse;
            return;
        }

        // Freeze updates when the player is dead
        if (_player.IsDead)
        {
            _statusMessage = $"GAME OVER — Wave {_waveManager.CurrentWave} | Press R to restart";
            _prevKeys  = keys;
            _prevMouse = mouse;
            return;
        }

        // ── Entity updates ─────────────────────────────────────────────────
        _player.Update(gameTime, _grid);

        // Spawn projectile in the aim direction
        if (_player.IsAttacking && _player.AimedCell is var (aimR, aimC))
        {
            if (_grid.InBounds(aimR, aimC) && !_grid.IsAlive(aimR, aimC))
            {
                int dRow = aimR - _player.Row;
                int dCol = aimC - _player.Col;
                _projectiles.Add(new Projectile(aimR, aimC, dRow, dCol, _player.AttackDamage));
            }
        }

        // Update projectiles and resolve hits
        foreach (var projectile in _projectiles)
            projectile.Update(gameTime, _grid);

        CollisionService.HandleProjectileCollisions(_projectiles, _enemies);
        _projectiles.RemoveAll(p => !p.IsActive);

        // Free cells of newly dead enemies so living ones can move through
        foreach (var enemy in _enemies)
        {
            if (enemy.IsDead && _grid.IsOccupied(enemy.Row, enemy.Col))
                _grid.Vacate(enemy.Row, enemy.Col);
        }

        foreach (var enemy in _enemies)
        {
            if (!enemy.IsDead)
                enemy.Update(gameTime, _grid);
        }

        CollisionService.HandlePlayerEnemyCollisions(_player, _enemies);

        // ── Tab: place / remove walls ──────────────────────────────────────
        if (keys.IsKeyDown(Keys.Tab) && !_prevKeys.IsKeyDown(Keys.Tab))
        {
            if (_grid.TryGetCellAt(new Vector2(mouse.X, mouse.Y), out int row, out int col))
                _grid.ToggleCell(row, col);
        }

        // ── Wave progression ───────────────────────────────────────────────
        int aliveEnemies = _enemies.Count(e => !e.IsDead);

        if (_waveManager.Update(gameTime, aliveEnemies))
        {
            SpawnEnemies();
            _player.Ammo.Refill();
        }
            

        // ── Status line ────────────────────────────────────────────────────
        if (_waveManager.IsInCountdown)
            _statusMessage = $"Wave {_waveManager.CurrentWave} cleared! Next wave in {_waveManager.CountdownRemaining:F1}s...";
        else
            _statusMessage = $"HP: {_player.Health.Current}/{_player.Health.Max} | Enemies: {aliveEnemies}/{_enemies.Count} | Wave: {_waveManager.CurrentWave}";

        _prevMouse = mouse;
        _prevKeys  = keys;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 20, 20));

        _spriteBatch.Begin();

        _grid.Draw(_spriteBatch, _pixel);

        // Targeting indicator on the aimed cell
        if (_player.AimedCell is var (aimRow, aimCol))
        {
            Rectangle aimRect = _grid.GetCellRect(aimRow, aimCol);
            _spriteBatch.Draw(_pixel, aimRect, new Color(32, 32, 32, 64));
        }

        foreach (var enemy in _enemies)
        {
            if (!enemy.IsDead)
                enemy.Draw(_spriteBatch, _pixel, _grid);
        }

        _player.Draw(_spriteBatch, _pixel, _grid);

        foreach (var projectile in _projectiles)
            projectile.Draw(_spriteBatch, _pixel, _grid);

        if (_font is not null)
        {
            _spriteBatch.DrawString(_font, _statusMessage, new Vector2(10, 6), Color.LightGray);

            DrawPlayerHealthBar();

            int legendX = Cols * CellSize + 20;
            int legendY = 40;
            _spriteBatch.DrawString(_font, "Controls:",            new Vector2(legendX, legendY),       Color.Yellow);
            _spriteBatch.DrawString(_font, "WASD   - move",        new Vector2(legendX, legendY + 20),  Color.LightGray);
            _spriteBatch.DrawString(_font, "Click  - shoot",       new Vector2(legendX, legendY + 40),  Color.LightGray);
            _spriteBatch.DrawString(_font, "Tab    - place walls",  new Vector2(legendX, legendY + 60),  Color.LightGray);
            _spriteBatch.DrawString(_font, "R      - reset",       new Vector2(legendX, legendY + 80),  Color.LightGray);
            _spriteBatch.DrawString(_font, "Esc    - quit",        new Vector2(legendX, legendY + 100), Color.LightGray);
            _spriteBatch.DrawString(_font, $"Ammo: { _player.Ammo.Current}/{ _player.Ammo.Max}", new Vector2(legendX, legendY + 160), Color.Yellow);
        
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void DrawPlayerHealthBar()
    {
        int legendX  = Cols * CellSize + 20;
        int barY     = 180;
        int barWidth = 160;
        int barHeight = 16;

        _spriteBatch.DrawString(_font!, "Health:", new Vector2(legendX, barY - 20), Color.LightGray);

        // Background
        _spriteBatch.Draw(_pixel, new Rectangle(legendX, barY, barWidth, barHeight), new Color(60, 0, 0));

        // Fill
        float ratio = (float)_player.Health.Current / _player.Health.Max;
        Color barColor = ratio > 0.5f  ? new Color(80, 200, 120) :
                         ratio > 0.25f ? new Color(220, 180, 50) :
                                         new Color(220, 50, 50);
        int fillWidth = (int)(barWidth * ratio);
        _spriteBatch.Draw(_pixel, new Rectangle(legendX, barY, fillWidth, barHeight), barColor);
    }

    private void SpawnEnemies()
    {
        _enemies.Clear();
        var random = new Random();
        var composition = _waveManager.GetWaveComposition();

        foreach (var config in composition)
        {
            int row, col;
            do
            {
                row = random.Next(Rows);
                col = random.Next(Cols);
            }
            while (!_grid.IsWalkable(row, col)
                   || Math.Abs(row - _player.Row) + Math.Abs(col - _player.Col) < 5);

            _enemies.Add(new Enemy(row, col, _player, config));
            _grid.Occupy(row, col);
        }
    }

    private void ResetGame()
    {
        _grid.Reset();
        _player.ResetTo(Rows / 2, Cols / 2, _grid);
        _projectiles.Clear();
        _waveManager.Reset();
        SpawnEnemies();
        _statusMessage = "Game reset!";
    }
}
