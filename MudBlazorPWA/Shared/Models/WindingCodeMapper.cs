using Riok.Mapperly.Abstractions;

namespace MudBlazorPWA.Shared.Models;
[Mapper]
public partial class WindingCodeMapper
{
	public partial Z80WindingCode MapToZ80(IWindingCode source);
	public partial PcWindingCode MapToPc(IWindingCode source);
}
