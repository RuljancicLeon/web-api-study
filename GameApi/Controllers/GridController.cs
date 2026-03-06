using GameApi.Models;
using GameApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GridController : ControllerBase
{
    private readonly GridService _gridService;

    public GridController(GridService gridService)
    {
        _gridService = gridService;
    }

    /// <summary>
    /// Returns the current grid state (rows, columns, and cell values).
    /// </summary>
    [HttpGet]
    public ActionResult<GridState> GetGrid()
    {
        return Ok(_gridService.GetState());
    }

    /// <summary>
    /// Updates a single cell's value.
    /// </summary>
    [HttpPut("cell")]
    public IActionResult UpdateCell([FromBody] CellUpdateRequest request)
    {
        bool updated = _gridService.SetCell(request.Row, request.Column, request.IsAlive);
        if (!updated)
            return BadRequest("Cell coordinates are out of bounds.");

        return NoContent();
    }

    /// <summary>
    /// Toggles a single cell (alive ↔ dead).
    /// </summary>
    [HttpPost("cell/{row:int}/{col:int}/toggle")]
    public IActionResult ToggleCell(int row, int col)
    {
        bool toggled = _gridService.ToggleCell(row, col);
        if (!toggled)
            return BadRequest("Cell coordinates are out of bounds.");

        return NoContent();
    }

    /// <summary>
    /// Resets the entire grid to its default empty state.
    /// </summary>
    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _gridService.Reset();
        return NoContent();
    }
}
