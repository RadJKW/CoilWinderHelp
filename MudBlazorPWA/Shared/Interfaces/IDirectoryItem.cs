using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Shared.Interfaces;
public interface IDirectoryItem {
	// Make the Type of DirectoryItem<T> accessible
	// { get; }
	ItemType ItemType { get; }
	string DropZoneId { get; set; }
	string Name { get; }
	string Path { get; }
	bool CanExpand { get; }
	bool Expanded { get; set; }
	bool Loading { get; set; }
	bool Selected { get; set; }
	HashSet<IDirectoryItem> TreeItems { get; set; }
	Task FetchTreeItems();
	IEnumerable<IDirectoryItem> GetFiles();
	bool HasFiles();
	IEnumerable<DropItem> GetFileDropItems();
}
