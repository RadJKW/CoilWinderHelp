using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class AdminEditorState : IAsyncDisposable {
	#region Constructor
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
	#endregion

	#region Constants
	public const string DzCodeFolder = "DZ-Code-Folder";
	public const string DzCodePdf = "DZ-Code-Pdf";
	public const string DzCodeVideo = "DZ-Code-Video";
	public const string DzCodeRefMedia = "DZ-Code-Ref";
	#endregion

	#region Properties
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
	#endregion

	#region LifeCycle
	private async void OnInitialized() {
		_windingCodeManager.WindingCodesChanged += OnWindingCodesChanged;
		WindingCodes = _windingCodeManager.WindingCodes;
		await BuildDirectoryTree();
	}

	ValueTask IAsyncDisposable.DisposeAsync() {
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}
	#endregion

	#region Events
	public event Action? StateChanged;
	public event Action? DropItemsChanged;

	private void NotifyStateChanged() => StateChanged?.Invoke();
	private void NotifyDropItemsChanged() => DropItemsChanged?.Invoke();
	#endregion

	#region Directory
	public IDirectoryItem? RootDirectoryItem { get; private set; }
	public IDirectoryItem? SelectedFolder {
		get => _selectedFolder;
		set {
			_selectedFolder = value;
			CurrentPage = 1;
			if (value != null)
				_ = BuildDirectoryDropItems(value);
		}
	}
	public int SelectedFolderFileCount() {
		return SelectedFolder?.GetFiles().Count() ?? 0;
	}
	private async Task BuildDirectoryTree() {
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;
		var rootDirectoryItem = new DirectoryItem<DirectoryNode>(rootDirectory) { Expanded = true, Selected = true };

		RootDirectoryItem = rootDirectoryItem;
		SelectedFolder = rootDirectoryItem;
		NotifyStateChanged();

		if (_directoryNavigator.NavigationHistory.Count == 0)
			NavigateToFolder(rootDirectory);
	}
	private Task BuildDirectoryDropItems(IDirectoryItem value) {
		DropItems.AddRange(
		value
			.GetFiles()
			.Where(item => item.ItemType is ItemType.File)
			.Select(
			item => new DropItem(item)
				{ DropZoneId = value.DropZoneId }));
		NotifyStateChanged();
		return Task.CompletedTask;
	}
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
	private void NavigateToFolder(DirectoryNode folder) {
		_directoryNavigator.NavigateToFolder(folder);
		_directoryNavigator.GetCurrentFolder();
	}
	public bool HasFolders(IDirectoryItem directoryItem) {
		var directoryNode = _directoryNavigator.GetFolder(directoryItem.Path);
		return
			directoryNode != null
			&&
			directoryNode.Folders.Any();
	}
	#endregion

	#region DropItems
	private List<DropItem> _assignedDropItems = new();
	public List<DropItem> AssignedDropItems {
		get => _assignedDropItems;
		private set {
			DropItems.RemoveAll(item => _assignedDropItems.Any(dropItem => dropItem.Path == item.Path && dropItem.IsCopy));
			_assignedDropItems = value;
			DropItems.AddRange(_assignedDropItems);
		}
	}
	public List<DropItem> DropItems { get; } = new();
	public void AddDropItem(DropItem item) {
		if (item.IsCopy) {
			AssignedDropItems.Add(item);
		}
		DropItems.Add(item);
		NotifyDropItemsChanged();
	}
	public void RemoveDropItem(DropItem item) {
		if (item.IsCopy) {
			AssignedDropItems.Remove(item);
		}
		DropItems.Remove(item);
		NotifyDropItemsChanged();
	}
	private void BuildCodeDropItems(IWindingCode windingCode) {
		var dropItems = new List<DropItem>();
		(string prefix, string? path)[] paths = {
			(DzCodeFolder, windingCode.FolderPath),
			(DzCodePdf, windingCode.Media.Pdf),
			(DzCodeVideo, windingCode.Media.Video)
		};

		foreach (var (prefix, path) in paths) {
			if (path != null) {
				dropItems.Add(CreateDropItem(prefix + "-" + windingCode.Id, path));
			}
		}

		if (windingCode.Media.RefMedia != null) {
			var assignedDropZoneId = DzCodeRefMedia + "-" + windingCode.Id;
			dropItems.AddRange(windingCode.Media.RefMedia.Select(media => CreateDropItem(assignedDropZoneId, media)));
		}

		AssignedDropItems = dropItems;
	}
	private static DropItem CreateDropItem(string assignedDropZoneId, string path) {
		var name = path.Split('/').Last();
		return new() { DropZoneId = assignedDropZoneId, Path = path, Name = name, IsCopy = true };
	}
	#endregion

	#region WindingCodes
	public IEnumerable<WindingCode> WindingCodes = new List<WindingCode>();
	public WindingCode? SelectedWindingCode {
		get => _windingCodeManager.SelectedWindingCode;
		set {
			if (value != null)
				BuildCodeDropItems(value);
			_windingCodeManager.SelectedWindingCode = value;
			NotifyStateChanged();
		}
	}
	public WindingCodeType SelectedWindingCodeType {
		get => _directoryHub.WindingCodeType;
		set => _directoryHub.WindingCodeType = value;
	}

	public async Task<bool> ModifyWindingCode(WindingCode windingCode) {
		return await _windingCodeManager.UpdateWindingCode(windingCode);
	}
  #endregion

	#region Event Handlers
	private void OnWindingCodesChanged(IEnumerable<WindingCode> windingCodes) {
		Console.WriteLine("OnWindingCodesChanged");
		WindingCodes = windingCodes.ToList();
		NotifyStateChanged();
	}
	#endregion

	#region Methods
	public async Task OpenFilePreview(string filePath) {
		var url = _navigation.BaseUri
		          + "files/"
		          + filePath;
		await _jsRuntime.InvokeVoidAsync("openFilePreview", url);
	}
	#endregion
	public async Task<WindingCode?> GetWindingCode(int windingCodeId) {
		return await _windingCodeManager.FetchWindingCode(windingCodeId);
	}
}
