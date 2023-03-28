namespace MudBlazorPWA.Client.ViewModels;
public class Folder
{
	private static int _counter;
	public Folder(string name, string? path, char splitChar) {
		Name = name;
		Path = path;
		SplitChar = splitChar;
		Id = GenerateUniqueId();
	}

	public string Id { get; }
	public string Name { get; }
	public string? Path { get; }
	public bool IsDisabled { get; set; }
	public MediaFiles MediaFiles { get; } = new();
	public Folder? RefMediaFiles { get; set; } = null;
	public List<Folder> SubFolders { get; } = new();

	// TODO: make DropItems sorted by name
	public List<DropItem> DropItems { get; } = new();
	public char SplitChar { get; }

	private static string GenerateUniqueId() {
		// return the incremented value of _counter as two hex digits
		return _counter++.ToString("X2");
	}
}

public class MediaFiles
{
	public List<string>? PdfFiles { get; set; }
	public List<string>? VideoFiles { get; set; }
}
