namespace MudBlazorPWA.Shared.Models;
public class Folder {
	public Folder(string name, string? path) {
		Name = name;
		Path = path;
	}
	public string Name { get; init; }
	public string? Path { get; init; }
	public bool IsDisabled { get; set; }
	//public RenderFragment? ChildrenFolders { get; set; }
	public List<Folder> SubFolders { get; set; } = new();
}
