using MudBlazorPWA.Client.ViewModels;

namespace MudBlazorPWA.Client.Services;

// functionality for this folder service
// Properties:
// - get the root folder ( the folder that contains the CoilWinderHelp Docs)
// - keep track of the current folder
// - keep track of the breadcrumbs ( history of folders the user has navigated to )
// - provide the subfolders (used to show the user the folders they can navigate to)
// Events:
// - when the current folder changes, the subfolders and breadcrumbs should be updated
//

public class FolderService
{
	public Folder RootFolder { get; set; } = default!;
	public Folder CurrentFolder { get; set; } = default!;
	public List<Folder> Breadcrumbs { get; set; } = new();
	public List<Folder> SubFolders { get; set; } = new();

}
