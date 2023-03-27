using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using MudBlazorPWA.Server.Extensions;
using MudBlazorPWA.Shared.Interfaces;

namespace MudBlazorPWA.Server.Hubs;
// ReSharper disable UnusedMember.Global

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
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return;
		}

		// Get the name of this hub using reflection
		string hubName = GetType().Name;

		// Add the connection to the group
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName: clientIp);

		// Add the connection to the list of active connections
		if (!HubExtensions.ActiveConnections.TryGetValue(hubName, out var connections)) {
			connections = new List<(string, string)>();
			HubExtensions.ActiveConnections.Add(hubName, connections);
		}
		connections.Add((clientIp, Context.ConnectionId));

		_logger.LogInformation("Client {ClientId} connected to group {GroupName}", Context.ConnectionId, clientIp);
	}
	public override async Task OnDisconnectedAsync(Exception? exception) {
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		string hubName = GetType().Name;
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
		// var clientIp = HubExtensions.GetConnectionIp(Context);
		string hubName = GetType().Name;
		var clients = HubExtensions.ActiveConnections[hubName].Select(x => x.ip).ToList();
		// use console write line to see each entry in the dictionary in the console
		foreach (string ip in clients) {
			Console.WriteLine($"IP: {ip}");
		}
		return Task.FromResult(clients);
	}

	public async Task<List<string>> GetCallbackMethods() {
		Type hubType = GetType();
		var methods = hubType.GetMethods()
			.Where(m =>
				(m.Attributes & MethodAttributes.Virtual) == 0 && // exclude overridden methods
				m.ReturnType == typeof(Task) ||                   // include public Task methods
				(m.ReturnType == typeof(void) && m.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null) // include public async void methods
			)
			.Select(m => m.Name)
			.ToList();
		await Clients.All.NewMessage("Server", "Hello from the server");
		return methods;
	}

	public async Task SendMessage(string user, string message, string? groupIp = null) {
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		_logger.LogInformation("Sending message to group {GroupIp}", groupIp);
		switch (groupIp) {
			case null:
				await Clients.All.NewMessage(user, message);
				break;
			default:
				await Clients.Groups(groupIp, clientIp!).NewMessage(user, message);
				break;
		}

		_logger.LogInformation("IP from Parameter: {GroupIp}", groupIp);
	}
}
