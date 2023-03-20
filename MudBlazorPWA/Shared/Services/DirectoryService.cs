using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MudBlazorPWA.Shared.Services;
public interface IDirectoryService
{
	Task<(string, string[], string[])> GetFolderContent(string? path = null);
	Task<string[]> GetFoldersInPath(string? path = null);
	Task<List<string>> ListPdfFiles(string? path = null);
	Task ExportWindingCodesToJson(IEnumerable<WindingCode> windingCodes, bool syncDatabase);
	Task<IEnumerable<WindingCode>> GetWindingCodesJson(string? path = null);
	Task<WindingCode> GetWindingCodeDocuments(WindingCode code);
	Task UpdateDatabaseWindingCodes(IEnumerable<WindingCode> windingCodes);
	Task<List<string>> ListVideoFiles(string? path = null);
	public string GetRelativePath(string fullPath);
}

public class DirectoryServiceOptions
{
	public string RootDirectoryPath { get; set; } = string.Empty;
	public string? WindingCodesJsonPath { get; set; } = null;
}

public class DirectoryService : IDirectoryService
{
	private readonly string _windingCodesJsonPath;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<DirectoryService> _logger;

	// add the IDataContext to the constructor
	private readonly IDataContext _dataContext;
	private readonly string _rootDirectory;

	//TODO: Create an Settings class to hold this and other settings/data
	private readonly string[] _allowedExtensions = {
		".mp4", ".pdf", ".webm"
	};

