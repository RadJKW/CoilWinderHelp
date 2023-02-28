using System.Text.Json.Serialization;

namespace MudBlazorPWA.Shared.Models;
public class WindingCode
{
	public int Id { get; set; }
	public string Code { get; set; } = default!;
	public Division Division { get; set; }
	public string Name { get; set; } = default!;
	public string? FolderPath { get; set; }
	public CodeTypeId CodeTypeId { get; set; }
	public Media Media { get; set; } = new();
	[JsonIgnore] public CodeType? CodeType { get; set; }
}

public enum Division
{
	D1 = 1,
	D2 = 2,
	D3 = 3
}
