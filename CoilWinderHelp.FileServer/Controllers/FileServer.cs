using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;


namespace CoilWinderHelp.FileServer.Controllers;
[Route("/")]
public class FileServer : Controller
{

    [HttpGet("file/{*path}")]
    public IActionResult GetFile(string path)
    {
        var filePath = FileSystemPath(path);
        if (filePath == null)
        {
            return NotFound();
        }

        // Read file into memory
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        var fileName = Path.GetFileName(filePath);
        // Get the file's MIME type
        new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);// Return the file to the client
        return File(fileBytes, contentType!, Path.GetFileName(filePath));
    }

    [HttpGet("video/{path}")]
    public async Task<IActionResult> GetVideo(string path)
    {
        var videoPath = FileSystemPath(path);
        if (videoPath == null)
        {
            return await Task.FromResult<IActionResult>(NotFound());
        }

        var videoStream = new FileStream(
        path: videoPath, 
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        bufferSize: 1024,
        useAsync: true);
        var videoName = Path.GetFileName(videoPath);
        new FileExtensionContentTypeProvider().TryGetContentType(videoName, out var contentType);// Return the file to the client


        var response = new FileStreamResult(videoStream, contentType ?? "video/mp4")
        {
            FileDownloadName = videoName,
            EnableRangeProcessing = true,


        };

        return await Task.FromResult<IActionResult>(response);
    }

    // create a new method that formats the path from the api calls and returns true if the file exists
    private static string? FileSystemPath(string path)
    {
        path = path.Replace("%20", " ").Replace("%2F", "/");
        var filePath = Path.Combine("B:/CoilWinderTraining-Edit/", path);

        return System.IO.File.Exists(filePath) ? filePath : null;
    }
}
