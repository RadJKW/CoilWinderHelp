using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class AdminEditorState {
	public event Action? StateChanged;

	private readonly HubClientService _directoryHub;
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AdminEditorState> _logger;
	private readonly IDirectoryNavigator _directoryNavigator;
	public AdminEditorState(HubClientService directoryHub, ILogger<AdminEditorState> logger, IDirectoryNavigator directoryNavigator) {
		_directoryHub = directoryHub;
		_logger = logger;
		_directoryNavigator = directoryNavigator;
	}

	public DirectoryNode CurrentDirectory {
		get => _directoryNavigator.GetCurrentFolder();
		set => _directoryNavigator.NavigateToFolder(value);
	}
	public async Task FetchDirectoryTree() {
		await _directoryNavigator.InitializeAsync();
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;

		if (_directoryNavigator.NavigationHistory.Count == 0) {
			_directoryNavigator.NavigateToFolder(rootDirectory);
		}
		NotifyStateChanged();
	}

	public void NavigateToFolder(DirectoryNode folder) =>
		_directoryNavigator.NavigateToFolder(folder);
	public void NavigateBack() =>
		_directoryNavigator.NavigateBack();
	public void NavigateToRoot() =>
		_directoryNavigator.NavigateToRoot();
	public bool HasNavigationHistory =>
		_directoryNavigator.NavigationHistory.Count > 1;

	private void NotifyStateChanged() {
		StateChanged?.Invoke();
	}
}
/*
 private const string FolderDropZoneId = "DZ-Folder";
	private const string PdfDropZoneId = "DZ-Pdf";
	private const string VideoDropZoneId = "DZ-Video";
 private List<DropItem> DropItems { get; set; } = new();
	private Folder? MediaDirectory { get; set; }
	public string CurrentFolder { get; private set; } = string.Empty;

	private readonly List<IWindingCode> _windingCodes = new();
	private readonly List<IWindingCode> _windingCodesWithFolders = new();
	// private readonly List<Folder?> _breadcrumbs = new();
private async Task ConvertDirectoryToDropItems(Folder? rootFolder) {
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
					DropItems.AddRange(AddFolderDropItems(folder, rootFolder.Id));
					break;
				case string pdf when pdf.EndsWith(".pdf"):
					DropItems.AddRange(AddPdfDropItems(pdf, rootFolder.Id));
					break;
				case string video when video.EndsWith(".mp4"):
					DropItems.AddRange(AddVideoDropItem(video, rootFolder.Id));
					break;
				default: {
					_logger.LogWarning("Unknown drop item type: {@DropItem}", dropItem);
					break;
				}
			}
		}
		foreach (Folder subFolder in rootFolder.SubFolders) {
			await ConvertDirectoryToDropItems(subFolder);
		}
	}
	private IEnumerable<DropItem> AddFolderDropItems(Folder folder, string folderId) {
		string? folderPath = folder.Path?.Replace(_directoryHub.WindingDocsFolder, "");
		var dropZoneId = $"{FolderDropZoneId}-{folderId}";
		List<DropItem> folderDropItems = new() {
			new() {
				Name = folder.Name,
				Path = folderPath,
				Type = DropItemType.Folder,
				Identifier = dropZoneId
			}
		};

		// add copies of the original drop-item to the list, but with the identifier of the winding code's drop-zone
		folderDropItems.AddRange(_windingCodesWithFolders
			.Where(w =>
				w.FolderPath == folderPath)
			.Select(windingCode => new DropItem {
				Name = folder.Name,
				Path = folder.Path.RelativePath(),
				Type = DropItemType.Folder,
				Identifier = $"DZ-Code-Folder-{windingCode.Id}",
				OriginalIdentifier = dropZoneId,
				IsCopy = true
			}));

		return folderDropItems;
	}
	private IEnumerable<DropItem> AddPdfDropItems(string pdf, string folderId) {
		var dropZoneId = $"{PdfDropZoneId}-{folderId}";

		List<DropItem> pdfDropItems = new() {
			new() {
				Name = pdf.Split("/").Last(),
				Path = pdf.RelativePath(),
				Type = DropItemType.Pdf,
				Identifier = dropZoneId
			}
		};
		pdfDropItems.AddRange(
		_windingCodesWithFolders
			.Where(w => w.Media.Pdf == pdf.RelativePath() || w.Media.Pdf == pdf)
			.Select(windingCode => new DropItem {
				Name = pdf.Split("/").Last(),
				Path = pdf.RelativePath(),
				Type = DropItemType.Pdf,
				Identifier = $"DZ-Code-Pdf-{windingCode.Id}",
				OriginalIdentifier = dropZoneId,
				IsCopy = true
			}));

		pdfDropItems.AddRange(AddRefDropItems(pdf));

		return pdfDropItems;
	}
	private IEnumerable<DropItem> AddVideoDropItem(string video, string folderId) {
		var dropZoneId = $"{VideoDropZoneId}-{folderId}";

		// initialize the list of (video) drop-items that will be added to the list
		// this list will always contain the original drop-item
		List<DropItem> videoDropItems = new() {
			new() {
				Name = video.Split("/").Last(),
				Path = video.RelativePath(),
				Type = DropItemType.Video,
				Identifier = dropZoneId
			}
		};
		// add copies of the original drop-item to the list, but with the identifier of the winding code's drop-zone
		videoDropItems.AddRange(_windingCodesWithFolders
			.Where(w => w.Media.Video == video.RelativePath() || w.Media.Video == video)
			.Select(windingCode => new DropItem {
				Name = video.Split("/").Last(),
				Path = video.RelativePath(),
				Type = DropItemType.Video,
				Identifier = $"DZ-Code-Video-{windingCode.Id}",
				OriginalIdentifier = dropZoneId,
				IsCopy = true
			}));

		videoDropItems.AddRange(AddRefDropItems(video));
		return videoDropItems;
	}
	private IEnumerable<DropItem> AddRefDropItems(string file) {
		var refDropItems = _windingCodesWithFolders
			.Where(w => w.Media.RefMedia is not null && w.Media.RefMedia.Any(r => r == file.RelativePath()))
			.Select(windingCode => new DropItem {
				Name = file.Split("/").Last(),
				Path = file.RelativePath(),
				Type = file.IsPdf() ? DropItemType.Pdf : DropItemType.Video,
				Identifier = $"DZ-Code-Ref-{windingCode.Id}",
				OriginalIdentifier = file.IsPdf() ? $"{PdfDropZoneId}-{windingCode.Id}" : $"{VideoDropZoneId}-{windingCode.Id}",
				IsCopy = true
			});
		return refDropItems;
	}

	private static Folder? BuildDirectoryTree(IEnumerable<string> paths) {
		var enumerable = paths.ToList();
		if (!enumerable.Any()) {
			return null;
		}
		string firstPath = enumerable.First();
		char splitChar = firstPath.Contains('\\') ? '\\' : '/';
		var splitPaths = enumerable.Select(path => path.Split(splitChar)).ToList();
		int commonParts = FindCommonParts(splitPaths);
		var root = new Folder(splitPaths[0][commonParts], string.Join(splitChar, splitPaths[0].Take(commonParts + 1)), splitChar);
		foreach (string[] splitPath in splitPaths)
			AddRemainingParts(root, splitPath, commonParts);
		return root;
	}
	private static int FindCommonParts(IEnumerable<string[]> splitPaths) {
		var commonParts = int.MaxValue;
		var stringsEnumerable = splitPaths.ToList();
		for (var i = 0; i < stringsEnumerable.First().Length; i++) {
			string currentPart = stringsEnumerable.First()[i];
			int count = 1 + stringsEnumerable.Skip(1)
				.TakeWhile(splitPath => i < splitPath.Length && splitPath[i] == currentPart)
				.Count();
			if (count == stringsEnumerable.Count) {
				commonParts = i;
			}
			else {
				break;
			}
		}
		return commonParts;
	}
	private static void AddRemainingParts(Folder? root, IReadOnlyList<string> splitPath, int commonParts) {
		Folder? current = root;
		for (int i = commonParts + 1; i < splitPath.Count; i++) {
			Folder? next = current?.SubFolders.FirstOrDefault(f => f.Name == splitPath[i]);
			if (next == null) {
				next = new(splitPath[i], current?.Path + current?.SplitChar + splitPath[i], current!.SplitChar);
				current.SubFolders.Add(next);
			}
			current = next;
		}
	}


private async Task PopulateDropItems() {
		_windingCodes.AddRange(await _directoryHub.GetCodeList());
		_windingCodesWithFolders.Clear();
		_windingCodesWithFolders.AddRange(_windingCodes.Where(w => w.FolderPath != null));
		await ConvertDirectoryToDropItems(MediaDirectory);
	}
	private async Task PopulateDirectoryTree() {
		var enumerable = await _directoryHub.GetFoldersInPath();
		var paths = enumerable.Select(f => f.Insert(0, _directoryHub.WindingDocsFolder)).ToList();
		MediaDirectory = BuildDirectoryTree(paths);
		CurrentFolder = _directoryHub.WindingDocsFolder;
	}*/
