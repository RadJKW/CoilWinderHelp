using MudBlazorPWA.Shared.Interfaces;
namespace MudBlazorPWA.Shared.Models;
public class DirectoryItem<T> : IDirectoryItem where T : class {
	public T Item { get; set; }

	public ItemType ItemType => Item switch {
		FileNode => ItemType.File,
		DirectoryNode => ItemType.Directory,
		_ => throw new ArgumentOutOfRangeException(nameof(Item))
	};
	public string DropZoneId {
		get {
			if (string.IsNullOrEmpty(_dropZoneId))
				_dropZoneId = GenerateUniqueId();
			return _dropZoneId;
		}
		set => _dropZoneId = value;
	}
	private string _dropZoneId = string.Empty;

	public string Name => Item switch {
		FileNode file => file.Name,
		DirectoryNode directory => directory.Name,
		_ => string.Empty
	};

	public string Path => Item switch {
		FileNode file => file.Path,
		DirectoryNode directory => directory.Path,
		_ => string.Empty
	};
	public bool CanExpand { get; set; }

	private static bool ShouldExpand(DirectoryNode directory) {
		return directory.Folders.Any() || directory.Files.Any();
	}
	public bool Expanded { get; set; }
	public bool Loading { get; set; }

	public bool Selected { get; set; }
	public HashSet<IDirectoryItem> TreeItems { get; set; }
	public DirectoryItem(T item) {
		Item = item;
		CanExpand = item switch {
			DirectoryNode directory => ShouldExpand(directory),
			_ => false
		};
		TreeItems = item switch {
			DirectoryNode directory => BuildTreeItems(directory),
			_ => new()
		};
	}
	private static HashSet<IDirectoryItem> BuildTreeItems(DirectoryNode item) {
		var treeItems = new HashSet<IDirectoryItem>();
		foreach (var folder in item.Folders) {
			treeItems.Add(new DirectoryItem<DirectoryNode>(folder));
		}
		foreach (var file in item.Files) {
			treeItems.Add(new DirectoryItem<FileNode>(file));
		}
		return treeItems;
	}

	public IEnumerable<DropItem> GetFileDropItems() {
		var dropItems = new List<DropItem>();
		dropItems.AddRange(GetFiles().Select(x => new DropItem(x)));
		return dropItems;
	}

	public IEnumerable<IDirectoryItem> GetFiles() {
		return TreeItems.Where(x => x.ItemType == ItemType.File);
	}

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
	public bool HasFiles() {
		// iterate over the TreeItems if any and check if any are of the Type DirectoryItem<FileNode>
		var hasFiles = TreeItems.Any(item => item is DirectoryItem<FileNode>);
		return hasFiles;
	}

	private static string GetParentFolderPath(string path) {
		int lastSeparatorIndex = path.LastIndexOf('/');
		if (lastSeparatorIndex == -1)
			lastSeparatorIndex = path.LastIndexOf('\\');

		return lastSeparatorIndex != -1
			? path[..lastSeparatorIndex]
			: string.Empty;
	}

	public bool HasChildFiles() {
		return TreeItems.Any(item => item is DirectoryItem<FileNode>);
	}

	public bool HasChildFolders() {
		return TreeItems.Any(item => item is DirectoryItem<DirectoryNode>);
	}
}

public enum ItemType {
	File,
	Directory
}

public static class DirectoryItemHelper {
	public static bool HasChildren(this DirectoryNode item) {
		return item.Folders.Any() || item.Files.Any();
	}
	public static int DirectoryCounter { get; set; } = 1;
	public static Dictionary<string, int> DirectoryIds { get; set; } = new();
}
