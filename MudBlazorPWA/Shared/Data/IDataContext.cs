using Microsoft.EntityFrameworkCore;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Data;
public interface IDataContext
{
    public DbSet<Z80WindingCode> Z80WindingCodes { get; }
    public DbSet<PcWindingCode> PcWindingCodes { get; }
    public IEnumerable<CodeType> CodeTypes { get; }


    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
