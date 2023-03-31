using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Data;
public class DataContextInitializer
{
	private readonly ILogger<DataContextInitializer> _logger;
	private readonly DataContext _dbContext;

	public DataContextInitializer(ILogger<DataContextInitializer> logger, DataContext dbContext) {
		_logger = logger;
		_dbContext = dbContext;
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

	public async Task SeedDataAsync(bool removeRecords = false, string? jsonFilePath = null)
	{
		try
		{
			await TrySeedAsync(removeRecords, jsonFilePath);
		}
		catch (Exception ex)
		{
			_logger.LogError("An error occurred while seeding the database : {Error}", ex);
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
	private async Task TrySeedAsync(bool removeRecords, string? jsonFilePath = null) {
		// check to see if z80 winding codes table has any data
		List<Task> seedTasks = new();
		bool z80WindingCodesHasData = await  CheckDbHasDataAsync(_dbContext.Z80WindingCodes);
		bool pcWindingCodesHasData = await CheckDbHasDataAsync(_dbContext.PcWindingCodes);

		switch (z80WindingCodesHasData) {
			case true when !removeRecords:
				_logger.LogInformation("Seeding the database was skipped because it already has data");
				return;
			// if the database has data and we do want to removeRecords it, then delete all the data
			case true when removeRecords:
				_dbContext.Z80WindingCodes.RemoveRange(_dbContext.Z80WindingCodes);
				await _dbContext.SaveChangesAsync();
				_logger.LogInformation("Removed all records from the database");
				break;
		}

		// Seed the database with data from the JSON file
		 seedTasks.Add(SeedZ80WindingCodesAsync(jsonFilePath));

		switch (pcWindingCodesHasData) {
			case true when !removeRecords:
				_logger.LogInformation("Seeding the database was skipped because it already has data");
				return;
			// if the database has data and we do want to removeRecords it, then delete all the data
			case true when removeRecords:
				_dbContext.PcWindingCodes.RemoveRange(_dbContext.PcWindingCodes);
				await _dbContext.SaveChangesAsync();
				_logger.LogInformation("Removed all records from the database");
				break;
		}

		seedTasks.Add(SeedPcWindingCodesAsync(jsonFilePath));
		await Task.WhenAll(seedTasks);
	}

	private static async Task<bool> CheckDbHasDataAsync<T>(IQueryable<T> dbSet) where T : IWindingCode {
		return await dbSet.AnyAsync();
	}

	private async Task SeedZ80WindingCodesAsync(string? jsonFilePath = null) {
		if (string.IsNullOrWhiteSpace(jsonFilePath)) {
			jsonFilePath = AppConfig.JsonDataSeedFile;
		}

		if (!File.Exists(jsonFilePath)) {
			_logger.LogError("Could not find JSON file at {Path}", jsonFilePath);
			return;
		}

		string json = await File.ReadAllTextAsync(jsonFilePath);
		JsonElement rootElement = JsonDocument.Parse(json).RootElement;

		if (rootElement.TryGetProperty("Z80WindingCodes", out JsonElement z80WindingCodesElement)) {
			var z80WindingCodes = JsonSerializer.Deserialize<List<Z80WindingCode>>(z80WindingCodesElement.GetRawText());
			if (z80WindingCodes != null) {
				foreach (Z80WindingCode? windingCode in z80WindingCodes) {
					Console.WriteLine("Z80WindingCode" + windingCode.Name);
				}
				await _dbContext.Z80WindingCodes.AddRangeAsync(z80WindingCodes);
				await _dbContext.SaveChangesAsync();
				_logger.LogInformation("Added {Count} Z80 winding codes to the database", z80WindingCodes.Count);
			}
		}
	}

	private async Task SeedPcWindingCodesAsync(string? jsonFilePath = null) {
		if (string.IsNullOrWhiteSpace(jsonFilePath)) {
			jsonFilePath = AppConfig.JsonDataSeedFile;
		}

		if (!File.Exists(jsonFilePath)) {
			_logger.LogError("Could not find JSON file at {Path}", jsonFilePath);
			return;
		}

		string json = await File.ReadAllTextAsync(jsonFilePath);
		JsonElement rootElement = JsonDocument.Parse(json).RootElement;

		if (rootElement.TryGetProperty("PcWindingCodes", out JsonElement pcWindingCodesElement)) {
			var pcWindingCodes = JsonSerializer.Deserialize<List<PcWindingCode>>(pcWindingCodesElement.GetRawText());
			if (pcWindingCodes != null) {
				foreach (PcWindingCode? windingCode in pcWindingCodes) {
					Console.WriteLine("PcWindingCode" + windingCode.Name);
				}
				await _dbContext.PcWindingCodes.AddRangeAsync(pcWindingCodes);
				await _dbContext.SaveChangesAsync();
				_logger.LogInformation("Added {Count} PC winding codes to the database", pcWindingCodes.Count);
			}

		}
	}
}
