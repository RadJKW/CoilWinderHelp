using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class EmployeeService {
	private readonly ILogger<EmployeeService> _logger;
	private HubConnection EmployeeHub { get; set; } = null!;
	private readonly NavigationManager _navigationManager;

	public EmployeeService(NavigationManager navigationManager, ILogger<EmployeeService> logger) {
		_navigationManager = navigationManager;
		_logger = logger;
		InitializeEmployeeHub();
	}
	private async void InitializeEmployeeHub() {
		var hubUrl = _navigationManager.ToAbsoluteUri("/employeehub");
		EmployeeHub = new HubConnectionBuilder()
			.WithUrl(hubUrl)
			.WithAutomaticReconnect()
			.Build();

		await EmployeeHub.StartAsync();
		_logger.LogInformation("EmployeeHub started");
	}

	public async Task<Employee> ValidateEmployee(string employeeId) {
		var employeeInfo = await EmployeeHub.InvokeAsync<Employee>("ValidateEmployee", employeeId);

		return employeeInfo;
	}

}
