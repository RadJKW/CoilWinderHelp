using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPWA.Client.ViewModels;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Instructions.Pages;
public partial class AdminDashboard
{
	private MudDropContainer<DropItem> _dropContainer = default!;
  private readonly List<string> _folderPathsCollection = new();
  private readonly List<DropItem> _dropItems = new();
  private readonly List<IWindingCode> _windingCodesList = new();

  protected override async Task OnInitializedAsync() {
    HubClientService.WindingCodeTypeChanged += async () => await OnWindingCodeTypeChanged();
    await base.OnInitializedAsync();
    var getWindingCodesTask = HubClientService.GetCodeList();
    var getFolderPathsTask = HubClientService.GetFoldersInPath();

    await Task.WhenAll(getWindingCodesTask, getFolderPathsTask);

    if (getWindingCodesTask.Result != null) {
      _windingCodesList.AddRange(getWindingCodesTask.Result);
      _folderPathsCollection.AddRange(getFolderPathsTask.Result);
    }
  }
  private async Task OnWindingCodeTypeChanged() {
    _folderPathsCollection.Clear();
    _windingCodesList.Clear();
    await OnInitializedAsync();
    await InvokeAsync(StateHasChanged);
  }
  protected override async Task OnAfterRenderAsync(bool firstRender) {
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender) {
      await JSRuntime.InvokeVoidAsync("createCustomTooltip");
    }
  }
  void IDisposable.Dispose() {
    Snackbar.Dispose();
  }
  private void ItemUpdated(MudItemDropInfo<DropItem> dropInfo) {
    DropItem? dropItem = dropInfo.Item;
    string? targetZoneId = dropInfo.DropzoneIdentifier;

    var dropItemListWithCopies = _dropItems.Where(x => x.Name == dropItem.Name && x.IsCopy).ToList();


  // If the item can't be dropped, just return
    if (!CanDropItem(dropInfo))
      return;

    if (targetZoneId == dropItem.Identifier)
      return;

    switch (dropItem.IsCopy) {
      case true when targetZoneId == dropItem.OriginalIdentifier:
        _dropItems.Remove(dropItem);
        return;
      case true when targetZoneId != dropItem.OriginalIdentifier:
        dropItem.Identifier = targetZoneId;
        return;
      case true when dropItemListWithCopies.Any(x => x.Identifier == targetZoneId):
        _dropItems.Remove(dropItem);
        return;
      case false when dropItemListWithCopies.Any(x => x.Identifier == targetZoneId):
        return;

      default:
        var dropItemCopy = new DropItem {
          Name = dropItem.Name,
          Path = dropItem.Path,
          Type = dropItem.Type,
          Identifier = dropInfo.DropzoneIdentifier,
          OriginalIdentifier = dropItem.Identifier,
          IsCopy = true
        };
        DropItem? originalDropItem = _dropItems.FirstOrDefault(x => !x.IsCopy && x.Name == dropItem.Name);


        if (originalDropItem != null) {
          _dropItems.Insert(_dropItems.IndexOf(originalDropItem), dropItemCopy);
        } else {
          _dropItems.Add(dropItemCopy);
        }
        break;
    }
  }

  private bool CanDropItem(MudItemDropInfo<DropItem> dropInfo) {
    DropItem? dropItem = dropInfo.Item;
    string? targetZoneId = dropInfo.DropzoneIdentifier;

  // If the item is going back to its original destination
    if (dropItem.OriginalIdentifier == targetZoneId)
      return true;

    // If the item is a copy and is being dragged to another list where that item already exists
    return !dropItem.IsCopy || _dropItems.Any(x => x.Name == dropItem.Name && x.Identifier == targetZoneId) != true;
  }

  private Dictionary<string, object> GetDropItemAttributes(DropItem context) {
    var attributes = new Dictionary<string, object> {
      {
        "id", "drop-zone-chip"
      }, {
        "data-name", context.Name!
      }, {
        "data-path", context.Path!
      }
    };

    var copies = _dropItems.Where(d => d.Name == context.Name && d.IsCopy).ToList();
    if (!copies.Any())
      return attributes;

    string identifiers = string.Join("<br />", copies.Select(c => c.Identifier));
    attributes.Add("data-title", identifiers);

    return attributes;
  }
  private Task DropItemsUpdated(List<DropItem> arg) {
    if (_dropItems.Any()) {
      _dropItems.Clear();
    }

    _dropItems.AddRange(arg);
    return Task.CompletedTask;
  }

  /*private async Task OnDropItemsViewChanged(bool arg) {
    _dropItemsViewDense = arg;
    await InvokeAsync(StateHasChanged);
  }*/
}
