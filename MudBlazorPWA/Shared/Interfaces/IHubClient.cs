using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Shared.Interfaces;
public interface IHubClient {
	Task WindingCodesDbUpdated();

	Task CurrentWindingStopUpdated(WindingCode code);
}
