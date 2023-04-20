using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Client.ViewModels;
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
	public AdminEditorState(HubClientService directoryHub,
		ILogger<AdminEditorState> logger,
		IDirectoryNavigator directoryNavigator, IJSRuntime jsRuntime,
		NavigationManager navigation) {
		_directoryHub = directoryHub;
		_logger = logger;
		_directoryNavigator = directoryNavigator;
		_jsRuntime = jsRuntime;
		_navigation = navigation;
	}
	public List<DropItem> DropItems { get; private set; } = new();

	public DirectoryNode CurrentDirectory {
		get => _directoryNavigator.GetCurrentFolder();
		set => _directoryNavigator.NavigateToFolder(value);
	}
	public async Task FetchDirectoryTree() {
		await _directoryNavigator.InitializeAsync();
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;

		if (_directoryNavigator.NavigationHistory.Count == 0)
			_directoryNavigator.NavigateToFolder(rootDirectory);
		NotifyStateChanged();
	}
	public void NavigateToFolder(DirectoryNode folder) {
		_directoryNavigator.NavigateToFolder(folder);
	}
	public void NavigateBack() {
		_directoryNavigator.NavigateBack();
	}
	public void NavigateToRoot() {
		_directoryNavigator.NavigateToRoot();
	}
	public bool HasNavigationHistory =>
		_directoryNavigator.NavigationHistory.Count > 1;


	private void NotifyStateChanged() {
		StateChanged?.Invoke();
	}

	public async Task OpenFilePreview(string filePath) {
		var url = _navigation.BaseUri
		          + "files/"
		          + filePath;
		await _jsRuntime.InvokeVoidAsync("openFilePreview", url);
	}
	public Task AppendDropItems(IEnumerable<IDirectoryItem> itemTreeItems) {
		DropItems.AddRange(
		itemTreeItems
			.OfType<DirectoryItem<FileNode>>()
			.Select(
			item => new DropItem {
				Name = item.Name,
				Path = item.Path,
				Type = item.Name.EndsWith(".pdf")
					? DropItemType.Pdf
					: DropItemType.Video,
				Identifier = item.Path
			}));
		NotifyStateChanged();
		return Task.CompletedTask;
	}
}
