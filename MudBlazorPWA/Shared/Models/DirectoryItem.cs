using MudBlazor;
using MudBlazorPWA.Shared.Interfaces;
namespace MudBlazorPWA.Shared.Models;
public class DirectoryItem<T> : IDirectoryItem where T : class {
	public T Item { get; set; }
	public string Name { get; }
	public string Path { get; }
	public string Icon { get; }
	public string DropZoneId {
		get {
			if (string.IsNullOrEmpty(_dropZoneId))
				_dropZoneId = GenerateUniqueId();
			return _dropZoneId;
		}
		set => _dropZoneId = value;
	}

	private string _dropZoneId = string.Empty;
	private string GenerateUniqueId() {
		switch (Item) {
			case FileNode file: {
				string parentFolderPath = GetParentFolderPath(file.Path);
				return DirectoryItemHelper.DirectoryIds.TryGetValue(parentFolderPath, out var id)
					? id.ToString()
					: string.Empty;// Fallback if the parent directory hasn't been added to the dictionary
			}
			case DirectoryNode directory: {
				int id = DirectoryItemHelper.DirectoryCounter++;
				DirectoryItemHelper.DirectoryIds[directory.Path] = id;
				return id.ToString();
			}
			default: return string.Empty;
		}
	}

	private static string GetParentFolderPath(string path) {
		int lastSeparatorIndex = path.LastIndexOf('/');
		if (lastSeparatorIndex == -1)
			lastSeparatorIndex = path.LastIndexOf('\\');

		return lastSeparatorIndex != -1
			? path[..lastSeparatorIndex]
			: string.Empty;
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
	}
	private static bool ShouldExpand(DirectoryNode directory) {
		return directory.Folders.Any() || directory.Files.Any();
	}
	private static string FileIcon(FileNode file) {
		return file.Name.EndsWith(".pdf")
			? Icons.Custom.FileFormats.FilePdf
			: Icons.Custom.FileFormats.FileVideo;
	}
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

public static class DirectoryItemHelper {
	public static bool HasChildren(this DirectoryNode item) {
		return item.Folders.Any() || item.Files.Any();
	}
	public static int DirectoryCounter { get; set; } = 1;
	public static Dictionary<string, int> DirectoryIds { get; set; } = new();
}
