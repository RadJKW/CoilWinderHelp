namespace MudBlazorPWA.Shared.Services;
public class DocViewService
{
	public event EventHandler? MajorUpdateOccured;
	private void OnMajorUpdateOccured() => MajorUpdateOccured?.Invoke(this, EventArgs.Empty);

	public AppSettings AppSettings { get; private set; } = new();

	public void SetAppSettings(AppSettings appSettings) {
		AppSettings = appSettings;
		OnMajorUpdateOccured();
	}
}

public class AppSettings
{
	public string RootDirectoryPath { get; set; } = string.Empty;
	public bool AppBarIsVisible { get; set; } = true;
	public bool DrawerOpen { get; set; } = false;
	public bool DrawerOpenOnHover { get; set; } = true;
}
