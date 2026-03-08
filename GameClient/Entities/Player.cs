using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameClient.Entities;

/// <summary>
/// Keyboard-controlled player entity with melee attack and invincibility frames.
/// Liskov Substitution: fully substitutable wherever a <see cref="GameEntity"/> is expected.
/// </summary>
public class Player(int row, int col) : GameEntity(row, col, new Color(50, 140, 255), maxHealth: 100)
{
    private KeyboardState _previousKeys;
    private MouseState _previousMouse;
    private double _invincibilityTimer;
    private double _moveTimer;
    private bool _moveHeld;

    private const double InvincibilityDuration = 1.0;
    private const double MoveInitialDelay = 0.18;
    private const double MoveRepeatInterval = 0.09;

    public int AttackDamage { get; } = 25;
    public Ammo Ammo { get; } = new(15);
    public bool IsInvincible => _invincibilityTimer > 0;
    public bool IsAttacking { get; private set; }

    /// <summary>
    /// The adjacent cell the cursor is currently pointing at, or null if out of bounds.
    /// Updated every frame so a targeting indicator can be drawn.
    /// </summary>
    public (int Row, int Col)? AimedCell { get; private set; }

    public override void Update(GameTime gameTime, Grid.Grid grid)
    {
        if (_invincibilityTimer > 0)
            _invincibilityTimer -= gameTime.ElapsedGameTime.TotalSeconds;

        KeyboardState keys = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();
        HandleMovement(keys, gameTime, grid);
        UpdateAim(grid);

        IsAttacking = IsMouseButtonPressed(mouse) && Ammo.TryConsume();

        _previousKeys = keys;
        _previousMouse = mouse;
    }

    /// <summary>
    /// Determines which adjacent cell the cursor is pointing at by snapping
    /// the player-to-cursor vector to the nearest cardinal direction.
    /// </summary>
    private void UpdateAim(Grid.Grid grid)
    {
        MouseState mouse = Mouse.GetState();
        Rectangle cellRect = grid.GetCellRect(Row, Col);
        Vector2 playerCenter = new(cellRect.X + cellRect.Width / 2f, cellRect.Y + cellRect.Height / 2f);
        Vector2 direction = new Vector2(mouse.X, mouse.Y) - playerCenter;

        if (direction.LengthSquared() < 1f)
        {
            AimedCell = null;
            return;
        }

        int dRow, dCol;
        if (Math.Abs(direction.X) >= Math.Abs(direction.Y))
        {
            dRow = 0;
            dCol = direction.X >= 0 ? 1 : -1;
        }
        else
        {
            dRow = direction.Y >= 0 ? 1 : -1;
            dCol = 0;
        }

        int targetRow = Row + dRow;
        int targetCol = Col + dCol;
        AimedCell = grid.InBounds(targetRow, targetCol) ? (targetRow, targetCol) : null;
    }

    private void HandleMovement(KeyboardState keys, GameTime gameTime, Grid.Grid grid)
    {
        bool anyMoveKey = keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up)
                       || keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down)
                       || keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left)
                       || keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right);

        if (!anyMoveKey)
        {
            _moveTimer = 0;
            _moveHeld = false;
            return;
        }

        if (_moveTimer > 0)
        {
            _moveTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            return;
        }

        if (keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up))    TryMove(-1, 0, grid);
        if (keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down))  TryMove(1, 0, grid);
        if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left))  TryMove(0, -1, grid);
        if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right)) TryMove(0, 1, grid);

        _moveTimer = _moveHeld ? MoveRepeatInterval : MoveInitialDelay;
        _moveHeld = true;
    }

    /// <summary>
    /// Polymorphic override: adds invincibility-frame protection before delegating to base.
    /// </summary>
    public override void TakeDamage(int amount)
    {
        if (IsInvincible) return;
        base.TakeDamage(amount);
        _invincibilityTimer = InvincibilityDuration;
    }

    /// <summary>Flashes the sprite during invincibility frames.</summary>
    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel, Grid.Grid grid)
    {
        if (IsInvincible && (int)(_invincibilityTimer * 10) % 2 == 0)
            return;

        base.Draw(spriteBatch, pixel, grid);
    }

    public void ResetTo(int row, int col, Grid.Grid grid)
    {
        grid.Vacate(Row, Col);
        Row = row;
        Col = col;
        Health.Reset();
        _invincibilityTimer = 0;
        IsAttacking = false;
        AimedCell = null;
        Ammo.Refill();
        grid.Occupy(Row, Col);
    }

    private bool IsKeyPressed(KeyboardState current, Keys key) =>
        current.IsKeyDown(key) && !_previousKeys.IsKeyDown(key);
    
    private bool IsMouseButtonPressed(MouseState mouse) =>
        (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released);
     
}
