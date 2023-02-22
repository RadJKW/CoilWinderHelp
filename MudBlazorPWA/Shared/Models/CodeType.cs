using System.ComponentModel.DataAnnotations;

namespace MudBlazorPWA.Shared.Models;
public class CodeType
{
	[Key]
	public CodeTypeId CodeTypeId { get; set; }
	public string? Name { get; set; }

}
