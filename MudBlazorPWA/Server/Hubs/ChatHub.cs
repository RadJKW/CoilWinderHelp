using Microsoft.AspNetCore.SignalR;

namespace MudBlazorPWA.Server.Hubs;
public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
