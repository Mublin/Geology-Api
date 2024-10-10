using Dropbox.Api;
using Geology_Api.Data;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Mvc;
using Geology_Api.Dtos;
using Geology_Api.Services;
using Geology_Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Geology_Api.Controllers
{
    [Route("api/files/")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly GeologyStoreContext _context;
        private readonly IConfiguration _config;
        public FileController(GeologyStoreContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        
        // Endpoint to get OAuth2 URL for authentication


        [HttpGet("auth-url")]
        public IActionResult GetDropboxAuthUrl()
        {
            var newUrl = Request.Host.Value + Request.Path.Value;
            var authorizeUri = UserServices.GetAuthUrl(_config, newUrl);
            return Ok(authorizeUri);
        }


        [HttpPost("token")]
        public async Task<IActionResult> GetDropboxToken(string code, RedirectDto redirect)
        {
            
            try
            {
                var data = await UserServices.GetDropboxTokenHandlerAsync(code, _config, redirect.Redirect);
                return Ok(new { AccessToken = data.AccessToken, ExpiringTime = data.ExpiringTime, RefreshToken = data.RefreshToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequestDto file, string accessToken)
        {
            var newUrl = Request.Host.Value + Request.Path.Value;
            Info info = UserServices.GetInfo(_config, newUrl);
            string ValidateFile = file.FileValidation();
            if (ValidateFile != "true")
            {
                return BadRequest(ValidateFile);
            }
            Console.WriteLine(file.CourseName);
            using (var dbx = new DropboxClient(accessToken))
            {
                using (var stream = file.UploadFile.OpenReadStream())
                {
                    // Ensure the file stream is at the beginning
                    stream.Seek(0, SeekOrigin.Begin);
                    string path = $"/geologydb/{file.DocType}/{file.Level}/{file.CourseCode}/{DateTime.Now}{file.CourseName}";
                    var uploadResult = await dbx.Files.UploadAsync(
                        path, // Adjust path based on CourseCode and Name
                        Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                        body: stream);
                    LectureNote newLectureNote = new LectureNote()
                    {
                        NoteName = file.Name,
                        CourseCode = file.CourseCode,
                        FilePath = path,
                        CourseName = file.CourseName
                        
                    };
                    Level level = new Level()
                    {
                        LevelNo = file.Level,
                    };
                    newLectureNote.level = level;
                    await _context.AddAsync(newLectureNote);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "File created successfully", uploadResult });
                }
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateFile(int id, [FromForm] UploadFileRequestDto file, string accessToken)
        {
            var findFile = await _context.LectureNotes.SingleOrDefaultAsync(x=> x.LectureNoteId == id);
            if (findFile == null)
            {
                return NotFound("File not found");
            }
            string ValidateFile = file.FileValidation();
            if (ValidateFile != "true")
            {
                return BadRequest(ValidateFile);
            }
            using (var dbx = new DropboxClient(accessToken))
            {
                using (var stream = file.UploadFile.OpenReadStream())
                {
                    // Ensure the file stream is at the beginning
                    stream.Seek(0, SeekOrigin.Begin);
                    var uploadResult = await dbx.Files.UploadAsync(
                        findFile.FilePath, // Adjust path based on CourseCode and Name
                        Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                        body: stream);
                    findFile.NoteName = file.CourseName;
                    findFile.CourseCode = file.CourseCode;
                    Level level = new Level()
                    {
                        LevelNo = file.Level
                    };
                    findFile.level = level;
                    await _context.SaveChangesAsync();
                    return Ok(uploadResult);
                }
            }
        }


        [HttpGet("lecturenotes/{id}")]
        public async Task<IActionResult> GetLevelNotes(int id)
        {
            var files = await _context.Levels
                                            .Where(x => x.LevelNo == id)
                                            .SelectMany(l => l.LectureNotes)
                                            .Select(ln => 
                                                ln.lectureNoteToDto(id)
                                            )
                                            .ToListAsync();

            if (files == null)
            {
                return BadRequest("Invalid level");
            }
            return Ok(files);
        }


        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id, string accessToken)
        {
            var file = _context.LectureNotes.SingleOrDefault(x => x.LectureNoteId == id);
            if (file == null)
            {
                return NotFound("Item not found");
            }
            using (var dbx = new DropboxClient(accessToken))
            {
                var downloadResult = await dbx.Files.DownloadAsync(file.FilePath);
                var fileContent = await downloadResult.GetContentAsByteArrayAsync();
                
                return File(fileContent, "application/octet-stream", file.CourseName);
            }
        }

        [Authorize(Policy = "AdminAccess")]
        [HttpGet("files")]
        public async Task<IActionResult> GetFiles(string accessToken, string fileName, string notetype, int level, string coursecode)
        {
            var newUrl = Request.Host.Value + Request.Path.Value;
            Info info = UserServices.GetInfo(_config, newUrl);

            using (var dbx = new DropboxClient(accessToken))
            {
                try
                {
                    // List files in the specified folder
                    var listFolderResult = await dbx.Files.ListFolderAsync($"{info.folderPath}/{notetype}/{level}/{coursecode}");

                    // Extract file names and paths
                    var files = listFolderResult.Entries
                        .Where(i => i.IsFile)  // Filter for files only
                        .Select(i => new { i.Name, i.PathLower })  // Return file name and path
                        .ToList();

                    return Ok(files);
                }
                catch (Dropbox.Api.ApiException<Dropbox.Api.Files.ListFolderError> ex)
                {
                    // Handle exceptions (e.g., folder not found)
                    return BadRequest(new { message = $"Error fetching files: {ex.Message}" });
                }
            }
        }


        [Authorize(Policy = "AdminAccess")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFile(int id, string accessToken)
        {
            var file = await _context.LectureNotes.FirstOrDefaultAsync(x => x.LectureNoteId == id);
            if (file == null)
            {
                return NotFound("File not found");
            }
            
            using (var dbx = new DropboxClient(accessToken))
            {
                try
                {
                    // Delete the file from Dropbox
                    var deleteResult = await dbx.Files.DeleteV2Async(file.FilePath);
                    _context.LectureNotes.Remove(file);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "File deleted successfully", deleteResult.Metadata });
                }
                catch (Dropbox.Api.ApiException<Dropbox.Api.Files.DeleteError> ex)
                {
                    // Handle error if the file doesn't exist or other API errors
                    return BadRequest(new { message = $"Error deleting file: {ex.Message}" });
                }
            }
        }

    }
}
