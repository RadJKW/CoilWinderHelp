using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Data;
public class DataContext : DbContext, IDataContext
{
	public DataContext(DbContextOptions<DataContext> options) : base(options) {
	}

	public DbSet<WindingCode> WindingCodes => Set<WindingCode>();
	public IEnumerable<CodeType> CodeTypes => Set<CodeType>();

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		modelBuilder.Entity<WindingCode>()
			.HasOne(w => w.CodeType)
			.WithMany()
			.HasForeignKey(w => w.CodeTypeId);

		var codeTypeBuilder = modelBuilder.Entity<CodeType>();

		codeTypeBuilder
			.Property(c => c.CodeTypeId)
			.HasConversion<int>();


		codeTypeBuilder
			.Property(c => c.Name)
			.HasColumnName("CodeType");

		codeTypeBuilder
			.HasData(Enum.GetValues(typeof(CodeTypeId))
				.Cast<CodeTypeId>()
				.Select(e =>
					new CodeType {
						CodeTypeId = e,
						Name = e.ToString()
					})
			);
	}
}
