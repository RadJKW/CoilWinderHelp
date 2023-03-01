namespace MudBlazorPWA.Shared.Hubs;
 #pragma warning disable CA2211

public static class HubInfo
{

public static class Strings
{
	public static string ChatHubUrl = "http://192.168.0.10:5126/chatHub";
	public static string DirectoryHubUrl = "http://192.168.0.10:5126/directoryHub";
}

public static class Events
{
	public static string FolderContent  => nameof(IHubClient.ReceiveFolderContent);
	public static string FileSelected => nameof(IHubClient.FileSelected);
	public static string NewWindingStop => nameof(IHubClient.CurrentWindingStopUpdated);
	public static string NewChatMessage => nameof(IChatHub.NewMessage);
}

public static class Actions
{
	public static string SendMessage => nameof(ChatHub.SendMessage);
	public static string GetFolderContent => nameof(DirectoryHub.GetFolderContent);
	public static string GetWindingCodes => nameof(DirectoryHub.GetWindingCodes);
}
}

 #pragma warning restore CA2211
