namespace MudBlazorPWA.Shared.Models;
public class AppSettings
{
	public bool DrawerOpen { get; set; }
	public bool AppBarIsVisible { get; init; } = true;
	public bool DrawerOpenOnHover { get; init; } = true;



}

public enum AppSettingsEnum
{
	DrawerOpen,
	AppBarIsVisible,
	DrawerOpenOnHover
}
