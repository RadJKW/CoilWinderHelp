using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Extensions;
public static class DirectoryExtensions
{
	public static string RelativePath(this string? path) {
		return path?.Replace(AppConfig.BasePath, "").Replace("\\", "/") ?? string.Empty;
	}

	public static bool IsPdf(this string? path) {
		return path?.EndsWith(".pdf") ?? false;
	}


}
