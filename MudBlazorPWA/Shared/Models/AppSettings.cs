namespace MudBlazorPWA.Shared.Models;
public class AppSettings
{
	public bool DrawerOpen { get; set; }
	public bool AppBarIsVisible { get; set; } = true;
	public bool DrawerOpenOnHover { get; set;}

}

public enum AppSettingsEnum
{
	DrawerOpen,
	AppBarIsVisible,
	DrawerOpenOnHover
}
