using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Models;
using MudBlazorPWA.Shared.Services;

namespace MudBlazorPWA.Shared.Data;
public class DataContextInitializer
{
	private readonly ILogger<DataContextInitializer> _logger;
	private readonly DataContext _dbContext;
	private readonly IDirectoryService _directoryService;

	public DataContextInitializer(ILogger<DataContextInitializer> logger, DataContext dbContext, IDirectoryService directoryService) {
		_logger = logger;
		_dbContext = dbContext;
		_directoryService = directoryService;
	}

	public async Task InitialiseAsync() {
		try {
			if (_dbContext.Database.IsSqlServer()) {
				await _dbContext.Database.MigrateAsync();
			}
		}
		catch (Exception ex) {
			_logger.LogError("An error occurred while initialising the database : {Error}", ex);
			throw;
		}
	}

	/// <summary>
	/// Seed the database with data from a JSON file.
	/// ** No method for ADDING data, can only clear and re-seed **
	/// </summary>
	/// <param name="removeRecords">
	/// Erases ALL records in the database before seeding. **CAUTION**
	/// </param>
	/// <param name="jsonFilePath">
	/// Full (string) path to 'File.Json' for seeding the database.
	/// </param>
	public async Task SeedDataAsync(bool removeRecords = false, string? jsonFilePath = null) {
		try {
			await TrySeedAsync(removeRecords, jsonFilePath);
		}
		catch (Exception ex) {
			_logger.LogError("An error occurred while seeding the database : {Error}", ex);
			throw;
		}
	}

	private async Task TrySeedAsync(bool removeRecords, string? jsonFilePath = null) {
		var dbHasData = await _dbContext.WindingCodes.AnyAsync();

		// create a switch for the different possible states using the combination of
		// the removeRecords flag, and whether the database has data or not

		switch (dbHasData) {
			// if the database has data and we don't want to removeRecords it, then do nothing
			case true when !removeRecords:
				_logger.LogInformation("Seeding the database was skipped because it already has data");
				return;
			// if the database has data and we do want to removeRecords it, then delete all the data
			case true when removeRecords:
				_dbContext.WindingCodes.RemoveRange(_dbContext.WindingCodes);
				await _dbContext.SaveChangesAsync();
				_logger.LogInformation("Removed all records from the database");
				break;
		}

		await ImportDataFromJson(jsonFilePath);
	}

	/*private async Task ExportDataToJson() {
		var windingCodes = await _dbContext.WindingCodes.ToListAsync();
		await _directoryService.ExportWindingCodesToJson(windingCodes, false);
	}*/

	private async Task ImportDataFromJson(string? jsonFilePath = null) {
		if (string.IsNullOrWhiteSpace(jsonFilePath)) {
			jsonFilePath = AppConfig.JsonDataSeedFile;
		}

		if (!File.Exists(jsonFilePath)) {
			_logger.LogError("Could not find JSON file at {Path}", jsonFilePath);
			return;
		}

		var json = await File.ReadAllTextAsync(jsonFilePath);
		var rootElement = JsonDocument.Parse(json).RootElement;
		var options = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		if (rootElement.TryGetProperty("WindingCodes", out var d1WindingCodesElement)) {
			var d1WindingCodes = JsonSerializer.Deserialize<List<WindingCode>>(d1WindingCodesElement.GetRawText());
			if (d1WindingCodes != null) {
				foreach (var windingCode in d1WindingCodes) {
					Console.WriteLine("WindingCode" + windingCode.Name);
				}
				await _dbContext.WindingCodes.AddRangeAsync(d1WindingCodes);
				await _dbContext.SaveChangesAsync();
				_logger.LogInformation("Added {Count} D1 winding codes to the database", d1WindingCodes.Count);
			}
		}
	}
}
