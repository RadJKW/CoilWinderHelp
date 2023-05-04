using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Shared.Interfaces;
public interface IDirectoryItem {
	string Name { get; }
	string Path { get; }
	string? Icon { get; set; }
	ItemType ItemType { get; }
	bool CanExpand { get; }
	bool Expanded { get; set; }
	bool Loading { get; set; }
	bool Selected { get; set; }
	bool HasFolders { get; }
	bool HasFiles { get; }
	DirectoryNode? GetFolder();
	HashSet<IDirectoryItem>? TreeItems { get; }
}
