using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
// ReSharper disable UnusedMember.Global

namespace MudBlazorPWA.Shared.Hubs;
public interface IChatHub
{
	Task NewMessage(string user, string message);
}

/// <inheritdoc />
public class ChatHub : Hub<IChatHub>
{
	#region Constructor
	private readonly ILogger<ChatHub> _logger;
	public ChatHub(ILogger<ChatHub> logger) {
		_logger = logger;
	}
	#endregion

	#region Hub Overrides
	public override async Task OnConnectedAsync() {
		var clientIp = this.GetHubCallerIp();
		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return;
		}
		// get the name of this hub using reflection
		var hubName = this.GetType().Name;
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName: clientIp);
		// if the hubName is already in the dictionary, add the (clientIp, Context.ConnectionId) to the list
		// otherwise, create a new list with the (clientIp, Context.ConnectionId) and add it to the dictionary
		if (HubExtensions.ActiveConnections.TryGetValue(hubName, out var value)) {
      value.Add((clientIp, Context.ConnectionId));
		} else {
			HubExtensions.ActiveConnections.Add(hubName, new List<(string, string)> { (clientIp, Context.ConnectionId) });
		}
		_logger.LogInformation("Client {ClientId} connected to group {GroupName}", Context.ConnectionId, clientIp);
	}
	public override async Task OnDisconnectedAsync(Exception exception) {
		var clientIp = this.GetHubCallerIp();
		var hubName = this.GetType().Name;
		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return;
		}
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName: clientIp);
		HubExtensions.ActiveConnections[hubName].RemoveAll(x => x.Item2 == Context.ConnectionId);
		_logger.LogInformation("Client {ClientIpAddress} disconnected from group {GroupName}", clientIp, clientIp);
	}
	#endregion


	public Task<List<string>> GetConnectedClients() {
		// var clientIp = this.GetHubCallerIp();
		var hubName = this.GetType().Name;
		var clients = HubExtensions.ActiveConnections[hubName].Select(x => x.Item1).ToList();
		return Task.FromResult(clients);
	}

	public async Task<List<string>> GetCallbackMethods() {
		// return a list<string> of methods defined  in the interface
		var methods = typeof(ChatHub).GetMethods().Select(x => x.Name).ToList();
		await Task.FromResult(methods);
		await Clients.All.NewMessage("Server", "Hello from the server");
		return methods;
	}
	public async void SendMessage(string user, string message, string? groupIp = null) {
		var clientIp = this.GetHubCallerIp();
		_logger.LogInformation("Sending message to group {GroupIp}", groupIp);
		if (groupIp is null) {
			await Clients.Groups(clientIp).NewMessage(user, message);
		}
		else {
			await Clients.Groups(groupIp).NewMessage(user, message);
		}

		_logger.LogInformation("IP from Parameter: {GroupIp}", groupIp);

	}
}
