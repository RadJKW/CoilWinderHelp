namespace MudBlazorPWA.Shared.Models;
public class CodeType
{
	public CodeType(CodeTypeId codeTypeId, string name) {
		CodeTypeId = codeTypeId;
		Name = name;
	}
	public CodeTypeId CodeTypeId { get; set; }
	public string Name { get; set; }

}
