using Microsoft.Extensions.Options;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Server.Services;
public class DirectoryServiceOptions
{
	public string RootDirectoryPath { get; set; } = string.Empty;
	public string? WindingCodesJsonPath { get; set; } = null;
}

public class DirectoryService : IDirectoryService
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<DirectoryService> _logger;

	// add the IDataContext to the constructor
	private readonly string _rootDirectory;

	//TODO: Create an Settings class to hold this and other settings/data
	private readonly string[] _allowedExtensions = {
		".mp4", ".pdf", ".webm"
	};

	public DirectoryService(IOptions<DirectoryServiceOptions> options, ILogger<DirectoryService> logger) {
		_logger = logger;
		_rootDirectory = options.Value.RootDirectoryPath;
	}

	public Task<List<string>> ListPdfFiles(string? path = null) {
		SearchOption searchOption = path == null ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		path ??= AppConfig.BasePath;
		var files = Directory.EnumerateFiles(path, "*.*", searchOption)
			.Where(f => f.EndsWith(".pdf"))
			.OrderBy(f => f)
			.ToList();
		return Task.FromResult(files
			.Select(f
				=> f
					.Replace(AppConfig.BasePath, "")
					.Replace("\\", "/"))
			.ToList());

	}

	public Task<List<string>> ListVideoFiles(string? path = null) {
		SearchOption searchOption = path == null ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		path ??= AppConfig.BasePath;

		var mp4Files = Directory.EnumerateFiles(path, "*.*", searchOption)
			.Where(f => f.EndsWith(".mp4"))
			.OrderBy(f => f)
			.ToList();
		return Task.FromResult(mp4Files
			.Select(f => f
				.Replace(AppConfig.BasePath, "")
				.Replace("\\", "/"))
			.ToList());
	}

	#region Refactor Later
	public Task<(string, string[], string[])> GetFolderContent(string? path = null) {
		string directory = path ?? _rootDirectory;
		string[] files = Directory.EnumerateFiles(directory).Where(f => _allowedExtensions.Any(f.EndsWith)).OrderBy(f => f).ToArray();
		string[] folders = Directory.GetDirectories(directory).OrderBy(f => f).ToArray();

		// _logger.LogInformation("FolderContent: \n Path: {Path} \n Files: {Files} \n Folder: {Folders}", path, files, folders);
		return Task.FromResult((directory, files, folders));
	}
	public string GetRelativePath(string path) {
		string relativePath = Path.GetRelativePath(_rootDirectory, path);
		Console.WriteLine($"Root: {_rootDirectory} \n Path: {path} \n Relative: {relativePath}");
		return relativePath;
	}
	public Task<string[]> GetFoldersInPath(string? path = null) {
		Console.WriteLine("GettingFoldersInPath");
		// return a list of all folders starting from the root directory
		path ??= _rootDirectory;
		var folders = Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
		// foreach folder in the list, if the folder uses "\\" as a separator, replace it with "/"
		// also remove AppConfig.BasePath from the folder path
		folders = folders.Select(f => f.Replace(AppConfig.BasePath, "").Replace("\\", "/"));
		Console.WriteLine("Completed");
		return Task.FromResult(folders.ToArray());
	}

	#endregion
}
