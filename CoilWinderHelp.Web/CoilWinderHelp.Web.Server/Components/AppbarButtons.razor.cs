using CoilWinderHelp.Web.Server.Services;
using Microsoft.AspNetCore.Components;

// ReSharper disable ClassNeverInstantiated.Global

namespace CoilWinderHelp.Web.Server.Components;
public partial class AppbarButtons
{
  [Inject] private LayoutService? LayoutService { get; set; }
  private IDictionary<string, bool>? _messages;
  private bool _newNotificationsAvailable = true;

  private Task MarkNotificationAsRead()
  {
    _newNotificationsAvailable = false;
    return Task.CompletedTask;
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      _messages = GetNotifications();
      StateHasChanged();
    }

    await base.OnAfterRenderAsync(firstRender);
  }
  private static IDictionary<string, bool> GetNotifications()
  {
    // return two new notifications
    var notifications = new Dictionary<string, bool>
   {
     {
       "New Notification 1", true
     },
     {
       "New Notification 2", true
     }
   };

    return notifications;
  }


}
