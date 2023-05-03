using MudBlazor;
using MudBlazorPWA.Shared.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace MudBlazorPWA.Shared.Models;
public class DropItem {
	public string DropZoneId { get; init; } = null!;
	public string Name { get; init; } = null!;
	public string Path { get; init; } = null!;
	public bool IsCopy { get; init; }
	[JsonIgnore] public string Icon => _icon.Value;
	public DropItemType Type => _type.Value;
	public bool IsFolder => Type == DropItemType.Folder;

	public DropItem() {
		_type = new(AssignedType);
		_icon = new(GetIcon);
	}

	public DropItem(IDirectoryItem item, string? dropZoneId = null) {
		// Set common properties
		DropZoneId = dropZoneId ?? item.DropZoneId;
		Name = item.Name;
		Path = item.Path;
		_itemType = item.ItemType;// Store the ItemType
		_type = new(AssignedType);
		_icon = new(GetIcon);
	}

	private readonly Lazy<string> _icon;
	private readonly ItemType _itemType;
	private readonly Lazy<DropItemType> _type;
	#region Methods
	private DropItemType AssignedType() {
		// If the item is a directory, return Folder
		if (_itemType == ItemType.Directory) {
			return DropItemType.Folder;
		}

		// if the item is a file, and the extension is mapped
		// return the mapped type
		var extension = GetExtension(Name).ToLowerInvariant();
		foreach (var entry
		         in ExtensionTypeMap.Where(
		         entry
			         => entry.Value.Contains(extension, StringComparer.OrdinalIgnoreCase))) { return entry.Key; }
		// else return
		return string.IsNullOrEmpty(extension)
			? DropItemType.Folder
			: DropItemType.Unknown;
	}
	private string GetIcon() {
		if (_itemType == ItemType.Directory) {
			return Icons.Material.Filled.Folder;
		}

		return IconTypeMap.TryGetValue(Type, out var icon)
			? icon
			: Icons.Material.Filled.Error;
	}
	#endregion

	#region Static Methods
	private static readonly Dictionary<DropItemType, string> IconTypeMap = new() {
		{ DropItemType.Pdf, Icons.Custom.FileFormats.FilePdf },
		{ DropItemType.Video, Icons.Custom.FileFormats.FileVideo },
		{ DropItemType.Folder, Icons.Material.Filled.Folder },
		{ DropItemType.Media, Icons.Material.Filled.Radio },
		{ DropItemType.Unknown, Icons.Material.Filled.DataObject }
	};
	private static readonly Dictionary<DropItemType, string[]> ExtensionTypeMap = new() {
		{ DropItemType.Pdf, new[] { ".pdf" } },
		{ DropItemType.Video, new[] { ".mp4", ".mkv" } },
	};
	private static string GetExtension(string fileName) {
		int index = fileName[^4..].LastIndexOf('.');
		return index == -1
			? string.Empty
			: fileName[index..];
	}
	#endregion
	public DropItem Clone() {
		// serialize and deserialize the object
		var json = JsonSerializer.Serialize(this);
		return JsonSerializer.Deserialize<DropItem>(json) ?? throw new NullReferenceException();
	}
}

public enum DropItemType {
	Folder,
	Video,
	Pdf,
	Media,
	Unknown
}
