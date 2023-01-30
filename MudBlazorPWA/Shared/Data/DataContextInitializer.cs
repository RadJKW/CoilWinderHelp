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

	public async Task SeedAsync() {
		try {
			await TrySeedAsync();
		}
		catch (Exception ex) {
			_logger.LogError("An error occurred while seeding the database : {Error}", ex);
			throw;
		}
	}

	private async Task TrySeedAsync() {
		// seed default data if necessary
		if (!_dbContext.WindingCodes.Any()) {
			object[] windingCodes = {
				new WindingCode {
					Code = "AD",
					Name = "Annular Duct",
					Type = CodeType.Stop,
					FolderPath = null
				},
				new WindingCode {
					Code = "CX",
					Name = "Crepe Paper",
					Type = CodeType.Stop,
					FolderPath = null
				},
				new WindingCode {
					Code = "DS",
					Name = "Duct Stop",
					Type = CodeType.Stop,
					FolderPath = null
				},
				new WindingCode {
					Code = "LE",
					Name = "Layer End",
					Type = CodeType.Stop,
					FolderPath = null
				},
				new WindingCode {
					Code = "PX",
					Name = "Paper Stop",
					Type = CodeType.Stop,
					FolderPath = null
				},
				new WindingCode {
					Code = "TB",
					Name = "Section Break",
					Type = CodeType.Stop,
					FolderPath = null
				},
				new WindingCode {
					Code = "TS",
					Name = "Tab Stop",
					Type = CodeType.Stop,
					FolderPath = null
				},
				new WindingCode {
					Code = "XP",
					Name = "Extra Paper",
					Type = CodeType.Stop,
					FolderPath = null
				},
				new WindingCode {
					Code = "CI",
					Name = "Coil Information",
					Type = CodeType.Data,
					FolderPath = null
				},
				new WindingCode {
					Code = "NC",
					Name = "New Coil",
					Type = CodeType.Data,
					FolderPath = null
				},
				new WindingCode {
					Code = "NW",
					Name = "New Winding",
					Type = CodeType.Data,
					FolderPath = null
				},
				new WindingCode {
					Code = "RS",
					Name = "Run Screen",
					Type = CodeType.Data,
					FolderPath = null
				},
				new WindingCode {
					Code = "DE",
					Name = "Data Entry",
					Type = CodeType.Data,
					FolderPath = null
				},
				new WindingCode {
					Code = "FC",
					Name = "Finish Coil",
					Type = CodeType.Data,
					FolderPath = null
				},
				new WindingCode {
					Code = "PA",
					Name = "Paper",
					Type = CodeType.Layer,
					FolderPath = null
				},
				new WindingCode {
					Code = "LV",
					Name = "Low Voltage",
					Type = CodeType.Layer,
					FolderPath = null
				},
				new WindingCode {
					Code = "HV",
					Name = "High Voltage",
					Type = CodeType.Layer,
					FolderPath = null
				},
				new WindingCode {
					Code = "HVA",
					Name = "High Voltage Aluminum",
					Type = CodeType.Layer,
					FolderPath = null
				},
				new WindingCode {
					Code = "HVC",
					Name = "High Voltage Copper",
					Type = CodeType.Layer,
					FolderPath = null
				},
				new WindingCode {
					Code = "PM",
					Name = "Paper Material",
					Type = CodeType.Material,
					FolderPath = null
				},
				new WindingCode {
					Code = "WC",
					Name = "Wire Conductor",
					Type = CodeType.Material,
					FolderPath = null
				},
				new WindingCode {
					Code = "SC",
					Name = "Sheet Conductor",
					Type = CodeType.Material,
					FolderPath = null
				},
			};
			_dbContext.AddRange(windingCodes);
			await _dbContext.SaveChangesAsync();

			await _directoryService.ExportWindingCodesToJson(windingCodes);
		}
	}
}
