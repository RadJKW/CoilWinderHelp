using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Shared.Interfaces;
public interface IDirectoryItem {
	string Name { get; }
	string Path { get; }
	string Icon { get; }
	string DropZoneId { get; set; }
	bool CanExpand { get; }
	bool Expanded { get; set; }
	bool Loading { get; set; }
	bool Selected { get; set; }
	bool IsCopy { get; init; }
	string OriginalIdentifier { get; set; }
	DropItemType Type { get; }
	HashSet<IDirectoryItem> TreeItems { get; set; }
	Task FetchTreeItems();
	IEnumerable<IDirectoryItem> GetFiles();
	bool HasFiles();
	HashSet<IDirectoryItem> GetFolders();
}
