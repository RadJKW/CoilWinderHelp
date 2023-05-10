using MudBlazor;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Extensions;
public static class DirectoryExtensions {
	public static async Task FetchTreeItems(IDirectoryItem item) {
		if (!item.Expanded || item.TreeItems == null) {
			Console.WriteLine("Not expanded or already loaded");
			return;
		}

		await Task.Run(
		() => {
			var directoryNode = item.GetFolder();
			if (directoryNode is null) return;

			foreach (var folder in directoryNode.Folders.Select(
			         folder
				         => new DirectoryItem<DirectoryNode>(folder))) { item.TreeItems.Add(folder); }

			foreach (var file in directoryNode.Files.Select(
			         file
				         => new DirectoryItem<FileNode>(file))) { item.TreeItems.Add(file); }
		});
	}
	public static readonly Dictionary<FileType, string[]> FileExtensionTypeMap = new() {
		{ FileType.Pdf, new[] { ".pdf" } },
		{ FileType.Video, new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv" } },
		{ FileType.Excel, new[] { ".csv, .xls", ".xlsx", ".xlsm", ".xlsb" } },
		{ FileType.Word, new[] { ".doc", ".docx" } },
		{ FileType.Code, new[] { ".json" } }
	};

	public static readonly Dictionary<FileType, string> FileTypeIconMap = new() {
		{ FileType.Pdf, Icons.Custom.FileFormats.FilePdf },
		{ FileType.Video, Icons.Custom.FileFormats.FileVideo },
		{ FileType.Excel, Icons.Custom.FileFormats.FileExcel },
		{ FileType.Word, Icons.Custom.FileFormats.FileWord },
		{ FileType.Code, Icons.Custom.FileFormats.FileCode }
	};

	private static ItemType GetItemType(this string path) {
		var name = path.Split('/').Last();

		int index = name.LastIndexOf('.');
		return index is -1 or <= 4
			? ItemType.Directory
			: ItemType.File;
	}


	public static IDirectoryItem AsDirectoryItem(this string path) {
		// determine if the path is a file or directory
		var itemType = path.GetItemType();
		var name = path.Split('/').Last();

		if (itemType is ItemType.File) {
			var file = new FileNode(name, path);
			return new DirectoryItem<FileNode>(file);
		}
		var directory = new DirectoryNode(name, path);
		return new DirectoryItem<DirectoryNode>(directory);
	}
	public static Enum FileOrDirectory(this string path) {
		var itemType = path.GetItemType();

		if (itemType is ItemType.Directory)
			return itemType;

		var name = path.Split('/').Last();
		int index = name.LastIndexOf('.');
		var extension = name[index..].ToLowerInvariant();
		var fileType = extension.TryMatchExtension();

		return fileType ?? FileType.Unknown;
	}

	private static FileType? TryMatchExtension(this string extension) {
		if (extension.Contains('.') is false) return null;

		extension = extension.ToLowerInvariant();

		foreach (var values
		         in FileExtensionTypeMap
			         .Where(
			         values
				         =>
				         values.Value.Contains(extension))) {
			return values.Key;
		}

		return null;
	}

	public static string GetName(this string path) {
		return path.Split('/').Last();
	}
	public static string RelativePath(this string? path) {
		return path?.Replace(AppConfig.BasePath, "").Replace("\\", "/") ?? string.Empty;
	}

	public static bool IsPdf(this string? path) {
		return path?.EndsWith(".pdf") ?? false;
	}

	public static Task<AssignedItem>? GetAssignedType(this string name) {
		AssignedItem result = default!;
		if (name[^5..].Contains('.') is false) result = AssignedItem.Directory;
		if (name.EndsWith(".pdf")) result = AssignedItem.Pdf;
		if (name.EndsWith(".mp4")) result = AssignedItem.Video;

		return result == default!
			? null
			: Task.FromResult(result);
	}
}
