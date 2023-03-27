namespace MudBlazorPWA.Shared.Interfaces;
public interface IChatHub
{
	Task NewMessage(string user, string message);
}
