using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace MudBlazorPWA.Shared.Hubs;
public static class HubExtensions
{

	public static readonly List<(string, string)> ActiveConnections = new();

	public static string? GetConnectionIp(HubCallerContext context) {
		var connection = context.Features.Get<IHttpConnectionFeature>();
		return
			connection?
				.RemoteIpAddress?
				.ToString()
				.Replace("::ffff:", string.Empty);
	}


	public static string? GetHubCallerIp(this Hub hub) {

		var connection = hub.Context.Features.Get<IHttpConnectionFeature>();
		var ip = connection?.RemoteIpAddress?.ToString().Replace("::ffff:", string.Empty);
		return ip;
	}
}
