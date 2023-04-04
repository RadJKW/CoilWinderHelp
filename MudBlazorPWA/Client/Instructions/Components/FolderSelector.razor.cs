using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Client.ViewModels;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Models;
using MudExtensions;

namespace MudBlazorPWA.Client.Instructions.Components;
public partial class FolderSelector
{
	[Parameter] public EventCallback<Folder[]?> OnFoldersSubmitted { get; set; }
	[Parameter] public EventCallback<bool> DropItemViewChanged { get; set; }
	[Parameter] public EventCallback<List<DropItem>> OnDropItemsUpdated { get; set; }
	[Parameter] public string? DirectoryPath { get; set; }

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
	private List<IWindingCode> _windingCodesWithFolders = default!;
	private List<DropItem> _dropItems = new();
	private IEnumerable<string> _folders = default!;
	private bool _rootFolderChanged;
	private bool _dropItemsDense = true;
	#endregion

	protected override async Task OnInitializedAsync() {
		DirectoryHubClient.WindingCodeTypeChanged += async () => await OnWindingCodeTypeChanged();
		var windingCodes = await DirectoryHubClient.GetCodeList();
		if (windingCodes != null)
			_windingCodesWithFolders = windingCodes.Where(w => w.FolderPath != null).ToList();
		Logger.LogInformation("WindingCodesWithFolders: {@WindingCodesWithFolders}", _windingCodesWithFolders.Count);
		_folders = await DirectoryHubClient.GetFoldersInPath();
		await OnReceiveAllFolders();
	}
	private async Task OnWindingCodeTypeChanged() {
		_dropItems.Clear();
		_windingCodesWithFolders.Clear();
		var windingCodes = await DirectoryHubClient.GetCodeList();
		if (windingCodes != null)
			_windingCodesWithFolders = windingCodes.Where(w => w.FolderPath != null).ToList();
		SetFolderAsRoot(_breadCrumbs[0]);
		await ConvertDirectoryToDropItems(_breadCrumbs[0]);
		await OnDropItemsUpdated.InvokeAsync(_dropItems);
	}
	// OnAfterRenderAsync to call JS Interop
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		// only invoke the js FUnction if the RootFolder has changed since the last render
		if (_rootFolderChanged) {
			await JSRuntime.InvokeVoidAsync("checkOverflowingElements");
			_rootFolderChanged = false;
		}
	}
	private async Task OnReceiveAllFolders() {
		string[] folders = _folders.Select(f => f.Insert(0, DirectoryHubClient.WindingDocsFolder)).ToArray();
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
		var (pdfFiles, videoFiles) = await DirectoryHubClient.ListMediaFiles(rootFolder.Path);
		rootFolder.MediaFiles.PdfFiles = pdfFiles;
		rootFolder.MediaFiles.VideoFiles = videoFiles;

		// create a list of subfolders to add to the dropzone list
		// then add each item to the list as many times as needed depending on if it should
		// appear in multiple drop-zones.
		var dropItemFolders = rootFolder.SubFolders
			.ToList();

		// iterate over dropItemFolders and call AddFolderDropItems for each folder
		foreach (Folder folder in dropItemFolders) {
			AddFolderDropItems(folder, rootFolder.Id);
		}

		var dropItemPdfs = rootFolder.MediaFiles.PdfFiles
			.ToList();

		foreach (string pdf in dropItemPdfs) {
			AddPdfDropItems(pdf, rootFolder.Id);
			AddRefDropItems(pdf);
		}

		var dropItemVideos = rootFolder.MediaFiles.VideoFiles.ToList();

		foreach (string video in dropItemVideos) {
			AddVideoDropItem(video, rootFolder.Id);
			AddRefDropItems(video);
		}

		foreach (Folder subFolder in rootFolder.SubFolders) {
			await ConvertDirectoryToDropItems(subFolder);
		}
	}
	private void AddRefDropItems(string file) {
		// this method is similar to AddVideoDropItem and AddPdfDropItems except for there is no original drop-item to add
		// this file has already been added to the list, so we need to look in _windingsCodesWithFolders to find the copies
		// that need to be added to the list
		_dropItems.AddRange(_windingCodesWithFolders
			.Where(w => w.Media.RefMedia is not null && w.Media.RefMedia.Any(r => r == file.RelativePath()))
			.Select(windingCode => new DropItem {
				Name = file.Split("/").Last(),
				Path = file.RelativePath(),
				Type = file.IsPdf() ? DropItemType.Pdf : DropItemType.Video,
				Identifier = $"DZ-Code-Ref-{windingCode.Id}",
				OriginalIdentifier = file.IsPdf() ? $"{PdfDropZoneId}-{windingCode.Id}" : $"{VideoDropZoneId}-{windingCode.Id}",
				IsCopy = true
			}));
	}
	private void AddVideoDropItem(string video, string folderId) {
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
		_dropItems.AddRange(videoDropItems);
	}
	private void AddPdfDropItems(string pdf, string folderId) {
		var dropZoneId = $"{PdfDropZoneId}-{folderId}";
		// initialize the list of (pdf) drop-items that will be added to the list
		// this list will always contain the original drop-item
		List<DropItem> pdfDropItems = new() {
			new() {
				Name = pdf.Split("/").Last(),
				Path = pdf.RelativePath(),
				Type = DropItemType.Pdf,
				Identifier = dropZoneId
			}
		};
		// add copies of the original drop-item to the list, but with the identifier of the winding code's drop-zone
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

		_dropItems.AddRange(pdfDropItems);
	}
	private void AddFolderDropItems(Folder folder, string folderId) {
		string? folderPath = folder.Path?.Replace(DirectoryHubClient.WindingDocsFolder, "");
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

		_dropItems.AddRange(folderDropItems);
	}

	private bool CanDropVideo(DropItem arg) {
		return arg.Type == DropItemType.Video && _dropItems.Any(d => d.Path == arg.Path || d.Name == arg.Name);
	}
	private bool CanDropPdf(DropItem arg) {
		return arg.Type == DropItemType.Pdf && _dropItems.Any(d => d.Path == arg.Path || d.Name == arg.Name);
	}
	private bool CanDropFolder(DropItem arg) {
		return arg.Type == DropItemType.Folder && _dropItems.Any(d => d.Path == arg.Path || d.Name == arg.Name);
	}
	private Dictionary<string, object> GetDropItemAttributes(DropItem context) {
		var attributes = new Dictionary<string, object> {
			{
				"data-path", context.Name!
			}
		};

		var copies = RootFolder?.DropItems.Where(d => d.Name == context.Name && d.IsCopy).ToList();
		if (copies == null || !copies.Any())
			return attributes;

		string identifiers = string.Join("<br />", copies.Select(c => c.Identifier));
		attributes.Add("data-title", identifiers);


		return attributes;
	}

	private async Task ToggleDropItemsDense() {
		_dropItemsDense = !_dropItemsDense;
		Console.WriteLine($"DropItemsDense: {_dropItemsDense}");
		await InvokeAsync(StateHasChanged);
	}

	public void Dispose() {
		SelectableFolders?.Dispose();
		SelectedFoldersList?.Dispose();
		SelectedItem?.Dispose();
		GC.SuppressFinalize(this);
	}

	#region Static Methods
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
	#endregion


}