	public DirectoryService(IOptions<DirectoryServiceOptions> options, ILogger<DirectoryService> logger, IDataContext dataContext) {
		_logger = logger;
		_dataContext = dataContext;
		_rootDirectory = options.Value.RootDirectoryPath;
		_windingCodesJsonPath = options.Value.WindingCodesJsonPath ?? _rootDirectory + "WindingCodes.json";
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
		var directory = path ?? _rootDirectory;
		var files = Directory.EnumerateFiles(directory).Where(f => _allowedExtensions.Any(f.EndsWith)).OrderBy(f => f).ToArray();
		var folders = Directory.GetDirectories(directory).OrderBy(f => f).ToArray();

		// _logger.LogInformation("FolderContent: \n Path: {Path} \n Files: {Files} \n Folder: {Folders}", path, files, folders);
		return Task.FromResult((directory, files, folders));
	}
	public string GetRelativePath(string path) {
		var relativePath = Path.GetRelativePath(_rootDirectory, path);
		Console.WriteLine($"Root: {_rootDirectory} \n Path: {path} \n Relative: {relativePath}");
		return relativePath;
	}
	public async Task<WindingCode> GetWindingCodeDocuments(WindingCode code) {
		if (code.FolderPath == null) { return code; }
		var documents = code.Media;

		// get the pdf path
		try {
			var pdfPath = await GetPdfPath(code.FolderPath, false);
			if (pdfPath != null) { documents.Pdf = GetRelativePath(pdfPath); }
		}
		catch (Exception e) {
			Console.WriteLine($"TryPdfPath: {e.Message}");
			throw;
		}

		// get the video path
		try {
			var videoPath = await GetVideoPath(code.FolderPath);
			if (videoPath != null) { documents.Video = GetRelativePath(videoPath); }
		}
		catch (Exception e) {
			Console.WriteLine($"TryVideoPath: {e.Message}");
			throw;
		}

		// get the refMedia path
		try {
			var refMediaPath = await GetRefMediaPath(code.FolderPath);
			if (refMediaPath != null) { documents.ReferenceFolder = GetRelativePath(refMediaPath); }
		}
		catch (Exception e) {
			Console.WriteLine($"TryRefMediaPath: {e.Message}");
			throw;
		}
		return code;
	}
	private Task<string?> GetPdfPath(string folder, bool relative) {
		var pdfPath = Directory.EnumerateFiles(folder).FirstOrDefault(f => f.EndsWith(".pdf"));
		if (relative && pdfPath != null) { pdfPath = Path.GetRelativePath(_rootDirectory, pdfPath); }
		return Task.FromResult(pdfPath);
	}
	private static Task<string?> GetVideoPath(string? folder) {
		if (folder == null) { return Task.FromResult<string?>(null); }
		var platformVideoFolder = Path.Combine(AppConfig.BasePath, "TrainingVideos", "Unsorted");
		var videos = Directory.EnumerateFiles(platformVideoFolder).Where(f => f.EndsWith(".mp4")).ToArray();
		var random = new Random();
		for (var i = videos.Length - 1; i > 0; i--) {
			var j = random.Next(i + 1);
			(videos[i], videos[j]) = (videos[j], videos[i]);
		}
		var randomNumber = random.Next(videos.Length);
		var videoPath = videos[randomNumber];
		return Task.FromResult(videoPath)!;
	}
	private static Task<string?> GetRefMediaPath(string folder) {
		var refMediaPath = Directory.EnumerateDirectories(folder).FirstOrDefault(f => f.Contains("Ref", StringComparison.OrdinalIgnoreCase));
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
		return Task.FromResult(folders.ToArray());
	}
	public async Task ExportWindingCodesToJson(IEnumerable<WindingCode> windingCodes, bool syncDatabase) {
		var windingCodesList = windingCodes.ToList();
		var json = JsonSerializer.Serialize(windingCodesList, new JsonSerializerOptions {
			WriteIndented = true,
			Converters = {
				new JsonStringEnumConverter()
			}
		});
		Console.WriteLine("_windingCodesJsonPath: " + _windingCodesJsonPath);
		try {
			// if the file exists, delete it
			if (File.Exists(_windingCodesJsonPath)) { File.Delete(_windingCodesJsonPath); }
			await File.WriteAllTextAsync(_windingCodesJsonPath, json);
		}
		catch (Exception ex) {
			_logger.LogError("An error occurred while exporting the database : {Error}", ex);
			throw;
		}
		if (syncDatabase) { await UpdateDatabaseWindingCodes(windingCodesList); }
	}
	public async Task<IEnumerable<WindingCode>> GetWindingCodesJson(string? path = null) {
		try {
			var filePath = path ?? _windingCodesJsonPath;
			var json = await File.ReadAllTextAsync(filePath);
			var jObject = JsonDocument.Parse(json).RootElement.GetProperty("WindingCodes");
			return JsonSerializer.Deserialize<IEnumerable<WindingCode>>(jObject.GetRawText()) ?? Array.Empty<WindingCode>();
		}
		catch (Exception ex) {
			_logger.LogError("An error occurred while getting the json file : {Error}", ex);
			throw;
		}
	}
	public async Task UpdateDatabaseWindingCodes(IEnumerable<WindingCode> windingCodes) {
		var dbWindingCodes = await _dataContext.WindingCodes.ToListAsync();
		var jsonWindingCodes = windingCodes.ToList();
		foreach (var jsonWindingCode in jsonWindingCodes) {
			// if the code is not in the database, add it
			if (dbWindingCodes.All(dbWindingCode => dbWindingCode.Code != jsonWindingCode.Code)) { _dataContext.WindingCodes.Add(jsonWindingCode); }
			else {
				// if the code is in the database, update it
				var dbWindingCode = dbWindingCodes.First(dbCode => dbCode.Code == jsonWindingCode.Code);
				dbWindingCode.Code = jsonWindingCode.Code;
				dbWindingCode.Name = jsonWindingCode.Name;
				dbWindingCode.FolderPath = jsonWindingCode.FolderPath;
				dbWindingCode.CodeType = jsonWindingCode.CodeType;
				dbWindingCode.CodeTypeId = jsonWindingCode.CodeTypeId;
				_dataContext.WindingCodes.Update(dbWindingCode);
			}
		}

		// if the code is in the database but not in the json file, delete it
		foreach (var dbWindingCode in dbWindingCodes.Where(dbWindingCode => jsonWindingCodes.All(jsonWindingCode => jsonWindingCode.Code != dbWindingCode.Code))) { _dataContext.WindingCodes.Remove(dbWindingCode); }
		await _dataContext.SaveChangesAsync();
	}
	#endregion
}
