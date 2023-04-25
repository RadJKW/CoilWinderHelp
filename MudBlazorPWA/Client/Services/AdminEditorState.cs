using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class AdminEditorState {
	public event Action? StateChanged;

	private readonly HubClientService _directoryHub;
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AdminEditorState> _logger;
	private readonly IDirectoryNavigator _directoryNavigator;
	private readonly IJSRuntime _jsRuntime;
	private readonly NavigationManager _navigation;
	private IDirectoryItem? _selectedItem;

	public AdminEditorState(HubClientService directoryHub, ILogger<AdminEditorState> logger, IDirectoryNavigator directoryNavigator, IJSRuntime jsRuntime, NavigationManager navigation) {
		_directoryHub = directoryHub;
		_logger = logger;
		_directoryNavigator = directoryNavigator;
		_jsRuntime = jsRuntime;
		_navigation = navigation;
	}
	public IDirectoryItem? SelectedItem {
		get => _selectedItem;
		set {
			_selectedItem = value;
			_ = FetchSelectedDirectoryItems();
			NotifyStateChanged();
		}
	}
	public IDirectoryItem RootDirectoryItem { get; private set; } = default!;

	// ReSharper disable once UnusedAutoPropertyAccessor.Local
	private DirectoryNode CurrentDirectory { get; set; } = default!;
	public async Task FetchDirectoryTree() {
		await _directoryNavigator.InitializeAsync();
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;
		var rootDirectoryItem = new DirectoryItem<DirectoryNode>(rootDirectory) { Expanded = true, Selected = true };

		RootDirectoryItem = rootDirectoryItem;
		SelectedItem = rootDirectoryItem;

		await FetchSelectedDirectoryItems();

		if (_directoryNavigator.NavigationHistory.Count == 0)
			NavigateToFolder(rootDirectory);
		NotifyStateChanged();
	}
	private void NavigateToFolder(DirectoryNode folder) {
		_directoryNavigator.NavigateToFolder(folder);
		CurrentDirectory = _directoryNavigator.GetCurrentFolder();
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
}
