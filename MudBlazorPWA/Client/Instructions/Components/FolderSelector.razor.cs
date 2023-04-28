using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Client.ViewModels;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Models;
using MudExtensions;

namespace MudBlazorPWA.Client.Instructions.Components;
public partial class FolderSelector : IDisposable {
	[Parameter] public EventCallback<List<DropItem>> OnDropItemsUpdated { get; set; }

	private const string FolderDropZoneId = "DZ-Folder";
	private const string PdfDropZoneId = "DZ-Pdf";
	private const string VideoDropZoneId = "DZ-Video";

	#region Properties
	private readonly List<Folder?> _breadCrumbs = new();
	private MudListExtended<Folder>? SelectableFolders { get; set; }
	private MudListExtended<Folder>? SelectedFoldersList { get; set; }
	private MudListItemExtended<Folder?>? SelectedItem { get; set; }
	private Folder? SelectedValue { get; set; }
	private Folder? RootFolder { get; set; }
	private List<WindingCode> _windingCodesWithFolders = default!;
	private List<DropItem> _dropItems = new();
	private IEnumerable<string> _folders = default!;
	private bool _rootFolderChanged;
	private bool _dropItemsDense = true;
	#endregion

	#region Component Lifecycle
	protected override async Task OnInitializedAsync() {
		DirectoryHubClient.WindingCodeTypeChanged += async () => await OnWindingCodeTypeChanged();
		DirectoryHubClient.WindingCodesDbUpdated += async () => await OnWindingCodeTypeChanged();
		var windingCodes = await DirectoryHubClient.GetWindingCodes();
		_windingCodesWithFolders = windingCodes.Where(w => w.FolderPath != null)
			.ToList();
		Logger.LogInformation("WindingCodesWithFolders: {@WindingCodesWithFolders}", _windingCodesWithFolders.Count);
		_folders = await DirectoryHubClient.GetFoldersInPath();
		await OnReceiveAllFolders();
	}
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		// only invoke the js FUnction if the RootFolder has changed since the last render

