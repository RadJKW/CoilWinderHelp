namespace MudBlazorPWA.Shared.Models;
public class AppSettings
{
	public bool DrawerOpen { get; set; }
	public bool AppBarIsVisible { get; init; } = true;
	public bool DrawerOpenOnHover { get; init; }



}

public enum AppSettingsEnum
{
	DrawerOpen,
	AppBarIsVisible,
	DrawerOpenOnHover
}
