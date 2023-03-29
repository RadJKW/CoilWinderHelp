using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorPWA.Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MudBlazorPWA.Shared.Persistence.EntityConfigurations;
public class WindingCodeConfiguration : IEntityTypeConfiguration<WindingCode>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions = new() {
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = {
			new JsonStringEnumConverter()
		}
	};

	public void Configure(EntityTypeBuilder<WindingCode> builder) {
		// Entity Configuration
		builder.HasKey(w => w.Id);
		builder.HasIndex(w => new {
			w.Code,
			w.Division
		}).IsUnique();
		builder.HasOne(w => w.CodeType)
			.WithMany()
			.HasForeignKey(w => w.CodeTypeId);
		// owner of Media entity
		builder.OwnsOne<Media>(w => w.Media, mediaBuilder => {
			mediaBuilder.Property(m => m.Video)
				.HasMaxLength(255).HasColumnName("Video");
			mediaBuilder.Property(m => m.Pdf)
				.HasMaxLength(255).HasColumnName("Pdf");
			mediaBuilder.Property(m => m.RefMedia)
				.HasColumnName("RefMedia")
				.HasConversion(
				files =>
					JsonSerializer.Serialize
							(files, _jsonSerializerOptions),
				filesJson =>
					JsonSerializer.Deserialize<List<string>>
							(filesJson, _jsonSerializerOptions) ?? new List<string>(),
				new ValueComparer<List<string>>(
				(c1, c2) =>
					c2 != null && c1 != null && c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) =>
					HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()));
		});

		// Property Configuration
		// Code
		builder.Property(w => w.Code)
			.IsRequired()
			.HasMaxLength(10);

		// Division
		builder
			.Property(w => w.Division)
			.IsRequired()
			.HasConversion<int>();

		// Name
		builder
			.Property(w => w.Name)
			.IsRequired()
			.HasMaxLength(50)
			.HasColumnName("Stop");

		builder.Property(w => w.FolderPath).HasMaxLength(255);
	}
}
