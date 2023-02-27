using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MudBlazorPWA.Shared.Hubs;

public interface IChatHub
{
  Task NewMessage(string user, string message);
}

/// <inheritdoc />
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
    var clientIp = this.GetHubCallerIp();
    if (clientIp is null) {
      _logger.LogWarning("Client IP is null");
      return;
    }
    // get the name of this hub using reflection
    var hubName = this.GetType().Name;
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName: clientIp);
    HubExtensions.ActiveConnections.Add((hubName, clientIp));
    _logger.LogInformation("Client {ClientId} connected to group {GroupName}", Context.ConnectionId, clientIp);
  }
  public override async Task OnDisconnectedAsync(Exception exception) {
    var clientIp = this.GetHubCallerIp();
    var hubName = this.GetType().Name;
    if (clientIp is null) {
      _logger.LogWarning("Client IP is null");
      return;
    }
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName: clientIp);
    HubExtensions.ActiveConnections.Remove((hubName, clientIp));
    _logger.LogInformation("Client {ClientIpAddress} disconnected from group {GroupName}", clientIp, clientIp);
  }
  #endregion

  public Task<(string, List<string>)> GetConnectedGroups() {
    var hubName = this.GetType().Name;
    var connectedGroups = HubExtensions.ActiveConnections
     .Where(x => x.Item1 == hubName)
     .Select(x => x.Item2)
     .ToList();
   return Task.FromResult((hubName, connectedGroups));

  }

  public virtual  Task<List<string>> GetCallbackMethods() {
    // return a list<string> of methods defined  in the interface
    var methods = typeof(IChatHub).GetMethods().Select(x => x.Name).ToList();
    return Task.FromResult(methods);
  }
  public async Task SendMessage(string user, string message, string? groupIp = null){
    var clientIp = this.GetHubCallerIp();
    _logger.LogInformation("Sending message to group {GroupIp}", groupIp);
    if (groupIp is null) {
      await Clients.All.NewMessage(user, message);
      return;
    }
    _logger.LogInformation("IP from Parameter: {GroupIp}", groupIp);
      await Clients.Groups(clientIp).NewMessage(user, message);
  }
}
