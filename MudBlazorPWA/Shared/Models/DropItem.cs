using MudBlazor;
using MudBlazorPWA.Shared.Interfaces;
using System.Text.Json.Serialization;
namespace MudBlazorPWA.Shared.Models;
public class DropItem {
	public string OriginalIdentifier { get; init; } = null!;
	public string DropZoneId { get; set; } = null!;
	public string Name { get; init; } = null!;
	public string Path { get; init; } = null!;

	[JsonIgnore]
	public string Icon { get; private set; } = null!;
	public DropItemType Type => AssignedType();

	public bool IsDisabled { get; set; }

	public bool IsCopy { get; init; }

	// ReSharper disable once UnusedMember.Local
	private DropItemType AssignedType() {
		if (Name.Contains('.'))
			return Name.EndsWith(".pdf")
				? DropItemType.Pdf
				: DropItemType.Video;

		return DropItemType.Folder;
	}

	public DropItem() {
	}

	public DropItem(IDirectoryItem item) {
		// Set common properties
		DropZoneId = item.DropZoneId;
		Name = item.Name;
		Path = item.Path;
		Icon = item.ItemType switch {
			ItemType.File => item.Name.EndsWith(".pdf")
				? Icons.Custom.FileFormats.FilePdf
				: Icons.Custom.FileFormats.FileVideo,
			ItemType.Directory => Icons.Material.Filled.Folder,
			_ => throw new ArgumentOutOfRangeException(nameof(item))
		};
	}
}

public enum DropItemType {
	Folder,
	Video,
	Pdf
}
