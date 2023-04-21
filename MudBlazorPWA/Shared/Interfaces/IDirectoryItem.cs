namespace MudBlazorPWA.Shared.Interfaces;
public interface IDirectoryItem {
	string Name { get; }
	string Path { get; }
	string Icon { get; }
	string DropZoneId { get; set; }
	bool CanExpand { get; }
	bool Expanded { get; set; }
	bool Loading { get; set; }
	HashSet<IDirectoryItem> TreeItems { get; set; }
	Task FetchTreeItems();
}
