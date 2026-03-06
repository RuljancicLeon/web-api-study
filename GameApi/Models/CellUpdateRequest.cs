namespace GameApi.Models;

public class CellUpdateRequest
{
    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsAlive { get; set; }
}
