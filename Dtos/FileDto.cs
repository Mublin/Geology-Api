namespace Geology_Api.Dtos;

public record UploadFileRequestDto (string Name, string CourseCode, IFormFile UploadFile, int Level, string DocType);
public record UpdateFileRequestDto(string Name, string CourseCode, IFormFile UploadFile, int Level, string DocType, int fileId);