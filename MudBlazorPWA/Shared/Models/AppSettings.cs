namespace MudBlazorPWA.Shared.Models;
public class AppSettings
{

	public AppSettings(bool? drawerOpen = null, bool? appBarIsVisible = null, bool? drawerOpenOnHover = null)
	{
		DrawerOpen = drawerOpen ?? DrawerOpen;
		AppBarIsVisible = appBarIsVisible ?? AppBarIsVisible;
		DrawerOpenOnHover = drawerOpenOnHover ?? DrawerOpenOnHover;
	}
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
