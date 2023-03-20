namespace MudBlazorPWA.Client.ViewModels;
public class DropItem
{
	public string? Identifier { get; set; }
	public string? Name { get; set; }
	public string? Path { get; set; }
	public DropItemType Type { get; set; }
}

public enum DropItemType
{
	Folder,
	Video,
	Pdf
}
