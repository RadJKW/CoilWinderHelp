using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MudBlazorPWA.Shared.Services;
public interface IDirectoryService
{
	Task<(string, string[], string[])> GetFolderContent(string? path = null);
	Task<string> GetRelativePath(string path);
	Task<string[]> GetFoldersInPath(string? path = null);

	Task ExportWindingCodesToJson(IEnumerable<WindingCode> windingCodes, bool syncDatabase);
	Task<IEnumerable<WindingCode>> GetWindingCodesJson(string? path = null);

}

public class DirectoryServiceOptions
{
	public string RootDirectoryPath { get; set; } = string.Empty;
}

public class DirectoryService : IDirectoryService
{
	private string WindingCodesJsonPath { get; set; }

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<DirectoryService> _logger;

	// add the IDataContext to the constructor
	private readonly DataContext _dataContext;
	private readonly string _rootDirectory;


	//TODO: Create an Settings class to hold this and other settings/data
	private readonly string[] _allowedExtensions = {
		".mp4", ".pdf", ".webm"
	};

	public DirectoryService(IOptions<DirectoryServiceOptions> options, ILogger<DirectoryService> logger, DataContext dataContext) {
		_logger = logger;
		_dataContext = dataContext;
		_rootDirectory = options.Value.RootDirectoryPath;
		WindingCodesJsonPath = _rootDirectory + "WindingCodes.json";
	}

	// Used by the hub to get the contents of the root
	public Task<(string, string[], string[])> GetFolderContent(string? path = null) {
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

	public Task<string> GetRelativePath(string path) {
		return
			Task.FromResult(Path.GetRelativePath(_rootDirectory, path));
	}

	public Task<string[]> GetFoldersInPath(string? path = null) {
		Console.WriteLine("GettingFoldersInPath");
		// return a list of all folders starting from the root directory
		path ??= _rootDirectory;
		var folders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
		Console.WriteLine(folders);
		return Task.FromResult(folders);
	}

	public async Task ExportWindingCodesToJson(IEnumerable<WindingCode> windingCodes, bool syncDatabase) {
		var json = JsonConvert.SerializeObject(windingCodes, Formatting.Indented);
		try {
			await File.WriteAllTextAsync(WindingCodesJsonPath, json);
		}
		catch (Exception ex) {
			_logger.LogError("An error occurred while exporting the database : {Error}", ex);
			throw;
		}
		if (syncDatabase) {
			await UpdateDatabaseWindingCodes(windingCodes);
		}
	}

	// function to return the json file to DataContextInitializer so it can compare the json file to the database
	public async Task<IEnumerable<WindingCode>> GetWindingCodesJson(string? path = null) {
		try {
			var filePath = path ?? WindingCodesJsonPath;
			var json = await File.ReadAllTextAsync(filePath);
			return JsonConvert
				       .DeserializeObject<IEnumerable<WindingCode>>(JObject
					       .Parse(json)["WindingCodes"]?
					       .ToString()!)
			       ?? Array.Empty<WindingCode>();
		}
		catch (Exception ex) {
			_logger.LogError("An error occurred while getting the json file : {Error}", ex);
			throw;
		}
	}

	private async Task UpdateDatabaseWindingCodes(IEnumerable<WindingCode> windingCodes) {
		var dbWindingCodes = await _dataContext.WindingCodes.ToListAsync();

		var jsonWindingCodes = windingCodes.ToList();
		foreach (var jsonWindingCode in jsonWindingCodes) {
			// if the code is not in the database, add it
			if (dbWindingCodes.All(dbWindingCode => dbWindingCode.Code != jsonWindingCode.Code)) {
				_dataContext.WindingCodes.Add(jsonWindingCode);
			}
			else {
				// if the code is in the database, update it
				var dbWindingCode = dbWindingCodes.First(dbCode => dbCode.Code == jsonWindingCode.Code);
				dbWindingCode.Code = jsonWindingCode.Code;
				dbWindingCode.Name = jsonWindingCode.Name;
				dbWindingCode.CodeType = jsonWindingCode.CodeType;
				dbWindingCode.CodeTypeId = jsonWindingCode.CodeTypeId;
				_dataContext.WindingCodes.Update(dbWindingCode);
			}

		}

		// if the code is in the database but not in the json file, delete it
		foreach (var dbWindingCode in dbWindingCodes.Where(dbWindingCode => jsonWindingCodes.All(jsonWindingCode => jsonWindingCode.Code != dbWindingCode.Code))) {
			_dataContext.WindingCodes.Remove(dbWindingCode);
		}

		await _dataContext.SaveChangesAsync();






	}

}
