using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class OperatorState {

	private readonly EmployeeService _employeeService;
	private readonly ILogger<OperatorState> _logger;

	public OperatorState(ILogger<OperatorState> logger, EmployeeService employeeService) {
		_logger = logger;
		_employeeService = employeeService;
	}

	public event Action? StateChanged;
	public event Action? OperatorLoggedIn;
	public Employee? CurrentEmployee { get; set; }

	public async Task<Employee> ValidateEmployee(Employee employee) {
		var employeeId = employee.EmployeeInfo.EmployeeNumber;
		var employeeInfo = await _employeeService.ValidateEmployee(employeeId);
		_logger.LogInformation("Validating employee {EmployeeId}", employeeId);
		return employeeInfo;
	}

	public async Task<Employee?> ValidateEmployee(string employeeId) {
		var employeeInfo = await _employeeService.ValidateEmployee(employeeId);
		_logger.LogInformation("Validating employee {EmployeeId}", employeeId);
		return employeeInfo.IsValid
			? employeeInfo
			: null;
	}



	public void Login(Employee employee) {
		CurrentEmployee = employee;
		NotifyOperatorLoggedIn();
	}

	private void NotifyOperatorLoggedIn() => OperatorLoggedIn?.Invoke();
	private void NotifyStateHasChanged() => StateChanged?.Invoke();
}
