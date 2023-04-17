namespace MudBlazorPWA.Shared.Models;
public class DirectoryNode
{
	public DirectoryNode(string name, string path) {
		Name = name;
		Path = path;
	}
	public string Name { get; set; }
	public string Path { get; set; }
	public List<DirectoryNode> Folders { get; set; } = new();
	public List<FileNode> Files { get; set; } = new List<FileNode>();
}

public class FileNode
{
	public FileNode(string name, string path) {
		Name = name;
		Path = path;
	}
	public string Name { get; set; }
	public string Path { get; set; }
}
