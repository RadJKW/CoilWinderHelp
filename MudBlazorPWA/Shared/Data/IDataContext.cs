using Microsoft.EntityFrameworkCore;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Data;
public interface IDataContext
{
    public DbSet<WindingCode> WindingCodes { get; }
    public IEnumerable<CodeType> CodeTypes { get; }
}
