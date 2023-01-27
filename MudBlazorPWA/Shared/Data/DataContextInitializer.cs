using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Models;
using Newtonsoft.Json;

namespace MudBlazorPWA.Shared.Data;
public class DataContextInitializer
{
    private readonly ILogger<DataContextInitializer> _logger;
    private readonly DataContext _dbContext;

    public DataContextInitializer(ILogger<DataContextInitializer> logger, DataContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
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

            object[] windingCodes = {
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

                }
            };

            _dbContext.AddRange(windingCodes);
            await _dbContext.SaveChangesAsync();

        }
        await ExportWindingCodesToJson();
    }

    private async Task ExportWindingCodesToJson()
    {
        var windingCodes = await _dbContext.WindingCodes.ToListAsync();

        var json = JsonConvert.SerializeObject(windingCodes, Formatting.Indented);
        try
        {
            await File.WriteAllTextAsync("B:/CoilWinderTraining-Edit/WindingStops.json", json);

        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while exporting the database : {Error}", ex);
            throw;
        }
    }



}
