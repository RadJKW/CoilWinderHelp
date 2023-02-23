using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Persistence.EntityConfigurations;
public class CodeTypeConfiguration : IEntityTypeConfiguration<CodeType>
{
	public void Configure(EntityTypeBuilder<CodeType> builder) {

		builder.HasKey(c => c.CodeTypeId);
		builder.Property(c => c.CodeTypeId).HasConversion<int>();

		builder.Property(c => c.Name)
			.IsRequired()
			.HasMaxLength(50)
			.HasColumnName("CodeType");

		builder.HasData(Enum.GetValues(typeof(CodeTypeId))
			.Cast<CodeTypeId>()
			.Select(e =>
				new CodeType(e, e.ToString())
			)
		);
	}
}
