using MudBlazor;
using MudBlazorPWA.Shared.Interfaces;
namespace MudBlazorPWA.Shared.Models;
public class DirectoryItem<T> : IDirectoryItem where T : class {
	public T Item { get; set; }
	public string Name { get; }
	public string Path { get; }
	public string Icon { get; }
	public string DropZoneId => GenerateUniqueId();
	private string GenerateUniqueId() {
		return Item switch {
			FileNode file => file.Path.Replace(file.Name, string.Empty).GetHashCode().ToString(),
			DirectoryNode directory => directory.Path.GetHashCode().ToString(),
			_ => string.Empty
		};
	}
	public bool CanExpand { get; }
	public bool Expanded { get; set; }
	public bool Loading { get; set; }

	public HashSet<IDirectoryItem> TreeItems { get; set; } = new();
	public DirectoryItem(T item) {
		Item = item;
		(Name, Path, Icon, CanExpand) = item switch {
			DirectoryNode directory
				=> (directory.Name, directory.Path, Icons.Material.Filled.Folder, ShouldExpand(directory)),
			FileNode file
				=> (file.Name, file.Path, FileIcon(file), false),
			_
				=> ("", "", "", false)
		};
		// if the item is a directory, call method to convert
	}
	private static bool ShouldExpand(DirectoryNode directory) => directory.Folders.Any() || directory.Files.Any();
	private static string FileIcon(FileNode file) => file.Name.EndsWith(".pdf")
		? Icons.Custom.FileFormats.FilePdf
		: Icons.Custom.FileFormats.FileVideo;

	public async Task FetchTreeItems() {
		if (!Expanded || TreeItems.Any()) {
			Console.WriteLine("Not expanded or already loaded");
			return;
		}

		await Task.Run(
		() => {
			if (Item is not DirectoryNode directory) return;

			foreach (var item in directory.Folders.Select(
			         folder
				         => new DirectoryItem<DirectoryNode>(folder))) { TreeItems.Add(item); }

			foreach (var item in directory.Files.Select(
			         file
				         => new DirectoryItem<FileNode>(file))) { TreeItems.Add(item); }
		});

		Console.WriteLine("TreeItems Loaded");
	}
}
