namespace MudBlazorPWA.Shared.Models;
public class DropItem {
	public string? OriginalIdentifier { get; set; }
	public string Identifier { get; set; } = null!;
	public string Name { get; init; } = null!;
	public string Path { get; init; } = null!;

	public string Icon { get; set; } = null!;
	public DropItemType Type { get; init; }

	public bool IsDisabled { get; set; }

	public bool IsCopy { get; init; }

	private DropItemType AssignTypeType() {
		if (Name.Contains('.'))
			return Name.EndsWith(".pdf")
				? DropItemType.Pdf
				: DropItemType.Video;

		return DropItemType.Folder;
	}
}

public enum DropItemType {
	Folder,
	Video,
	Pdf
}
