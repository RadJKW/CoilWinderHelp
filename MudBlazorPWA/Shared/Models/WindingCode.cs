using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MudBlazorPWA.Shared.Models;
public class WindingCode
{
	[Key]
	public int Id { get; set; }
	[MaxLength(10)]
	public string? Code { get; set; }
	[MaxLength(50)]
	[Display(Name = "Stop")]
	public string? Name { get; set; }
	public string? D1FolderPath { get; set; }
	public string? D2FolderPath { get; set; }
	public string? D3FolderPath { get; set; }

	public CodeTypeId CodeTypeId { get; set; }

	public Media Media { get; set; } = new();

	[JsonIgnore]
	public CodeType? CodeType { get; set; }
}
