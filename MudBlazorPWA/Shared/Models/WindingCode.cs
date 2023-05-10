using System.Text.Json;
using System.Text.Json.Serialization;

namespace MudBlazorPWA.Shared.Models;
public class Z80WindingCode : WindingCode {
}

public class PcWindingCode : WindingCode {
}

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

	public void RemoveAssignedItem(AssignedItem key, string? value) {
		switch (key) {
			case AssignedItem.Directory:
				FolderPath = null;
				break;
			case AssignedItem.Pdf:
				Media.Pdf = null;
				break;
			case AssignedItem.Video:
				Media.Video = null;
				break;
			case AssignedItem.RefMedia:
				if (string.IsNullOrEmpty(value)) break;
				Media.RefMedia?.Remove(value);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(key), key, null);
		}
	}
	public void EditAssignedItem(AssignedItem key, string? value) {
		Console.WriteLine("AddAssignedItem");
		Console.WriteLine($" key: {key}, value: {value}");
		switch (key) {
			case AssignedItem.Directory:
				FolderPath = value;
				break;
			case AssignedItem.Pdf:
				Media.Pdf = value;
				break;
			case AssignedItem.Video:
				Media.Video = value;
				break;
			case AssignedItem.RefMedia:
				if (string.IsNullOrEmpty(value)) break;
				Media.RefMedia ??= new();
				Media.RefMedia.Add(value);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(key), key, null);
		}
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

public enum AssignedItem {
	Directory,
	Pdf,
	Video,
	RefMedia
}
