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

			var items = directoryNode.Folders.Select(d => new DirectoryItem<DirectoryNode>(d)).Cast<IDirectoryItem>().ToList();

			items.AddRange(directoryNode.Files.Select(f => new DirectoryItem<FileNode>(f)));
			// sort the items so that they are all sorted alphabetically
			// folders first, then files


			items = items.OrderBy(i => i.Name).ToList();
			foreach (var directoryItem in items) {
				item.TreeItems.Add(directoryItem);
			}
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
}
