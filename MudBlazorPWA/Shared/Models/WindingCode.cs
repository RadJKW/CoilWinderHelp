﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MudBlazorPWA.Shared.Models;
public class Z80WindingCode : WindingCode {
}

public class PcWindingCode : WindingCode {
}

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class WindingCode : IWindingCode {
	public int Id { get; set; }
	public required string Code { get; set; }
	public Division Division { get; set; }
	public required string Name { get; set; }
	public string? FolderPath { get; set; }
	public CodeTypeId CodeTypeId { get; set; }
	public Media Media { get; set; } = null!;
	public CodeType? CodeType { get; set; }

	public WindingCode Clone() {
		var json = JsonSerializer.Serialize(this);
		return JsonSerializer.Deserialize<WindingCode>(json) ?? throw new NullReferenceException();
	}
}

public interface IWindingCode {
	public int Id { get; set; }
	public string Code { get; set; }
	public Division Division { get; set; }
	public string Name { get; set; }
	public string? FolderPath { get; set; }
	public CodeTypeId CodeTypeId { get; set; }
	public Media Media { get; set; }
	[JsonIgnore] public CodeType? CodeType { get; set; }
	public WindingCode Clone();
}

public enum WindingCodeType {
	Z80,
	Pc
}

public enum Division {
	All = 0,
	D1 = 1,
	D2 = 2,
	D3 = 3
}
