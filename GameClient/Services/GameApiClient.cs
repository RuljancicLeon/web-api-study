using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GameClient.Services;

/// <summary>
/// Data-transfer objects that mirror the models in GameApi.
/// </summary>
public class GridStateDto
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public bool[][] Cells { get; set; } = [];
}

public class CellUpdateDto
{
    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsAlive { get; set; }
}

/// <summary>
/// Thin HTTP client for talking to the GameApi.
/// All methods are fire-and-forget friendly: failures are swallowed so the
/// game keeps running even when the API is unavailable.
/// </summary>
public class GameApiClient
{
    private readonly HttpClient _http;

    public GameApiClient(string baseUrl = "http://localhost:5078")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    /// <summary>Fetches the current grid state from the API.</summary>
    public async Task<GridStateDto?> GetGridAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<GridStateDto>("api/grid");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Pushes a cell update to the API.</summary>
    public async Task UpdateCellAsync(int row, int col, bool isAlive)
    {
        try
        {
            var dto = new CellUpdateDto { Row = row, Column = col, IsAlive = isAlive };
            await _http.PutAsJsonAsync("api/grid/cell", dto);
        }
        catch { /* API unavailable – ignore */ }
    }

    /// <summary>Toggles a cell on the server.</summary>
    public async Task ToggleCellAsync(int row, int col)
    {
        try
        {
            await _http.PostAsync($"api/grid/cell/{row}/{col}/toggle", null);
        }
        catch { /* API unavailable – ignore */ }
    }

    /// <summary>Resets the entire grid on the server.</summary>
    public async Task ResetAsync()
    {
        try
        {
            await _http.PostAsync("api/grid/reset", null);
        }
        catch { /* API unavailable – ignore */ }
    }
}
