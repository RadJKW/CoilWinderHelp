using MudBlazorPWA.Shared.Models;
using Blazored.LocalStorage;
namespace MudBlazorPWA.Client.Services;
public class DirectoryNavigator : IDirectoryNavigator {
	public DirectoryNode? RootDirectory {
		get => _rootDirectory;
		set {
			_rootDirectory = value;
			NavigationHistory.Push(value!);
		}
	}
	private DirectoryNode? _rootDirectory;
	public ObservableStack<DirectoryNode> NavigationHistory { get; private set; } = new();

	private readonly ILocalStorageService _localStorage;

	public DirectoryNavigator(ILocalStorageService localStorage) {
		_localStorage = localStorage;
		OnInitialized();
	}
	private async void OnInitialized() {
		NavigationHistory.StackChanged += async () =>
			await UpdateNavigationHistoryInLocalStorage();

		var storedNavigationHistory = await
			_localStorage
				.GetItemAsync<Stack<DirectoryNode>>("navigationHistory");

		if (storedNavigationHistory != null) {
			NavigationHistory = new(storedNavigationHistory);
		}
	}
	private async Task UpdateNavigationHistoryInLocalStorage() {
		await _localStorage.SetItemAsync("navigationHistory", NavigationHistory);
	}
	public void NavigateToFolder(DirectoryNode folder) {
		NavigationHistory.Push(folder);
	}
	public void NavigateToFolder(string folderPath) {
		var folder = RootDirectory!.GetFolder(folderPath);
		if (folder != null) {
			NavigateToFolder(folder);
		}
	}
	public void NavigateBack() {
		if (NavigationHistory.Count > 1) {
			NavigationHistory.Pop();
		}
	}
	public void NavigateToRoot() {
		NavigationHistory.Clear();
		NavigationHistory.Push(RootDirectory!);
	}

	public FileNode? GetFile(string filePath) {
		var file = RootDirectory!.GetFile(filePath);
		return file ?? null;
	}
	public DirectoryNode? GetFolder(string folderPath) {
		// if the folderPath is string.Empty, return the root directory
		if (string.IsNullOrEmpty(folderPath)) {
			return RootDirectory;
		}
		var folder = RootDirectory!.GetFolder(folderPath);
		return folder ?? null;
	}
	public DirectoryNode GetCurrentFolder() {
		return NavigationHistory.Count == 0
			? RootDirectory!
			: NavigationHistory.Peek();
	}
}

public interface IDirectoryNavigator {
	DirectoryNode? RootDirectory { get; set; }
	ObservableStack<DirectoryNode> NavigationHistory { get; }
	void NavigateToFolder(DirectoryNode folder);
	void NavigateToFolder(string folderPath);
	void NavigateBack();
	void NavigateToRoot();
	DirectoryNode? GetFolder(string folderPath);
	FileNode? GetFile(string filePath);
	DirectoryNode GetCurrentFolder();
}
