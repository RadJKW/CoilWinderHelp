using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace MudBlazorPWA.Shared.Hubs;
public static class HubExtensions
{

	public static readonly Dictionary<string, List<(string ip , string contextId)>> ActiveConnections = new();

	public static string? GetConnectionIp(HubCallerContext context)
	{
		var connection = context.Features.Get<IHttpConnectionFeature>();
		var remoteIpAddress = connection?.RemoteIpAddress;

		// Check if the address is IPv6
		if (remoteIpAddress is not { IsIPv4MappedToIPv6: true })
			return remoteIpAddress?.ToString();

		// Convert the IPv6 address to IPv4 format
		var bytes = remoteIpAddress.MapToIPv4().GetAddressBytes();
		return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";

	}

}
