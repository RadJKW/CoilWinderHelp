using System.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MudBlazorPWA.Shared.Services;
public interface IDirectoryService
{
    Task<(string, string[], string[])> GetFolderContent(string? path = null);
    Task<string> GetRelativePath(string path);

    Task ExportWindingCodesToJson(IEnumerable windingCodes);
}

public class DirectoryServiceOptions
{
    public string RootDirectoryPath { get; set; } = string.Empty;
}

public class DirectoryService : IDirectoryService
{
    // ReSharper disable once NotAccessedField.Local
    private readonly ILogger<DirectoryService> _logger;
    private readonly string _rootDirectory;

    private readonly string[] _allowedExtensions =
    {
        ".mp4", ".pdf", ".webm"
    };

    public DirectoryService(IOptions<DirectoryServiceOptions> options, ILogger<DirectoryService> logger)
    {
        _logger = logger;
        _rootDirectory = options.Value.RootDirectoryPath;
    }

    // Used by the hub to get the contents of the root
    public Task<(string, string[], string[])> GetFolderContent(string? path = null)
    {
        var directory = path ?? _rootDirectory;

        var files = Directory.EnumerateFiles(directory)
            .Where(f => _allowedExtensions.Any(f.EndsWith))
            .OrderBy(f => f)
            .ToArray();
        var folders = Directory.GetDirectories(directory)
            .OrderBy(f => f)
            .ToArray();

        // _logger.LogInformation("FolderContent: \n Path: {Path} \n Files: {Files} \n Folder: {Folders}", path, files, folders);

        return Task.FromResult((directory, files, folders));

    }

    public Task<string> GetRelativePath(string path)
    {
        return
            Task.FromResult(Path.GetRelativePath(_rootDirectory, path));

    }

    public async Task ExportWindingCodesToJson(IEnumerable windingCodes)
    {
        var json = JsonConvert.SerializeObject(windingCodes, Formatting.Indented);
        try
        {
            await File.WriteAllTextAsync($"{_rootDirectory}WindingStops.json", json);

        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while exporting the database : {Error}", ex);
            throw;
        }

    }

}
