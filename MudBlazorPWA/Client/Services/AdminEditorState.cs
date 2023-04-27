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
	private IDirectoryItem? _selectedItem;

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
	public IDirectoryItem? SelectedItem {
		get => _selectedItem;
		set {
			_selectedItem = value;
			CurrentPage = 1;
			_ = FetchSelectedDirectoryItems();
			NotifyStateChanged();
		}
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
	private async Task FetchDirectoryTree() {
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;
		var rootDirectoryItem = new DirectoryItem<DirectoryNode>(rootDirectory) { Expanded = true, Selected = true };

		RootDirectoryItem = rootDirectoryItem;
		SelectedItem = rootDirectoryItem;
		NotifyStateChanged();

		await FetchSelectedDirectoryItems();

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
	private Task FetchSelectedDirectoryItems() {
		if (SelectedItem == null || SelectedItem.TreeItems.Any()) return Task.CompletedTask;
		var directoryNode = _directoryNavigator.GetFolder(SelectedItem.Path);
		if (directoryNode == null) return Task.CompletedTask;

		var treeItems = new List<IDirectoryItem>();
		var folderTreeItems = directoryNode.Folders.Select(folder => new DirectoryItem<DirectoryNode>(folder)).ToList();
		var fileTreeItems = directoryNode.Files.Select(file => new DirectoryItem<FileNode>(file)).ToList();

		treeItems.AddRange(folderTreeItems);
		treeItems.AddRange(fileTreeItems);
		SelectedItem.TreeItems = treeItems.ToHashSet();

		// if the SelectedItem is RootDirectoryItem, then we need to update the RootDirectoryItem.TreeItems
		if (SelectedItem == RootDirectoryItem) {
			RootDirectoryItem.TreeItems = treeItems.ToHashSet();
		}
		NotifyStateChanged();
		return Task.CompletedTask;
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

	public IEnumerable<IDirectoryItem> GetPaginatedFiles() {
		var files = SelectedItem?.GetFiles();
		if (files == null) return new List<IDirectoryItem>();
		var directoryItems = files.ToList();
		var count = directoryItems.Count;
		if (count <= PageItemsCount) return directoryItems;
		var pages = count / PageItemsCount;
		if (count % PageItemsCount != 0) pages++;
		if (CurrentPage > pages) CurrentPage = pages;
		if (CurrentPage < 1) CurrentPage = 1;
		var skip = (CurrentPage - 1) * PageItemsCount;
		return directoryItems.Skip(skip).Take(PageItemsCount);
	}
	public int GetPaginationCount() {
		// get the amount of pages required to display all of SelectedItem.GetFiles()
		var files = SelectedItem?.GetFiles();
		if (files == null) return 0;
		var count = files.Count();
		var pages = count / PageItemsCount;
		if (count % PageItemsCount != 0) pages++;
		return pages;
	}
	ValueTask IAsyncDisposable.DisposeAsync() {
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}
	public int SelectedItemFileCount() { return SelectedItem?.GetFiles().Count() ?? 0; }
}
