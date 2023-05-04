namespace MudBlazorPWA.Shared.Models;
public class DirectoryNode {
	public DirectoryNode(string name, string path) {
		Name = name;
		Path = path;
	}
	public string Name { get; set; }
	public string Path { get; set; }
	public List<DirectoryNode> Folders { get; set; } = new();
	public List<FileNode> Files { get; set; } = new();
	public bool HasChildren => Folders.Any() || Files.Any();
	public DirectoryNode? GetFolder(string folderPath) {
		var folderNode =
			Folders.FirstOrDefault(f => f.Path == folderPath)
			?? Folders.Select(f => f.GetFolder(folderPath)).FirstOrDefault(f => f != null);
		return folderNode ?? null;
	}
	public FileNode? GetFile(string filePath) {
		var fileNode =
			Files.FirstOrDefault(f => f.Path == filePath)
			?? Folders.Select(f => f.GetFile(filePath)).FirstOrDefault(f => f != null);
		return fileNode ?? null;
	}
}

public class FileNode {
	public FileNode(string name, string path) {
		Name = name;
		Path = path;
	}
	public string Name { get; set; }
	public string Path { get; set; }
}
