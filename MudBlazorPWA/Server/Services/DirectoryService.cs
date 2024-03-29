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
		Console.WriteLine($"Searching PDFs in {path ?? "BasePath"}");
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
	public async Task<IWindingCode> GetWindingCodeDocuments(IWindingCode code) {
		if (code.FolderPath == null) { return code; }
		Media documents = code.Media;

		// get the pdf path
		try {
			string? pdfPath = await GetPdfPath(code.FolderPath, false);
			if (pdfPath != null) { documents.Pdf = GetRelativePath(pdfPath); }
		}
		catch (Exception e) {
			Console.WriteLine($"TryPdfPath: {e.Message}");
			throw;
		}

		// get the video path
		try {
			string? videoPath = await GetVideoPath(code.FolderPath);
			if (videoPath != null) { documents.Video = GetRelativePath(videoPath); }
		}
		catch (Exception e) {
			Console.WriteLine($"TryVideoPath: {e.Message}");
			throw;
		}

		// get the refMedia path
		try {
			string? refMediaPath = await GetRefMediaPath(code.FolderPath);
			if (refMediaPath != null) { documents.RefMedia = new() {
				GetRelativePath(refMediaPath)
			}; }
		}
		catch (Exception e) {
			Console.WriteLine($"TryRefMediaPath: {e.Message}");
			throw;
		}
		return code;
	}
	private Task<string?> GetPdfPath(string folder, bool relative) {
		string? pdfPath = Directory.EnumerateFiles(folder).FirstOrDefault(f => f.EndsWith(".pdf"));
		if (relative && pdfPath != null) { pdfPath = Path.GetRelativePath(_rootDirectory, pdfPath); }
		return Task.FromResult(pdfPath);
	}
	private static Task<string?> GetVideoPath(string? folder) {
		if (folder == null) { return Task.FromResult<string?>(null); }
		string platformVideoFolder = Path.Combine(AppConfig.BasePath, "TrainingVideos", "Unsorted");
		string[] videos = Directory.EnumerateFiles(platformVideoFolder).Where(f => f.EndsWith(".mp4")).ToArray();
		var random = new Random();
		for (int i = videos.Length - 1; i > 0; i--) {
			int j = random.Next(i + 1);
			(videos[i], videos[j]) = (videos[j], videos[i]);
		}
		int randomNumber = random.Next(videos.Length);
		string videoPath = videos[randomNumber];
		return Task.FromResult(videoPath)!;
	}
	private static Task<string?> GetRefMediaPath(string folder) {
		string? refMediaPath = Directory.EnumerateDirectories(folder).FirstOrDefault(f => f.Contains("Ref", StringComparison.OrdinalIgnoreCase));
		return Task.FromResult(refMediaPath);
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
