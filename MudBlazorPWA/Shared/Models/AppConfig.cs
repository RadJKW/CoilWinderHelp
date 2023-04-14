using System.Runtime.InteropServices;

namespace MudBlazorPWA.Shared.Models;
public static class AppConfig
{
	private static string WindowsPath => @"B:/CoilWinderTraining-Edit/";
	private static string MacPath => @"/Users/jkw/WindingPractices/";

	public static string BasePath => IsWindows ? WindowsPath : MacPath;

	public static string CorsPolicy => "AllowAll";

	public static bool UseInMemoryDatabase => false;

	// the file i want to use is in the parent folder (CoilWinderHelp) of this project folder (MudBlazorPWA)
	// use c# to get the path to the parent folder

	public static string JsonDataSeedFile => GetJsonDataSeedFile();

	private static string GetJsonDataSeedFile()
	{
		DirectoryInfo? projectDir = Directory.GetParent(Directory.GetCurrentDirectory());
		string solutionDir = projectDir?.Parent?.FullName!;
		string jsonFile = Path.Combine(solutionDir, "WindingCodes.json");
		Console.WriteLine("JSON File: " + jsonFile);
		return jsonFile;
	}

	// check to see if the OS is not Mac, then it must be windows
	private static bool IsWindows => !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
}
