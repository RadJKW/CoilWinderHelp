namespace MudBlazorPWA.Shared.Models;
public class TreeItemData
{
    public string Title { get; set; }
    public string Icon { get; set; }
    public int? Number { get; set; }
    public bool CanExpand { get; set; }
    public HashSet<TreeItemData> TreeItems { get; set; } = new HashSet<TreeItemData>();

    public TreeItemData(string title, string icon, int? number = null, bool canExpand = true)
    {
        Title = title;
        Icon = icon;
        Number = number;
        CanExpand = canExpand;
    }
}
