using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Shared.Interfaces;
public interface IDirectoryItem {
	ItemType ItemType { get; }
	string Icon { get; }
	string DropZoneId { get; set; }
	string Name { get; }
	string Path { get; }
	bool CanExpand { get; }
	bool Expanded { get; set; }
	bool Loading { get; set; }
	bool Selected { get; set; }


	HashSet<IDirectoryItem> TreeItems { get; set; }
	HashSet<IDirectoryItem> BuildTreeItems();
	IEnumerable<IDirectoryItem> GetFiles();
	IEnumerable<IDirectoryItem> GetFolders();
}
