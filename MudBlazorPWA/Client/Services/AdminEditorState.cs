using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class AdminEditorState : IAsyncDisposable {
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
			_windingCodeManager.SelectedWindingCode = value;
			NotifyStateChanged();
		}
	}
	public IEnumerable<WindingCode> WindingCodes {
		get => _windingCodeManager.WindingCodes;
		set => _windingCodeManager.WindingCodes = value;
	}

	public List<DropItem> DropItems { get; } = new();
	private List<DropItem> AssignedDropItems { get; } = new();


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
	public void NavigateToRoot() {
		_directoryNavigator.NavigateToRoot();
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

	public IEnumerable<DropItem> GetPaginatedFileDropItems() {
		var files = SelectedFolder?.GetFiles();
		if (files == null) return new List<DropItem>();
		var directoryItems = files.ToList();
		var count = directoryItems.Count;
		if (count <= PageItemsCount) return DropItems.Where(x => x.DropZoneId == SelectedFolder!.DropZoneId);
		var pages = count / PageItemsCount;
		if (count % PageItemsCount != 0) pages++;
		if (CurrentPage > pages) CurrentPage = pages;
		if (CurrentPage < 1) CurrentPage = 1;
		var skip = (CurrentPage - 1) * PageItemsCount;
		return DropItems.Skip(skip).Take(PageItemsCount).ToList();
	}
	public int PageStart() {
		var start = 0;
		var files = SelectedFolder?.GetFiles();
		if (files == null) return start;
		var count = files.Count();
		if (count <= PageItemsCount) return start;
		var pages = count / PageItemsCount;
		if (count % PageItemsCount != 0) pages++;
		if (CurrentPage > pages) CurrentPage = pages;
		if (CurrentPage < 1) CurrentPage = 1;
		start = (CurrentPage - 1) * PageItemsCount;
		Console.WriteLine("start: " + start);
		return start;
	}
	public int PageEnd() {
		var end = 0;

		var files = SelectedFolder?.GetFiles();
		if (files == null) return end;
		var count = files.Count();
		if (count <= PageItemsCount) return count;
		var pages = count / PageItemsCount;
		if (count % PageItemsCount != 0) pages++;
		if (CurrentPage > pages) CurrentPage = pages;
		if (CurrentPage < 1) CurrentPage = 1;
		end = CurrentPage * PageItemsCount;
		if (end > count) end = count;
		Console.WriteLine("end: " + end);
		return end;
	}
	public int GetPaginationCount() {
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
	ValueTask IAsyncDisposable.DisposeAsync() {
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}
	public int SelectedItemFileCount() { return SelectedFolder?.GetFiles().Count() ?? 0; }
	public void AddDropItem(DropItem item) {
		if (item.IsCopy) {
			AssignedDropItems.Add(item);
		}
		else {
			DropItems.Add(item);
		}
	}
}
