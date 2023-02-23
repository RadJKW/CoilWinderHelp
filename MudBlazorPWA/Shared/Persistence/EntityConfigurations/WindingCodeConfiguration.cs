using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Persistence.EntityConfigurations;
public class WindingCodeConfiguration : IEntityTypeConfiguration<WindingCode>
{
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
			mediaBuilder.Property(m => m.ReferenceFolder)
				.HasMaxLength(255).HasColumnName("RefMedia");
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
