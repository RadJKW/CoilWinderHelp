namespace MudBlazorPWA.Shared.Models;
public class DropItem {
	public string OriginalIdentifier { get; set; } = null!;
	public string Identifier { get; init; } = null!;
	public string Name { get; init; } = null!;
	public string Path { get; init; } = null!;

	public string Icon { get; protected init; } = null!;
	public DropItemType Type { get; init; }

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
}

public enum DropItemType {
	Folder,
	Video,
	Pdf
}
