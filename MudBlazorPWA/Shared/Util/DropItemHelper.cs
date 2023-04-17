/*
namespace MudBlazorPWA.Shared.Util;
public static class DropItemHelper {
	public static async Task ConvertDirectoryToDropItems(Folder? rootFolder, HubConnection _directoryHub, List<DropItem> DropItems, List<WindingCode> _windingCodesWithFolders, ILogger _logger) {
		if (rootFolder == null)
			return;

		(
			rootFolder.MediaFiles.PdfFiles,
			rootFolder.MediaFiles.VideoFiles
			) = await
				_directoryHub.ListMediaFiles(rootFolder.Path);

		var dropItems = new List<object>();
		dropItems.AddRange(rootFolder.SubFolders);
		dropItems.AddRange(rootFolder.MediaFiles.PdfFiles);
		dropItems.AddRange(rootFolder.MediaFiles.VideoFiles);

		foreach (object dropItem in dropItems) {
			switch (dropItem) {
				case Folder folder:
					DropItems.AddRange(AddFolderDropItems(folder, rootFolder.Id, _directoryHub, _windingCodesWithFolders));
					break;
				case string pdf when pdf.EndsWith(".pdf"):
					DropItems.AddRange(AddPdfDropItems(pdf, rootFolder.Id, _windingCodesWithFolders));
					break;
				case string video when video.EndsWith(".mp4"):
					DropItems.AddRange(AddVideoDropItem(video, rootFolder.Id, _windingCodesWithFolders));
					break;
				default: {
					_logger.LogWarning("Unknown drop item type: {@DropItem}", dropItem);
					break;
				}
			}
		}
		foreach (Folder subFolder in rootFolder.SubFolders) {
			await ConvertDirectoryToDropItems(subFolder, _directoryHub, DropItems, _windingCodesWithFolders, _logger);
		}
	}

	// The rest of the Add*DropItems methods go here without any change.
	// ...
}
*/
