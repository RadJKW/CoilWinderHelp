namespace MudBlazorPWA.Shared.Models;
public class DirectoryNode {
	public DirectoryNode(string name, string path) {
		Name = name;
		Path = path;
	}
	public string Name { get; set; }
	public string Path { get; set; }
	public List<DirectoryNode> Folders { get; set; } = new();
	public List<FileNode> Files { get; set; } = new List<FileNode>();
}

public class FileNode {
	public FileNode(string name, string path) {
		Name = name;
		Path = path;
	}
	public string Name { get; set; }
	public string Path { get; set; }
}
/*@*<Content>
	@if (context.CanExpand) {
	<MudTreeViewItemToggleButton Visible="true"
	LoadingIcon="@Icons.Material.Filled.Loop"
	Loading="@context.Loading"
	Expanded="context.Expanded"
	ExpandedChanged="context.OnExpandedChanged"/>

}
<MudIcon Icon="@context.Icon"></MudIcon>
	<div style="display: grid; grid-template-columns: 1fr auto; align-items: center; width: 100%">
	<MudText Style="justify-self: start;">@context.Name</MudText>
	<div style="justify-self: end;">
	<MudIconButton Icon="@Icons.Material.Filled.OpenInBrowser"
Size="Size.Small"
Color="Color.Inherit"/>
	<MudIconButton Icon="@Icons.Material.Filled.ContentCopy"
OnClick="() => OpenFilePreview(context)"
Size="Size.Medium"
Color="Color.Inherit"/>
	</div>
	</div>
	</Content>*@*/
