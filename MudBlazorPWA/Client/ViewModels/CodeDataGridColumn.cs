using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.ViewModels;
public class CodeDataGridColumn<TItem, TProperty> where TItem : class
{
	public required Expression<Func<TItem, TProperty>> Property { get; set; }
	public Func<TProperty, string>? Formatter { get; set; }
	public RenderFragment<WindingCode>? EditTemplate { get; set; }

	public Func<WindingCode, bool>? VisibilityFunction { get; set; }

	public bool IsEditable { get; set; } = true;
	public bool Hideable { get; set; }
	public bool Hidden { get; set; }
	public string? Title { get; set; }
}



#region ExampleUsage (NotWorking)
#if EXAMPLE
  List<CodeDataGridColumn<WindingCode, object>> _columns = new() {
    new() {
      Property = x => x.Id,
      Title = "Id",
      IsEditable = false,
    },
    new () {
      Property = x => x.Division.ToString(),
      Title = "Dept",
      Hideable = true,
    },
    new() {
      Property = x => x.Code,
      Title = "Code",
    },
    new() {
      Property = x => x.Name,
      Title = "Name",
      EditTemplate = (x) =>
        @<MudTextField T="string"
             @bind-Value="@x.Name"
             Variant="Variant.Outlined"
             Margin="Margin.Dense"
             Label="Name"
             Placeholder="Enter Name"/>
    },
    new() {
      Property = x => x.CodeTypeId.ToString(),
      Title = "CodeType",
      IsEditable = false,
      Hideable = true,
      EditTemplate = (x) =>
        @<MudSelectExtended T="CodeTypeId"
             Class="mt-4"
             RelativeWidth="true"
             ItemCollection="@(Enum.GetValues<CodeTypeId>().ToList())"
             @bind-Value="@x.CodeTypeId"
             Margin="Margin.Dense"
             Dense="true"
             Variant="Variant.Outlined"
             Label="CodeType">
        </MudSelectExtended>
    },
    new() {
      Property = x => x.FolderPath!,
      Hidden = true,
      EditTemplate = (x) =>
        @<MudSelectExtended T="string"
             Context="selectContext"
             ItemCollection="@(FolderPathsCollection.OrderByDirection(SortDirection.Ascending, item => item).ToList())"
             @bind-Value="@x.FolderPath"
             Variant="Variant.Filled"
             Clearable="true"
             OnClearButtonClick="@(() => x.FolderPath = null)"
             Color="Color.Dark"
             Label="Assigned Folder"
             Placeholder="Select a folder"
             PopoverClass="folder-select-popover"
             SearchBox="true"
             SearchBoxAutoFocus="true"
             DisablePopoverPadding="true">
          <ItemTemplate>
            <MudText Typo="Typo.button"> @selectContext.Value</MudText>
          </ItemTemplate>
        </MudSelectExtended>

    }
  };
#endif
#endregion
