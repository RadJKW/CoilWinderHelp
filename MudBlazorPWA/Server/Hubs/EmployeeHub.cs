using Microsoft.AspNetCore.SignalR;
using MudBlazorPWA.Server.Extensions;
using MudBlazorPWA.Shared.Models;
using WebSrvcCall;
namespace MudBlazorPWA.Server.Hubs;
public interface IEmployeeHubEvents {
	Task EmployeeAuthenticated();
}
public class EmployeeHub : Hub<IEmployeeHubEvents> {
	private readonly SQLMethods _employeeDb = new();
	private readonly ILogger<EmployeeHub> _logger;

	public EmployeeHub(ILogger<EmployeeHub> logger) {
		_logger = logger;
	}

	public async Task<Employee> ValidateEmployee(string employeeId) {
		_logger.LogInformation("Validating employee {EmployeeId}", employeeId);
		EmployeeInfo employeeInfo = new EmployeeInfo {
			EmployeeNumber = employeeId
		};
		var isValid = await Task.Run(() =>
			_employeeDb.ValidEmployee(ref employeeInfo));

		var employee = new Employee(employeeInfo, isValid);
			_logger.LogInformation("Employee: {EmployeeId}, Valid: {Valid}", employeeId, isValid);

		return employee;


	}

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

	public override Task OnDisconnectedAsync(Exception? exception) {

		string? clientIp = HubExtensions.GetConnectionIp(Context);
		string hubName = GetType().Name;
		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return Task.CompletedTask;
		}
		Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName: clientIp);
		HubExtensions.ActiveConnections[hubName].RemoveAll(x => x.Item2 == Context.ConnectionId);
		_logger.LogInformation("Client {ClientIpAddress} disconnected from group {GroupName}", clientIp, clientIp);
		return Task.CompletedTask;

	}
	#endregion
}
