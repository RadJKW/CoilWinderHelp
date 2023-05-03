using MudBlazor;
using MudBlazorPWA.Shared.Interfaces;
namespace MudBlazorPWA.Shared.Models;
public class DirectoryItem<T> : IDirectoryItem where T : class {
	public T Item { get; init; }

	private readonly Lazy<ItemType> _type;
	private readonly Lazy<string> _icon;
	private readonly Lazy<bool> _canExpand;

	private readonly Dictionary<ItemType, string> _iconTypeMap = new() {
		{ ItemType.FilePdf, Icons.Custom.FileFormats.FilePdf },
		{ ItemType.FileVideo, Icons.Custom.FileFormats.FileVideo },
		{ ItemType.Directory, Icons.Material.Filled.Folder },
		{ ItemType.File, Icons.Custom.FileFormats.FileDocument },
		{ ItemType.Unknown, Icons.Material.Filled.DataObject },
	};

	private readonly Dictionary<ItemType, string[]> _extensionTypeMap = new() {
		{ ItemType.FilePdf, new[] { ".pdf" } },
		{ ItemType.FileVideo, new[] { ".mp4", ".mkv" } },
	};

	public DirectoryItem(T item) {
		Item = item;
		_type = new(GetItemType);
		_icon = new(GetIcon);
		_canExpand = new(ShouldExpand);
		TreeItems = new();
	}

	public string Icon => _icon.Value;
	public ItemType ItemType => _type.Value;

	private ItemType GetItemType() {
		string extension = GetExtension(Name).ToLowerInvariant();

		var foundType = _extensionTypeMap.FirstOrDefault(
			entry =>
				entry.Value.Contains(extension, StringComparer.OrdinalIgnoreCase))
			.Key;

		return foundType != default
			? foundType
			: (string.IsNullOrEmpty(extension)
				? ItemType.Directory
				: ItemType.Unknown);
	}


	private string GetIcon() {
		return _iconTypeMap.TryGetValue(ItemType, out var icon)
			? icon
			: Icons.Material.Filled.Folder;
	}

	private static string GetExtension(string fileName) {
		int index = fileName.LastIndexOf('.');
		return index is -1 or <= 4
			? string.Empty
			: fileName[index..];
	}

