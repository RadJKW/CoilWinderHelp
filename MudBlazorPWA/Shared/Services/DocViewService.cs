using Blazored.LocalStorage;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Services;
public class DocViewService
{
	public event Action MajorUpdateOccured = default!;
	private void OnMajorUpdateOccured() => MajorUpdateOccured.Invoke();


	public AppSettings Settings { get; private set; } = new();

	public void SetAppSettings(AppSettings appSettings) {
		Settings = appSettings;
		OnMajorUpdateOccured();
	}

	public T GetAppSetting<T>(Enum property) => GetAppSetting<T>(property.ToString());

	private T GetAppSetting<T>(string propertyName) {
		var property = Settings.GetType().GetProperty(propertyName);
		return (T)property?.GetValue(Settings)!;
	}

	public void SetAppSetting<T>(Enum property, T value) => SetAppSetting(property.ToString(), value);

	private void SetAppSetting<T>(string propertyName, T value) {
		var settingsType = Settings.GetType();
		var property = settingsType.GetProperty(propertyName);
		if (property is null)
			return;
		// if property is AppSettings.DrawerOpenOnHover and its value is true, set DrawerOpen to false
		if (property.Name == nameof(AppSettings.DrawerOpenOnHover)) {
			// set the DrawerOpen property to opposite of its current value
			var drawerOpenProperty = settingsType.GetProperty(nameof(AppSettings.DrawerOpen));
			drawerOpenProperty?.SetValue(Settings, !(bool)drawerOpenProperty.GetValue(Settings)!);
		}

		property.SetValue(Settings, value);
		SaveSettingsAsync();
		OnMajorUpdateOccured();
	}

	private ILocalStorageService LocalStorageService { get; }

	public DocViewService(ILocalStorageService localStorageService) {
		LocalStorageService = localStorageService;
	}

	public async Task LoadSettingsAsync() {
		foreach (var property in Settings.GetType().GetProperties()) {
			if ( await LocalStorageService.ContainKeyAsync(property.Name) == false) {
				// set the property to its default value
				property.SetValue(Settings, property.GetValue(Settings));
				// save the property to local storage
				await LocalStorageService.SetItemAsync(property.Name, property.GetValue(Settings));
				continue;
			}
			var value = await LocalStorageService.GetItemAsync<bool>(property.Name);
			property.SetValue(Settings, value);
		}
	}

	public async void SaveSettingsAsync() {
		foreach (var property in Settings.GetType().GetProperties()) {
			var value = property.GetValue(Settings);
			if (value is not null) {
				await LocalStorageService.SetItemAsync(property.Name, value);
			}
		}
	}
}
