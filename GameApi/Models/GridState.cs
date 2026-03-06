namespace GameApi.Models;

public class GridState
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public bool[][] Cells { get; set; } = [];
}
