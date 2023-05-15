using WebSrvcCall;
namespace MudBlazorPWA.Shared.Models;
public class Employee {
	public Employee(EmployeeInfo employeeInfo, bool isValid) { EmployeeInfo = employeeInfo; IsValid = isValid; }
	public EmployeeInfo EmployeeInfo { get; set; }
	public bool IsValid { get; set; }
}
