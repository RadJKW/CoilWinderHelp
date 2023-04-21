namespace MudBlazorPWA.Client.ViewModels;
public class DropItem {
	private static int _counter;
	public DropItem(string name, string path, string icon, string identifier) {
		Identifier = identifier;
		Name = name;
		Path = path;
		Icon = icon;
		Type = AssignTypeType();
		Id = GenerateUniqueId();
	}

	public DropItem() {
		Id = GenerateUniqueId();
	}

	public int Id { get; set; }
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
	private static int GenerateUniqueId() {
		// return the incremented value of _counter as two hex digits
		return _counter++;
	}
}

public enum DropItemType {
	Folder,
	Video,
	Pdf
}
