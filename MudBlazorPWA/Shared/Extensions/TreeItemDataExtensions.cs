using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Extensions;

public static class TreeItemDataExtensions
{
    public static void ConvertToTreeItems(this TreeItemData parentNode, string path, string[]? files, string fileIcon, string[]? folders, string folderIcon)
    {
        if (folders != null)
        {
            foreach (var folder in folders)
            {
                parentNode.TreeItems.Add(new(folder, folderIcon)
                {
                    TreeItems = new()
                });
            }
        }

        // ReSharper disable once InvertIf
        if (files != null)
        {
            foreach (var file in files)
            {
                parentNode.TreeItems.Add(new(title: file, icon: fileIcon, canExpand: false));
            }
        }

    }
}
