using Blazored.LocalStorage;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class EmployeeState {
	private readonly ILocalStorageService _localStorage;
	private readonly EmployeeService _employeeService;
	private readonly ILogger<EmployeeState> _logger;
	private const string Key = "Employee";

	public EmployeeState(ILogger<EmployeeState> logger, EmployeeService employeeService, ILocalStorageService localStorage) {
		_logger = logger;
		_employeeService = employeeService;
		_localStorage = localStorage;
	}


	public event Action? MajorUpdate;
	public Employee? CurrentEmployee {
		get;
		set;
	}


	public async Task<Employee?> ValidateEmployee(string employeeId) {
		var employeeInfo = await _employeeService.ValidateEmployee(employeeId);
		_logger.LogInformation("Validating employee {EmployeeId}", employeeId);
		return employeeInfo.IsValid
			? employeeInfo
			: null;
	}



	public async Task Login(Employee employee) {
		CurrentEmployee = employee;
		await SaveEmployeeState();
		OnMajorUpdate();
	}
	public async Task Logout() {
		CurrentEmployee = null;
		await SaveEmployeeState();
		OnMajorUpdate();
	}

	private void OnMajorUpdate() => MajorUpdate?.Invoke();

	private async Task SaveEmployeeState() {
		await _localStorage.SetItemAsync(Key, CurrentEmployee);
		_logger.LogInformation("Employee state saved");
	}

	public async Task LoadEmployeeState() {
		CurrentEmployee = await _localStorage.GetItemAsync<Employee>(Key);
	}

	// public event Action? StateChanged;
	// private void NotifyStateHasChanged() => StateChanged?.Invoke();
}
