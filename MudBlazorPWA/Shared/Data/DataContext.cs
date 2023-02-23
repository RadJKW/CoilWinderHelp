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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}
