namespace MudBlazorPWA.Shared.Models;
public class Media
{
	public string? Video { get; set; }
	public string? Pdf { get; set; }
	public List<string>? RefMedia { get; set; } = new();
}
