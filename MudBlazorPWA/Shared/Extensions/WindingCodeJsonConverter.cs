using System.Text.Json;
using System.Text.Json.Serialization;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Shared.Extensions;

public class WindingCodeJsonConverter : JsonConverter<IWindingCode>
{
	public override IWindingCode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
	{
		// Deserialize as WindingCode
		return JsonSerializer.Deserialize<WindingCode>(ref reader, options);
	}

	public override void Write(Utf8JsonWriter writer, IWindingCode value, JsonSerializerOptions options)
	{
		// Serialize as WindingCode
		JsonSerializer.Serialize(writer, (WindingCode)value, options);
	}
}
