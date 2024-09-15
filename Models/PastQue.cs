namespace Geology_Api.Models;

public class PastQue
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FilePath { get; set; }
    public int LevelId { get; set; }
    public Level level { get; set; }
}