		if (_rootFolderChanged) {
			await JSRuntime.InvokeVoidAsync("checkOverflowingElements");
			_rootFolderChanged = false;
		}
		await base.OnAfterRenderAsync(firstRender);
	}

	public void Dispose() {
		SelectableFolders?.Dispose();
		SelectedFoldersList?.Dispose();
		SelectedItem?.Dispose();
		GC.SuppressFinalize(this);
	}
	#endregion

	#region Event Handlers
	private async Task OnWindingCodeTypeChanged() {
		_dropItems.Clear();
		_windingCodesWithFolders.Clear();
		var windingCodes = await DirectoryHubClient.GetWindingCodes();
		_windingCodesWithFolders = windingCodes.Where(w => w.FolderPath != null)
			.ToList();
		SetFolderAsRoot(_breadCrumbs[0]);
		await ConvertDirectoryToDropItems(_breadCrumbs[0]);
		await OnDropItemsUpdated.InvokeAsync(_dropItems);
	}
	#endregion

	// OnAfterRenderAsync to call JS Interop

	#region Methods
	private async Task OnReceiveAllFolders() {
		string[] folders = _folders.Select(f => f.Insert(0, DirectoryHubClient.WindingDocsFolder))
			.ToArray();
		RootFolder = BuildDirectoryTree(folders);
		if (RootFolder == null) return;

		_breadCrumbs.Add(RootFolder);
		_rootFolderChanged = true;
		await ConvertDirectoryToDropItems(RootFolder);
		await OnDropItemsUpdated.InvokeAsync(_dropItems);
		StateHasChanged();
	}
	private void SetFolderAsRoot() {
		if (SelectedValue == null)
			return;

		RootFolder = SelectedValue;
		_rootFolderChanged = true;
		_breadCrumbs.Add(SelectedValue);
		SelectableFolders!.Clear();
		SelectedValue = null;
	}
	private void SetFolderAsRoot(Folder? newRoot) {
		if (newRoot == null)
			return;
		if (newRoot == _breadCrumbs.Last())
			return;

		if (newRoot == RootFolder) {
			_breadCrumbs.Clear();
			_breadCrumbs.Add(newRoot);
		}
		else {
			int index = _breadCrumbs.IndexOf(newRoot);
			_breadCrumbs.RemoveRange(index, _breadCrumbs.Count - index);
			RootFolder = newRoot;
			_rootFolderChanged = true;
		}
		_breadCrumbs.Add(newRoot);
		SelectableFolders!.Clear();
	}
	private async Task ConvertDirectoryToDropItems(Folder? rootFolder) {
		if (rootFolder == null)
			return;

		(
			rootFolder.MediaFiles.PdfFiles,
			rootFolder.MediaFiles.VideoFiles
			) = await
				DirectoryHubClient.ListMediaFiles(rootFolder.Path);

		var dropItems = new List<object>();
		dropItems.AddRange(rootFolder.SubFolders);
		dropItems.AddRange(rootFolder.MediaFiles.PdfFiles);
		dropItems.AddRange(rootFolder.MediaFiles.VideoFiles);


		foreach (object dropItem in dropItems) {
			switch (dropItem) {
				case Folder folder:
					_dropItems.AddRange(AddFolderDropItems(folder, rootFolder.Id));
					break;
				case string pdf when pdf.EndsWith(".pdf"):
					_dropItems.AddRange(AddPdfDropItems(pdf, rootFolder.Id));
					break;
				case string video when video.EndsWith(".mp4"):
					_dropItems.AddRange(AddVideoDropItem(video, rootFolder.Id));
					break;
				default: {
					Logger.LogWarning("Unknown drop item type: {@DropItem}", dropItem);
					break;
				}
			}
		}
		foreach (var subFolder in rootFolder.SubFolders) {
			await ConvertDirectoryToDropItems(subFolder);
		}
	}
	private IEnumerable<DropItem> AddFolderDropItems(Folder folder, string folderId) {
		string folderPath = folder.Path!.Replace(DirectoryHubClient.WindingDocsFolder, "");
		var dropZoneId = $"{FolderDropZoneId}-{folderId}";
		List<DropItem> folderDropItems = new() {
			new() {
				Name = folder.Name,
				Path = folderPath,
				DropZoneId = dropZoneId
			}
		};

		// add copies of the original drop-item to the list, but with the identifier of the winding code's drop-zone
		folderDropItems.AddRange(
		_windingCodesWithFolders
			.Where(
			w =>
				w.FolderPath == folderPath)
			.Select(
			windingCode => new DropItem {
				Name = folder.Name,
				Path = folder.Path.RelativePath(),
				DropZoneId = $"DZ-Code-Folder-{windingCode.Id}",
				OriginalIdentifier = dropZoneId,
				IsCopy = true
			}));

		return folderDropItems;
	}
	private IEnumerable<DropItem> AddPdfDropItems(string pdf, string folderId) {
		var dropZoneId = $"{PdfDropZoneId}-{folderId}";

		List<DropItem> pdfDropItems = new() {
			new() {
				Name = pdf.Split("/")
					.Last(),
				Path = pdf.RelativePath(),
				DropZoneId = dropZoneId
			}
		};
		pdfDropItems.AddRange(
		_windingCodesWithFolders
			.Where(w => w.Media.Pdf == pdf.RelativePath() || w.Media.Pdf == pdf)
			.Select(
			windingCode => new DropItem {
				Name = pdf.Split("/")
					.Last(),
				Path = pdf.RelativePath(),
				DropZoneId = $"DZ-Code-Pdf-{windingCode.Id}",
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
				Name = video.Split("/")
					.Last(),
				Path = video.RelativePath(),
				DropZoneId = dropZoneId
			}
		};
		// add copies of the original drop-item to the list, but with the identifier of the winding code's drop-zone
		videoDropItems.AddRange(
		_windingCodesWithFolders
			.Where(w => w.Media.Video == video.RelativePath() || w.Media.Video == video)
			.Select(
			windingCode => new DropItem {
				Name = video.Split("/")
					.Last(),
				Path = video.RelativePath(),
				DropZoneId = $"DZ-Code-Video-{windingCode.Id}",
				OriginalIdentifier = dropZoneId,
				IsCopy = true
			}));

		videoDropItems.AddRange(AddRefDropItems(video));
		return videoDropItems;
	}
	private IEnumerable<DropItem> AddRefDropItems(string file) {
		var refDropItems = _windingCodesWithFolders
			.Where(w => w.Media.RefMedia is not null && w.Media.RefMedia.Any(r => r == file.RelativePath()))
			.Select(
			windingCode => new DropItem {
				Name = file.Split("/")
					.Last(),
				Path = file.RelativePath(),
				DropZoneId = $"DZ-Code-Ref-{windingCode.Id}",
				OriginalIdentifier = file.IsPdf()
					? $"{PdfDropZoneId}-{windingCode.Id}"
					: $"{VideoDropZoneId}-{windingCode.Id}",
				IsCopy = true
			});
		return refDropItems;
	}
	private static bool CanAcceptDropItem(DropItem arg) {
		return arg.Type switch {
			DropItemType.Pdf => arg.Type == DropItemType.Pdf,
			DropItemType.Video => arg.Type == DropItemType.Video,
			DropItemType.Folder => arg.Type == DropItemType.Folder,
			_ => false
		};
	}
	private async Task ToggleDropItemsDense() {
		_dropItemsDense = !_dropItemsDense;
		Console.WriteLine($"DropItemsDense: {_dropItemsDense}");
		await InvokeAsync(StateHasChanged);
	}
	#endregion

	#region Static Methods
	private static Folder? BuildDirectoryTree(IEnumerable<string> paths) {
		var enumerable = paths.ToList();
		if (!enumerable.Any()) {
			return null;
		}
		string firstPath = enumerable.First();
		char splitChar = firstPath.Contains('\\')
			? '\\'
			: '/';
		var splitPaths = enumerable.Select(path => path.Split(splitChar))
			.ToList();
		int commonParts = FindCommonParts(splitPaths);
		var root = new Folder(
		splitPaths[0][commonParts],
		string.Join(
		splitChar,
		splitPaths[0]
			.Take(commonParts + 1)),
		splitChar);
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
		var current = root;
		for (int i = commonParts + 1; i < splitPath.Count; i++) {
			var next = current?.SubFolders.FirstOrDefault(f => f.Name == splitPath[i]);
			if (next == null) {
				next = new(splitPath[i], current?.Path + current?.SplitChar + splitPath[i], current!.SplitChar);
				current.SubFolders.Add(next);
			}
			current = next;
		}
	}
	#endregion
}
