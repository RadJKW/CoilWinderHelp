using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class AdminEditorState : IAsyncDisposable {
	private readonly HubClientService _directoryHub;
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<AdminEditorState> _logger;
	private readonly IDirectoryNavigator _directoryNavigator;
	private readonly WindingCodeManager _windingCodeManager;
	private readonly IJSRuntime _jsRuntime;
	private readonly NavigationManager _navigation;

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
		_windingCodeManager.WindingCodesChanged += OnWindingCodesChanged;
		WindingCodes = _windingCodeManager.WindingCodes;
		await BuildDirectoryTree();
	}

	ValueTask IAsyncDisposable.DisposeAsync() {
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}

	public event Action? StateChanged;
	private void NotifyStateChanged() => StateChanged?.Invoke();

	public IDirectoryItem? RootDirectoryItem { get; private set; }
	public IDirectoryItem? SelectedFolder { get; set; }
	private async Task BuildDirectoryTree() {
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;
		var rootDirectoryItem = new DirectoryItem<DirectoryNode>(rootDirectory) { Expanded = true, Selected = true };

		rootDirectoryItem.TreeItems = await GetTreeItems(rootDirectoryItem);
		RootDirectoryItem = rootDirectoryItem;

		SelectedFolder = rootDirectoryItem;
		NotifyStateChanged();
	}
	public bool HasChildItems(IDirectoryItem directoryItem) {
		var directoryNode = _directoryNavigator.GetFolder(directoryItem.Path);

		if (directoryNode == null) return false;

		return directoryNode.Folders.Any() || directoryNode.Files.Any();
	}


	public IEnumerable<WindingCode> WindingCodes = new List<WindingCode>();

	public WindingCode? SelectedWindingCode {
		get => _windingCodeManager.SelectedWindingCode;
		set {
			if (value != null)
				_windingCodeManager.SelectedWindingCode = value;
			NotifyStateChanged();
		}
	}
	public WindingCodeType SelectedWindingCodeType {
		get => _directoryHub.WindingCodeType;
		set => _directoryHub.WindingCodeType = value;
	}
	public async Task<WindingCode?> GetWindingCode(int windingCodeId) {
		return await _windingCodeManager.FetchWindingCode(windingCodeId);
	}
	public async Task<bool> ModifyWindingCode(WindingCode windingCode) {
		return await _windingCodeManager.UpdateWindingCode(windingCode);
	}

	private void OnWindingCodesChanged(IEnumerable<WindingCode> windingCodes) {
		Console.WriteLine("OnWindingCodesChanged");
		WindingCodes = windingCodes.ToList();
		NotifyStateChanged();
	}

	public async Task OpenFilePreview(string filePath) {
		var url = _navigation.BaseUri
		          + "files/"
		          + filePath;
		await _jsRuntime.InvokeVoidAsync("openFilePreview", url);
	}
	public Task<HashSet<IDirectoryItem>> GetTreeItems(IDirectoryItem arg) {
		if (arg.TreeItems.Any()) return Task.FromResult(arg.TreeItems);

		var treeItems = arg.BuildTreeItems();
		foreach (var treeItem in treeItems) {
			treeItem.TreeItems = treeItem.BuildTreeItems();
		}
		arg.Expanded = true;
		return Task.FromResult(treeItems);
	}
}
