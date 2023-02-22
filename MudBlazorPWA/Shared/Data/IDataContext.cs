using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Data;
public interface IDataContext
{
    public DbSet<WindingCode> WindingCodes { get; }
    public IEnumerable<CodeType> CodeTypes { get; }


    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
