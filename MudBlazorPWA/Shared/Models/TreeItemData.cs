namespace MudBlazorPWA.Shared.Models;
public class TreeItemData
{
    public string Title { get; set; }
    public ItemType Type => Path.LastIndexOf('.') > 0 ? ItemType.File : ItemType.Folder;
    public string Path { get; set; }
    public string Icon { get; set; }
    public int? Number { get; set; }
    public bool CanExpand { get; set; }

    public HashSet<TreeItemData> TreeItems { get; set; } = new HashSet<TreeItemData>();

    public TreeItemData(string title, string path, string icon, int? number = null, bool canExpand = true)
    {
        Title = title;
        Path = path;
        Icon = icon;
        Number = number;
        CanExpand = canExpand;
    }

    public enum ItemType
    {
        Folder,
        File
    }
}
