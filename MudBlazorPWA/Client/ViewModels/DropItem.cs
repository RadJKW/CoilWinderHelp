namespace MudBlazorPWA.Client.ViewModels;
public class DropItem {
	private static int _counter;
	public DropItem() {
		Id = GenerateUniqueId();
	}

	public int Id { get; set; }
	public string? OriginalIdentifier { get; set; }
	public string? Identifier { get; set; }
	public string? Name { get; set; }
	public string? Path { get; set; }
	public DropItemType Type { get; set; }

	public bool IsDisabled { get; set; }

	public bool IsCopy { get; set; }

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
