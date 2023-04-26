using MudBlazorPWA.Shared.Models;
using Blazored.LocalStorage;
namespace MudBlazorPWA.Client.Services;
public class DirectoryNavigator : IDirectoryNavigator {
	public DirectoryNode? RootDirectory { get; set; }
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

	public DirectoryNode? GetFolder(string folderPath) {
		if (folderPath == RootDirectory!.Path) return RootDirectory;
		var folder = RootDirectory!.GetFolder(folderPath);
		if (folder != null) {
			return folder;
		}
		Console.WriteLine("Folder not found at path: " + folderPath);
		return null;
	}
	public DirectoryNode GetCurrentFolder() {
		return NavigationHistory.Peek();
	}
}

public interface IDirectoryNavigator {
	DirectoryNode? RootDirectory { set; }
	ObservableStack<DirectoryNode> NavigationHistory { get; }
	void NavigateToFolder(DirectoryNode folder);
	void NavigateToFolder(string folderPath);
	void NavigateBack();
	void NavigateToRoot();
	DirectoryNode? GetFolder(string folderPath);
	DirectoryNode GetCurrentFolder();
}
