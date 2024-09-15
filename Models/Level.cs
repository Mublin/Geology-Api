namespace Geology_Api.Models;

public class Level
{
    public int LevelId { get; set; }
    public int LevelNo { get; set; }
    public ICollection<LectureNote> LectureNotes { get; set; } = new List<LectureNote>();
    public ICollection<PastQue> pastQues { get; set; } = [];
}