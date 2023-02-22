using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MudBlazorPWA.Shared.Models;
[Owned]
public class Media
{
	public string? Video { get; set; }
	public string? Pdf { get; set; }
	public string? ReferenceFolder { get; set; }
}
