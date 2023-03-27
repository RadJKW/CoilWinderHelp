using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Interfaces;
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
