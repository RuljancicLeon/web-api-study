using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameClient.Grid;
using GameClient.Services;

namespace GameClient;

public class Game1 : Game
{
    // ── MonoGame boilerplate ───────────────────────────────────────────────────
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    // A 1×1 white pixel used to draw filled rectangles.
    private Texture2D _pixel = null!;

    // ── Grid ───────────────────────────────────────────────────────────────────
    private const int Rows = 20;
    private const int Cols = 20;
    private const int CellSize = 28;
    private Grid.Grid _grid = null!;

    // ── Web API client ─────────────────────────────────────────────────────────
    private readonly GameApiClient _api = new();

    // ── Input state ───────────────────────────────────────────────────────────
    private MouseState _prevMouse;
    private KeyboardState _prevKeys;

    // ── UI ────────────────────────────────────────────────────────────────────
    private SpriteFont? _font;
    private string _statusMessage = "Click cells to toggle | R = Reset | S = Sync from API";

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
        int gridOffsetX = 10;
        int gridOffsetY = 40;
        _grid = new Grid.Grid(Rows, Cols, CellSize,
                              new Vector2(gridOffsetX, gridOffsetY));
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create the 1×1 white pixel texture used for all rectangle drawing.
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        // Load the default MonoGame content font if available; otherwise font stays null.
        try { _font = Content.Load<SpriteFont>("Font"); }
        catch { _font = null; }
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keys = Keyboard.GetState();
        MouseState mouse  = Mouse.GetState();

        // Exit
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
            || keys.IsKeyDown(Keys.Escape))
            Exit();

        // Left-click: toggle the clicked cell locally and push to API.
        if (mouse.LeftButton == ButtonState.Pressed
            && _prevMouse.LeftButton == ButtonState.Released)
        {
            if (_grid.TryGetCellAt(new Vector2(mouse.X, mouse.Y), out int row, out int col))
            {
                _grid.ToggleCell(row, col);
                _ = _api.ToggleCellAsync(row, col);
                _statusMessage = $"Toggled cell ({row},{col})";
            }
        }

        // R: reset grid locally and on the API.
        if (keys.IsKeyDown(Keys.R) && !_prevKeys.IsKeyDown(Keys.R))
        {
            _grid.Reset();
            _ = _api.ResetAsync();
            _statusMessage = "Grid reset";
        }

        // S: sync the local grid from the API.
        if (keys.IsKeyDown(Keys.S) && !_prevKeys.IsKeyDown(Keys.S))
        {
            _ = SyncFromApiAsync();
            _statusMessage = "Syncing from API…";
        }

        _prevMouse = mouse;
        _prevKeys  = keys;

        base.Update(gameTime);
    }

    private async System.Threading.Tasks.Task SyncFromApiAsync()
    {
        GridStateDto? state = await _api.GetGridAsync();
        if (state is null)
        {
            _statusMessage = "API unavailable";
            return;
        }

        for (int r = 0; r < state.Rows && r < Rows; r++)
            for (int c = 0; c < state.Columns && c < Cols; c++)
                _grid.SetCell(r, c, state.Cells[r][c]);

        _statusMessage = "Synced from API";
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 20, 20));

        _spriteBatch.Begin();

        _grid.Draw(_spriteBatch, _pixel);

        // Status bar
        if (_font is not null)
        {
            _spriteBatch.DrawString(_font, _statusMessage, new Vector2(10, 6), Color.LightGray);

            // Legend on the right side
            int legendX = Cols * CellSize + 20;
            int legendY = 40;
            _spriteBatch.DrawString(_font, "Controls:", new Vector2(legendX, legendY), Color.Yellow);
            _spriteBatch.DrawString(_font, "Click  - toggle cell", new Vector2(legendX, legendY + 20), Color.LightGray);
            _spriteBatch.DrawString(_font, "R      - reset grid",   new Vector2(legendX, legendY + 40), Color.LightGray);
            _spriteBatch.DrawString(_font, "S      - sync from API", new Vector2(legendX, legendY + 60), Color.LightGray);
            _spriteBatch.DrawString(_font, "Esc    - quit",          new Vector2(legendX, legendY + 80), Color.LightGray);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
