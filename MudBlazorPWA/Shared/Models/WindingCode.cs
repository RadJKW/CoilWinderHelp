using System.ComponentModel.DataAnnotations;

namespace MudBlazorPWA.Shared.Models;
public class WindingCode
{
    [Key, MaxLength(10)]
    public string? Code { get; set; }
    [MaxLength(50)]
    [Display(Name="Stop")]
    public string? Name { get; set; }
    [Required]
    public CodeType Type { get; set; }

    public string? FolderPath { get; set; }
}

public enum CodeType
{
    Stop,
    Almost,
    Data,
    Layer,
    Material,
}
