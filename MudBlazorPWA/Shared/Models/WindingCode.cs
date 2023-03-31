using System.Text.Json.Serialization;

namespace MudBlazorPWA.Shared.Models;
public class Z80WindingCode : WindingCode
{
}

public class PcWindingCode : WindingCode
{
}

public class WindingCode : IWindingCode
{
	public int Id { get; set; }
	public required string Code { get; set; }
	public Division Division { get; set; }
	public required string Name { get; set; }
	public string? FolderPath { get; set; }
	public CodeTypeId CodeTypeId { get; set; }
	public Media Media { get; set; } = null!;
	public CodeType? CodeType { get; set; }
}

public interface IWindingCode
{
	public int Id { get; set; }
	public string Code { get; set; }
	public Division Division { get; set; }
	public string Name { get; set; }
	public string? FolderPath { get; set; }
	public CodeTypeId CodeTypeId { get; set; }
	public Media Media { get; set; }
	[JsonIgnore] public CodeType? CodeType { get; set; }
}

public enum WindingCodeType
{
	Z80,
	Pc
}

public enum Division
{
	All = 0,
	D1 = 1,
	D2 = 2,
	D3 = 3
}
