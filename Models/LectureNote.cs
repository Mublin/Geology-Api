using System.ComponentModel.DataAnnotations;

namespace Geology_Api.Models;

public class LectureNote
{
    public int LectureNoteId { get; set; }
    [Required]
    public string NoteName { get; set; }
    public string FilePath { get; set; }
    public string CourseCode { get; set; }
    public int LevelId { get; set; }
    public Level level { get; set; }

}
