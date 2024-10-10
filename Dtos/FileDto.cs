using Azure;
using Geology_Api.Models;
using static Dropbox.Api.TeamLog.TrustedTeamsRequestAction;

namespace Geology_Api.Dtos;

public record UploadFileRequestDto (string Name, string CourseCode, IFormFile UploadFile, int Level, string DocType, string CourseName);
public record UpdateFileRequestDto(string Name, string CourseCode, IFormFile UploadFile, int Level, string DocType, int fileId, string CourseName);
public record DropboxTokenAsyn(string AccessToken, DateTime? ExpiringTime, string RefreshToken);
public record RedirectDto(string Redirect);
public record LectureNoteDto(int LevelId, string FilePath, int LectureNoteId, string CourseCode, string NoteName, string CourseName);