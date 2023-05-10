using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPWA.Shared.Extensions;
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
	private void OnInitialized() {
		_windingCodeManager.WindingCodesChanged += OnWindingCodesChanged;
		WindingCodes = _windingCodeManager.WindingCodes;
	}
	ValueTask IAsyncDisposable.DisposeAsync() {
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}
	public event Action? StateChanged;

	public readonly Dictionary<AssignedItem, List<IDirectoryItem>> InitialAssignedItems = new();
	public IDirectoryItem? RootDirectoryItem { get; private set; }
	private IDirectoryItem? _selectedItem;
	public IDirectoryItem? SelectedItem {
		get => _selectedItem;
		set {
			_selectedItem = value;
			NotifyStateChanged();
		}
	}
	public IEnumerable<WindingCode> WindingCodes = new List<WindingCode>();
	public WindingCode? SelectedWindingCode {
		get => _windingCodeManager.SelectedWindingCode;
		set {
			_windingCodeManager.SelectedWindingCode = value;
			NotifyStateChanged();
		}
	}
	public WindingCodeType SelectedWindingCodeType {
		get => _directoryHub.WindingCodeType;
		set => _directoryHub.WindingCodeType = value;
	}
	public async Task BuildDirectoryTree() {
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;
		var rootDirectoryItem = new DirectoryItem<DirectoryNode>(rootDirectory) { Expanded = true, Selected = true, Icon = @Icons.Material.Filled.FolderSpecial};
		RootDirectoryItem = rootDirectoryItem;
		SelectedItem = rootDirectoryItem;
		await DirectoryExtensions.FetchTreeItems(rootDirectoryItem);
		// TreeItems.Add(rootDirectoryItem);
		NotifyStateChanged();
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
	internal void NotifyStateChanged() => StateChanged?.Invoke();
}
