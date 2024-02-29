using System.Diagnostics.CodeAnalysis;

namespace MudBlazorPWA.Shared.Models;
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class Media
{
	public string? Video { get; set; }
	public string? Pdf { get; set; }
	public List<string>? RefMedia { get; set; } = new();
}
