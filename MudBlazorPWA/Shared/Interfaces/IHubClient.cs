using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Shared.Interfaces;
public interface IHubClient {
	Task ReceiveFolderContent(string currentPath, string[] files, string[] folders);
	Task FileSelected(string relativePath);

	Task ReceiveAllFolders(string[] folders);
	Task WindingCodesDbUpdated();

	Task CurrentWindingStopUpdated(IWindingCode code);
}
