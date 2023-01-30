using MudBlazor;

namespace MudBlazorPWA.Shared.Services;
public class LayoutService
{
	public bool IsDarkMode { get; private set; }

	public MudTheme? CurrentTheme { get; private set; }

	public void SetDarkMode(bool value) {
		IsDarkMode = value;
	}

	public event EventHandler MajorUpdateOccured = null!;

	private void OnMajorUpdateOccured() => MajorUpdateOccured.Invoke(this, EventArgs.Empty);

	public void ToggleDarkMode() {
		IsDarkMode = !IsDarkMode;
		OnMajorUpdateOccured();
	}

	public void SetBaseTheme(MudTheme theme) {
		CurrentTheme = theme;
		OnMajorUpdateOccured();
	}
}
