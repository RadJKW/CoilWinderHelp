using MudBlazorPWA.Client.ViewModels;
namespace MudBlazorPWA.Client.Services;
public class AdminEditorState {
	public List<DropItem> DropItems { get; private set; } = new();
	public Folder? MediaDirectory { get; private set; }
	public string CurrentFolder { get; private set; } = string.Empty;
	public event Action? StateChanged;

	private readonly HubClientService _directoryHub;
	private readonly ILogger<AdminEditorState> _logger;


	public AdminEditorState(HubClientService directoryHub, ILogger<AdminEditorState> logger) {
		_directoryHub = directoryHub;
		_logger = logger;
		Initialize();
	}

	private async void Initialize() {
		_logger.LogInformation("Initializing");
		var enumerable = await _directoryHub.GetFoldersInPath();
		var paths = enumerable.Select(f => f.Insert(0, _directoryHub.WindingDocsFolder)).ToList();
		MediaDirectory = BuildDirectoryTree(paths);
		_logger.LogInformation("Completed");
		NotifyStateChanged();
	}

	private static Folder? BuildDirectoryTree(IEnumerable<string> paths) {
		var enumerable = paths.ToList();
		if (!enumerable.Any()) {
			return null;
		}
		string firstPath = enumerable.First();
		char splitChar = firstPath.Contains('\\') ? '\\' : '/';
		var splitPaths = enumerable.Select(path => path.Split(splitChar)).ToList();
		int commonParts = FindCommonParts(splitPaths);
		var root = new Folder(splitPaths[0][commonParts], string.Join(splitChar, splitPaths[0].Take(commonParts + 1)), splitChar);
		foreach (string[] splitPath in splitPaths)
			AddRemainingParts(root, splitPath, commonParts);
		return root;
	}
	private static int FindCommonParts(IEnumerable<string[]> splitPaths) {
		var commonParts = int.MaxValue;
		var stringsEnumerable = splitPaths.ToList();
		for (var i = 0; i < stringsEnumerable.First().Length; i++) {
			string currentPart = stringsEnumerable.First()[i];
			int count = 1 + stringsEnumerable.Skip(1)
				.TakeWhile(splitPath => i < splitPath.Length && splitPath[i] == currentPart)
				.Count();
			if (count == stringsEnumerable.Count) {
				commonParts = i;
			}
			else {
				break;
			}
		}
		return commonParts;
	}
	private static void AddRemainingParts(Folder? root, IReadOnlyList<string> splitPath, int commonParts) {
		Folder? current = root;
		for (int i = commonParts + 1; i < splitPath.Count; i++) {
			Folder? next = current?.SubFolders.FirstOrDefault(f => f.Name == splitPath[i]);
			if (next == null) {
				next = new(splitPath[i], current?.Path + current?.SplitChar + splitPath[i], current!.SplitChar);
				current.SubFolders.Add(next);
			}
			current = next;
		}
	}
	private void NotifyStateChanged() {
		StateChanged?.Invoke();
	}
}
