using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class AdminEditorState : IAsyncDisposable {
	public const string DzCodeFolder = "DZ-Code-Folder";
	public const string DzCodePdf = "DZ-Code-Pdf";
	public const string DzCodeVideo = "DZ-Code-Video";
	public const string DzCodeRefMedia = "DZ-Code-Ref";
	public event Action? StateChanged;

	private readonly HubClientService _directoryHub;
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AdminEditorState> _logger;
	private readonly IDirectoryNavigator _directoryNavigator;
	private readonly WindingCodeManager _windingCodeManager;
	private readonly IJSRuntime _jsRuntime;
	private readonly NavigationManager _navigation;
	private IDirectoryItem? _selectedFolder;

	public AdminEditorState(HubClientService directoryHub, ILogger<AdminEditorState> logger, IDirectoryNavigator directoryNavigator, IJSRuntime jsRuntime, NavigationManager navigation, WindingCodeManager windingCodeManager) {
		_directoryHub = directoryHub;
		_logger = logger;
		_directoryNavigator = directoryNavigator;
		_jsRuntime = jsRuntime;
		_navigation = navigation;
		_windingCodeManager = windingCodeManager;
		OnInitialized();
	}

	private async void OnInitialized() {
		_directoryHub.WindingCodesDbUpdated += async () => await OnWindingCodesDbUpdated();
		_directoryHub.WindingCodeTypeChanged += async () => await OnWindingCodesDbUpdated();
		await FetchDirectoryTree();
	}
	private async Task OnWindingCodesDbUpdated() {
		await _windingCodeManager.FetchWindingCodes();
		NotifyStateChanged();
	}
	public IDirectoryItem? SelectedFolder {
		get => _selectedFolder;
		set {
			_selectedFolder = value;
			CurrentPage = 1;
			if (value != null)
				_ = PopulateDropItems(value);
		}
	}
	private Task PopulateDropItems(IDirectoryItem value) {
		DropItems.AddRange(value.GetFiles().Where(item => item.ItemType is ItemType.File).Select(item => new DropItem(item) { DropZoneId = value.DropZoneId }));
		NotifyStateChanged();
		return Task.CompletedTask;
	}
	public IDirectoryItem? RootDirectoryItem { get; private set; }
	public WindingCode? SelectedWindingCode {
		get => _windingCodeManager.SelectedWindingCode;
		set {
			if (value != null)
				BuildCodeDropItems(value);
			_windingCodeManager.SelectedWindingCode = value;
			NotifyStateChanged();
		}
	}
	private void BuildCodeDropItems(IWindingCode windingCode) {
		// if any of the following are not null, create a drop item for it
		// - windingCode.FolderPath
		// - windingCode.Media.Pdf
		// - windingCode.Media.Video
		// - windingCode.Media.RefMedia (list of pdf and video files)
		//   * ensure that any items in refMedia are not already in the list


		var dropItems = new List<DropItem>();
		if (windingCode.FolderPath != null) {
			var assignedDropZoneId = DzCodeFolder + "-" + windingCode.Id;
			var name = windingCode.FolderPath.Split('/').Last();
			var path = windingCode.FolderPath;
			// create the new dropItem and add it to the list
			dropItems.Add(new() { DropZoneId = assignedDropZoneId, Path = path, Name = name, IsCopy = true });
		}

		if (windingCode.Media.Pdf != null) {
			var assignedDropZoneId = DzCodePdf + "-" + windingCode.Id;
			var name = windingCode.Media.Pdf.Split('/').Last();
			var path = windingCode.Media.Pdf;
			// create the new dropItem and add it to the list
			dropItems.Add(new() { DropZoneId = assignedDropZoneId, Path = path, Name = name, IsCopy = true });
		}

		if (windingCode.Media.Video != null) {
			var assignedDropZoneId = DzCodeVideo + "-" + windingCode.Id;
			var name = windingCode.Media.Video.Split('/').Last();
			var path = windingCode.Media.Video;
			// create the new dropItem and add it to the list
			dropItems.Add(new() { DropZoneId = assignedDropZoneId, Path = path, Name = name, IsCopy = true });
		}

		if (windingCode.Media.RefMedia != null) {
			var assignedDropZoneId = DzCodeRefMedia + "-" + windingCode.Id;
			dropItems.AddRange(
			from media in windingCode.Media.RefMedia
			let name = media.Split('/').Last()
			let path = media
			select new DropItem() {
				DropZoneId = assignedDropZoneId,
				Path = path,
				Name = name,
				IsCopy = true
			});
		}

		AssignedDropItems = dropItems;
	}
	public IEnumerable<WindingCode> WindingCodes {
		get => _windingCodeManager.WindingCodes;
		set => _windingCodeManager.WindingCodes = value;
	}

	public List<DropItem> DropItems { get; } = new();
	// ReSharper disable once CollectionNeverQueried.Local
	private List<DropItem> _assignedDropItems = new();

	private List<DropItem> AssignedDropItems {
		get => _assignedDropItems;
		set {
			DropItems.RemoveAll(item => _assignedDropItems.Any(dropItem => dropItem.Path == item.Path && dropItem.IsCopy));
			_assignedDropItems = value;
			DropItems.AddRange(_assignedDropItems);
			NotifyStateChanged();
		}
	}


	private async Task FetchDirectoryTree() {
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;
		var rootDirectoryItem = new DirectoryItem<DirectoryNode>(rootDirectory) { Expanded = true, Selected = true };

		RootDirectoryItem = rootDirectoryItem;
		SelectedFolder = rootDirectoryItem;
		NotifyStateChanged();

		if (_directoryNavigator.NavigationHistory.Count == 0)
			NavigateToFolder(rootDirectory);
	}
	private void NavigateToFolder(DirectoryNode folder) {
		_directoryNavigator.NavigateToFolder(folder);
		_directoryNavigator.GetCurrentFolder();
	}
	private void NotifyStateChanged() {
		StateChanged?.Invoke();
	}
	public async Task OpenFilePreview(string filePath) {
		var url = _navigation.BaseUri
		          + "files/"
		          + filePath;
		await _jsRuntime.InvokeVoidAsync("openFilePreview", url);
	}
	public bool HasFolders(IDirectoryItem directoryItem) {
		var directoryNode = _directoryNavigator.GetFolder(directoryItem.Path);
		return
			directoryNode != null
			&&
			directoryNode.Folders.Any();
	}

	private int _currentPage = 1;
	public int CurrentPage {
		get => _currentPage;
		set {
			_currentPage = value;
			NotifyStateChanged();
		}
	}
	public const int PageItemsCount = 9;

	public int SelectedItemPageCount => CalculatePageCount();

	private int CalculatePageCount() {
		var pages = 0;
		var files = SelectedFolder?.GetFiles();
		if (files == null) {
			Console.WriteLine("pages: " + pages);
			return pages;
		}
		var count = files.Count();
		pages = count / PageItemsCount;
		if (count % PageItemsCount != 0) pages++;
		Console.WriteLine("pages: " + pages);
		return pages;
	}

	public int SelectedItemFileCount() {
		return SelectedFolder?.GetFiles().Count() ?? 0;
	}
	public void AddDropItem(DropItem item) {
		if (item.IsCopy) {
			AssignedDropItems.Add(item);
		}
		DropItems.Add(item);
	}
	public void RemoveDropItem(DropItem item) {
		if (item.IsCopy) {
			AssignedDropItems.Remove(item);
		}
		DropItems.Remove(item);
	}
	public (int start, int end) GetPageRange() {
		// determine the total number of files in SelectedFolder
		// determine the start and end of the page range depending on PageItemsCount
		var files = SelectedFolder?.GetFiles();
		if (files == null) return (0, 0);
		var count = files.Count();
		int start;
		int end;

		if (count <= PageItemsCount) {
			start = 0;
			end = count;
		}
		else {
			var pages = count / PageItemsCount;
			if (count % PageItemsCount != 0) pages++;
			if (CurrentPage > pages) CurrentPage = pages;
			if (CurrentPage < 1) CurrentPage = 1;
			start = (CurrentPage - 1) * PageItemsCount;
			end = CurrentPage * PageItemsCount;
			if (end > count) end = count;
		}
		return (start, end);
	}
	ValueTask IAsyncDisposable.DisposeAsync() {
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}
}
