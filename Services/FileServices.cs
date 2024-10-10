using Geology_Api.Dtos;
using Geology_Api.Models;
using System.Linq;

namespace Geology_Api.Services;
public static class FileServices
{

    public static string FileValidation(this UploadFileRequestDto file)
    {
        if (file == null || file.UploadFile == null || file.UploadFile.Length == 0)
        {
            return "File not provided or is empty.";
        }
        // Maximum allowed file size: 15 MB
        const long MaxFileSize = 15 * 1024 * 1024;

        // Allowed file extensions
        var allowedExtensions = new[] { ".docx", ".pdf", ".png", ".jpg", ".jpeg", ".doc" };

        // Validate if file is provided
        if (file == null || file.UploadFile == null || file.UploadFile.Length == 0)
        {
            return "File not provided or is empty.";
        }

        // Validate file size
        if (file.UploadFile.Length > MaxFileSize)
        {
            return "File size exceeds the maximum limit of 15 MB.";
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(file.UploadFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return $"Invalid file type. Allowed types are: {string.Join(", ", allowedExtensions)}";
        }
        return "true";
    }
    public static LectureNoteDto lectureNoteToDto(this LectureNote lectureNote, int id)
    {
        LectureNoteDto lectureNote1 = new(
            CourseCode: lectureNote.CourseCode,
            FilePath: lectureNote.FilePath,
            LevelId: id,
            LectureNoteId: lectureNote.LectureNoteId,
            NoteName: lectureNote.NoteName,
            CourseName: lectureNote.CourseName
        );
        return lectureNote1;
    }
}

public record Info(string appKey, string ? redirectUri, string appSecret, string folderPath);