	public string DropZoneId {
		get => string.IsNullOrEmpty(_dropZoneId)
			? GenerateUniqueId()
			: _dropZoneId;
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
	public bool CanExpand => _canExpand.Value;

	private bool ShouldExpand() {
		return TreeItems.Any();
	}

	public bool Expanded { get; set; }
	public bool Loading { get; set; }
	public bool Selected { get; set; }
	public HashSet<IDirectoryItem> TreeItems { get; set; }

	public HashSet<IDirectoryItem> BuildTreeItems() {
		if (Item is not DirectoryNode directory) return new();
		var treeItems = new HashSet<IDirectoryItem>();
		foreach (var file in directory.Files) {
			treeItems.Add(new DirectoryItem<FileNode>(file));
		}
		foreach (var folder in directory.Folders) {
			treeItems.Add(new DirectoryItem<DirectoryNode>(folder));
		}

		return treeItems;
	}

	public IEnumerable<IDirectoryItem> GetFiles() {
		return TreeItems.Where(x => x.ItemType != ItemType.Directory && x.ItemType != ItemType.Unknown);
	}

	public IEnumerable<IDirectoryItem> GetFolders() {
		return TreeItems.Where(x => x.ItemType == ItemType.Directory);
	}

	private string GenerateUniqueId() {
		switch (Item) {
			case FileNode file:
				string parentFolderPath = GetParentFolderPath(file.Path);
				return DirectoryItemHelper.DirectoryIds.TryGetValue(parentFolderPath, out int parentId)
					? parentId.ToString()
					: string.Empty;
			case DirectoryNode directory:
				int id = DirectoryItemHelper.DirectoryCounter++;
				DirectoryItemHelper.DirectoryIds[directory.Path] = id;
				return id.ToString();
			default:
				return string.Empty;
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
}

public enum ItemType {
	Directory,
	FilePdf,
	FileVideo,
	File,
	Unknown,
}

public static class DirectoryItemHelper {
	public static bool HasChildren(this DirectoryNode item) {
		return item.Folders.Any() || item.Files.Any();
	}
	public static int DirectoryCounter { get; set; } = 1;
	public static Dictionary<string, int> DirectoryIds { get; set; } = new();
}

/*
public class DirectoryItem<T> : IDirectoryItem where T : class {
	public T Item { get; init; }

	private readonly Lazy<ItemType> _type;
	private readonly Lazy<string> _icon;
	private readonly Lazy<bool> _canExpand;
	private readonly Lazy<HashSet<IDirectoryItem>> _treeItems;
	public DirectoryItem(T item) {
		Item = item;
		_type = new(GetItemType);
		_icon = new(GetIcon);
		_canExpand = new(ShouldExpand);
		_treeItems = new(BuildTreeItems);
	}

	public string Icon => _icon.Value;

	public ItemType ItemType => _type.Value;

	#region Methods
	private ItemType GetItemType() {
		var extension = GetExtension(Name).ToLowerInvariant();
		foreach (var entry
		         in _extensionTypeMap.Where(
		         entry
			         => entry.Value.Contains(extension, StringComparer.OrdinalIgnoreCase))) { return entry.Key; }
		// else return
		return string.IsNullOrEmpty(extension)
			? ItemType.Directory
			: ItemType.Unknown;
	}
	private string GetIcon() {
		return _iconTypeMap.TryGetValue(ItemType, out var icon)
			? icon
			: Icons.Material.Filled.Folder;
	}
	#endregion

	#region Static Methods
	private readonly Dictionary<ItemType, string> _iconTypeMap = new() {
		{ ItemType.FilePdf, Icons.Custom.FileFormats.FilePdf },
		{ ItemType.FileVideo, Icons.Custom.FileFormats.FileVideo },
		{ ItemType.Directory, Icons.Material.Filled.Folder },
		{ ItemType.File, Icons.Custom.FileFormats.FileDocument },
		{ ItemType.Unknown, Icons.Material.Filled.DataObject }
	};
	private readonly Dictionary<ItemType, string[]> _extensionTypeMap = new() {
		{ ItemType.FilePdf, new[] { ".pdf" } },
		{ ItemType.FileVideo, new[] { ".mp4", ".mkv" } },
	};
	private static string GetExtension(string fileName) {
		int index = fileName.LastIndexOf('.');
		return index == -1
			? string.Empty
			: fileName[index..];
	}
	#endregion

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
	public bool CanExpand => _canExpand.Value;

	private bool ShouldExpand() {
		return TreeItems.Any();
	}
	public bool Expanded { get; set; }
	public bool Loading { get; set; }

	public bool Selected { get; set; }
	public HashSet<IDirectoryItem> TreeItems => _treeItems.Value;

	private HashSet<IDirectoryItem> BuildTreeItems() {
		if (Item is not DirectoryNode directory) return new();

		var treeItems = new HashSet<IDirectoryItem>();
		foreach (var file in directory.Files) {
			treeItems.Add(new DirectoryItem<FileNode>(file));
		}
		foreach (var folder in directory.Folders) {
			treeItems.Add(new DirectoryItem<DirectoryNode>(folder));
		}

		return treeItems;
	}

	public IEnumerable<IDirectoryItem> GetFiles() {
		return TreeItems
			.Where(
			x => x is not {
				ItemType: ItemType.Directory or ItemType.Unknown
			});
	}
	public IEnumerable<IDirectoryItem> GetFolders() {
		return TreeItems
			.Where(
			x => x.ItemType == ItemType.Directory);
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

	private static string GetParentFolderPath(string path) {
		int lastSeparatorIndex = path.LastIndexOf('/');
		if (lastSeparatorIndex == -1)
			lastSeparatorIndex = path.LastIndexOf('\\');

		return lastSeparatorIndex != -1
			? path[..lastSeparatorIndex]
			: string.Empty;
	}
}

public enum ItemType {
	Directory,
	FilePdf,
	FileVideo,
	File,
	Unknown,
}

public static class DirectoryItemHelper {
	public static bool HasChildren(this DirectoryNode item) {
		return item.Folders.Any() || item.Files.Any();
	}
	public static int DirectoryCounter { get; set; } = 1;
	public static Dictionary<string, int> DirectoryIds { get; set; } = new();
}
*/
