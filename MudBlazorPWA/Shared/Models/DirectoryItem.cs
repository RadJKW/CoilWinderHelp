using MudBlazor;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Interfaces;
using System.Text.Json.Serialization;
namespace MudBlazorPWA.Shared.Models;
public class DirectoryItem<T> : IDirectoryItem where T : class {
	public DirectoryItem(T item) {
		Item = item;
		(Name, Path, CanExpand) = BuildItemByType();
		Icon = AssignIcon();
		TreeItems = HasTreeItems();
		ItemType = Item switch {
			DirectoryNode => ItemType.Directory,
			FileNode => ItemType.File,
			_ => ItemType.Unknown
		};
	}

	public DirectoryItem(string path) {
		Name = path.Split('/') switch {
			{ Length: 0 } => string.Empty,
			{ Length: 1 } => path,
			_ => path.Split('/').Last()
		};
		Path = path;
	}
	private (string Name, string Path, bool CanExpand) BuildItemByType() {
		return Item switch {
			DirectoryNode directory => (directory.Name, directory.Path, directory.HasChildren),
			FileNode file => (file.Name, file.Path, false),
			_ => (string.Empty, string.Empty, false)
		};
	}

	private HashSet<IDirectoryItem>? HasTreeItems() {
		if (Item is not DirectoryNode directory) return null;

		return directory.HasChildren
			? new HashSet<IDirectoryItem>()
			: null;
	}
	public T? Item { get; set; }
	public bool CanExpand { get; set; }
	public bool Expanded { get; set; }
	public bool Loading { get; set; }
	public bool Selected { get; set; }
	public bool HasChildren => TreeItems != null;
	public bool HasFolders => TreeItems != null && TreeItems.Any(item => item.ItemType == ItemType.Directory);
	public bool HasFiles => TreeItems != null && TreeItems.Any(item => item.ItemType == ItemType.File);
	public string Name { get; }
	public string Path { get; }
	[JsonIgnore] public string? Icon { get; set; }
	public ItemType ItemType { get; set; }
	public IDirectoryItem? Parent { get; set; }

	public DirectoryNode? GetFolder() {
		return Item as DirectoryNode;
	}
	public HashSet<IDirectoryItem>? TreeItems { get; set; }
	private string? AssignIcon() {
		if (Item is DirectoryNode) {
			return Icons.Material.Filled.Folder;
		}

		var extension = GetExtension().ToLowerInvariant();
		// get the fileType form the directoryExtensions.FileExtensionTypeMap
		// then get the icon from the directoryExtensions.FileTypeIconMap
		foreach (var values
		         in DirectoryExtensions.FileExtensionTypeMap
			         .Where(
			         values
				         =>
				         values.Value.Contains(extension))) {
			return DirectoryExtensions.FileTypeIconMap[values.Key];
		}

		return Icons.Material.Filled.QuestionMark;
	}
	private string GetExtension() {
		if (Item is not FileNode file) return string.Empty;
		int index = file.Name.LastIndexOf('.');
		return index is -1 or <= 4
			? string.Empty
			: file.Name[index..];
	}

	public async Task FetchTreeItems() {
		if (!Expanded || TreeItems == null) {
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

public enum ItemType {
	Directory,
	File,
	Unknown
}
