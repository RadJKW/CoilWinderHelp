using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class AdminEditorState {
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
	public event Action? StateChanged;

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

	public List<int> WindingCodeChanges { get; set; } = new();
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
	public bool HasWindingCodeChanges { get; set; }
	public List<IDirectoryItem> AssignToDisabled { get; set; } = new();

	public async Task BuildDirectoryTree() {
		var rootDirectory = await _directoryHub.GetDirectorySnapshot();
		_directoryNavigator.RootDirectory = rootDirectory;
		var rootDirectoryItem = new DirectoryItem<DirectoryNode>(rootDirectory) { Expanded = true, Selected = true, Icon = @Icons.Material.Filled.FolderSpecial };
		RootDirectoryItem = rootDirectoryItem;
		SelectedItem = rootDirectoryItem;
		await DirectoryExtensions.FetchTreeItems(rootDirectoryItem);
		// add the RootDirectoryItem and its children to the AssignToDisabled list
		AssignToDisabled.Add(rootDirectoryItem);
		AssignToDisabled.AddRange(rootDirectoryItem.TreeItems!.Where(x => x.ItemType == ItemType.Directory));
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
	internal void NotifyStateChanged() {
		if (HasWindingCodeChanges) {
			if (!WindingCodeChanges.Contains(SelectedWindingCode!.Id))
				WindingCodeChanges.Add(SelectedWindingCode.Id);

		}
		StateChanged?.Invoke();
	}
	public async Task<WindingCode?> GetWindingCode(int windingCodeId) {
		var windingCode = await _directoryHub.GetWindingCode(windingCodeId);
		return windingCode;
	}
	public async Task UpdateWindingCodeAsync(WindingCode windingCode) {
		var result = await _directoryHub.UpdateWindingCodeDb( windingCode);
		if (result) {
			HasWindingCodeChanges = false;
			WindingCodeChanges.Remove(windingCode.Id);
			NotifyStateChanged();
		}
	}
	public async Task LoadWindingCodes() {
		await _windingCodeManager.FetchWindingCodes();
	}
	public string GetHref(string directoryItemPath)
		=> _navigation.BaseUri + "files/" + directoryItemPath;
}
