namespace MudBlazorPWA.Shared.Models;
public class AppSettings
{
	public AppSettings(bool drawerOpen, bool appBarIsVisible, bool drawerOpenOnHover) {
		DrawerOpen = drawerOpen;
		AppBarIsVisible = appBarIsVisible;
		DrawerOpenOnHover = drawerOpenOnHover;
	}
	public bool DrawerOpen { get; set; }
	public bool AppBarIsVisible { get; set; }
	public bool DrawerOpenOnHover { get; set;}

}

public enum AppSettingsEnum
{
	DrawerOpen,
	AppBarIsVisible,
	DrawerOpenOnHover
}
