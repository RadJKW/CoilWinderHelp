﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MudBlazorPWA.Shared.Models;
public class WindingCode
{
    [Key, MaxLength(10)]
    public string? Code { get; set; }
    [MaxLength(50)]
    [Display(Name="Stop")]
    public string? Name { get; set; }
    public string? FolderPath { get; set; }
    public CodeTypeId CodeTypeId { get; set; }

    [JsonIgnore]
    public CodeType? CodeType { get; set; }
}

public class CodeType
{
    [Key]
    public CodeTypeId CodeTypeId { get; set; }
    public string? Name { get; set; }

}

public enum CodeTypeId
    {

        Stop,
        Almost,
        Data,
        Layer,
        Material,
        None,

    }
