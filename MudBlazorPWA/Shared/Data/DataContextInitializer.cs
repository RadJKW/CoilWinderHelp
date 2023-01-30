using System.Runtime.InteropServices;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Models;
using MudBlazorPWA.Shared.Services;
using Newtonsoft.Json;

namespace MudBlazorPWA.Shared.Data;
public class DataContextInitializer
{
    private static string ApiFolder =>
        RuntimeInformation
            .IsOSPlatform(OSPlatform.Windows)
            ? @"B:/CoilWinderTraining-Edit"
            : @"/Users/jkw/WindingPractices";
    private readonly ILogger<DataContextInitializer> _logger;
    private readonly DataContext _dbContext;
    private readonly IDirectoryService _directoryService;

    public DataContextInitializer(ILogger<DataContextInitializer> logger, DataContext dbContext, IDirectoryService directoryService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _directoryService = directoryService;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_dbContext.Database.IsSqlServer())
            {
                await _dbContext.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while initialising the database : {Error}", ex);
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while seeding the database : {Error}", ex);
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // seed default data if necessary
        if (!_dbContext.WindingCodes.Any())
        {

            object[] windingCodes =
            {
                new WindingCode
                {
                    Code = "AD",
                    Name = "Annular Duct",
                    Type = CodeType.Stop,
                    FolderPath = null
                },
                new WindingCode
                {
                    Code = "CX",
                    Name = "Crepe Paper",
                    Type = CodeType.Stop,
                    FolderPath = null
                },
                new WindingCode
                {
                    Code = "DS",
                    Name = "Duct Stop",
                    Type = CodeType.Stop,
                    FolderPath = null
                },
                new WindingCode
                {
                    Code = "LE",
                    Name = "Layer End",
                    Type = CodeType.Stop,
                    FolderPath = null
                },
                new WindingCode
                {
                    Code = "PX",
                    Name = "Paper Stop",
                    Type = CodeType.Stop,
                    FolderPath = null
                },
                new WindingCode
                {
                    Code = "TB",
                    Name = "Section Break",
                    Type = CodeType.Stop,
                    FolderPath = null
                },
                new WindingCode
                {
                    Code = "TS",
                    Name = "Tab Stop",
                    Type = CodeType.Stop,
                    FolderPath = null
                },
                new WindingCode
                {
                    Code = "XP",
                    Name = "Extra Paper",
                    Type = CodeType.Stop,
                    FolderPath = null

                },
                new WindingCode
                {
                    Code = "CI",
                    Name = "Coil Information",
                    Type = CodeType.Data,
                    FolderPath = null
                },

            };

            _dbContext.AddRange(windingCodes);
            await _dbContext.SaveChangesAsync();

            await _directoryService.ExportWindingCodesToJson(windingCodes);
        }
    }

    private async Task ExportWindingCodesToJson()
    {
        var windingCodes = await _dbContext.WindingCodes.ToListAsync();

        var json = JsonConvert.SerializeObject(windingCodes, Formatting.Indented);
        try
        {
            await File.WriteAllTextAsync($"{ApiFolder}/WindingStops.json", json);

        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while exporting the database : {Error}", ex);
            throw;
        }

    }



}
