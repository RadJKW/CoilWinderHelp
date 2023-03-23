namespace MudBlazorPWA.Client.ViewModels;
public class Folder {
	public Folder(string name, string? path, char splitChar) {
		Name = name;
		Path = path;
		SplitChar = splitChar;
	}
	public string Name { get; }
	public string? Path { get; }
	public bool IsDisabled { get; set; }
	public MediaFiles MediaFiles { get; } = new();
	public Folder? RefMediaFiles { get; set; } = null;
	public List<Folder> SubFolders { get; } = new();

	// TODO: make DropItems sorted by name
	public List<DropItem> DropItems { get; } = new();
	public char SplitChar { get; }
}

public class MediaFiles
{
	public List<string>? PdfFiles { get; set; }
	public List<string>? VideoFiles { get; set; }
}
