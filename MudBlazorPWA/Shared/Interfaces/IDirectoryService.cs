using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Interfaces;
public interface IDirectoryService
{
	Task<(string, string[], string[])> GetFolderContent(string? path = null);
	Task<string[]> GetFoldersInPath(string? path = null);
	Task<List<string>> ListPdfFiles(string? path = null);
	Task<IWindingCode> GetWindingCodeDocuments(IWindingCode code);
	Task<List<string>> ListVideoFiles(string? path = null);
	public string GetRelativePath(string fullPath);
}
