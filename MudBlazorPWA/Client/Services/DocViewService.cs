using System.Reflection;
using Blazored.LocalStorage;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Services;
public class DocViewService
{
	private string _appBarTitle = "MudBlazorPWA";
	public string AppBarTitle {
		get => _appBarTitle;
		set {
			_appBarTitle = value;
			AppBarTitleChanged.Invoke();
		}
	}

	public event Action AppBarTitleChanged = default!;
	public event Action MajorUpdateOccured = default!;
	private void OnMajorUpdateOccured() => MajorUpdateOccured.Invoke();

	public AppSettings Settings { get; private set; } = new(false, true, true);

	public void SetAppSettings(AppSettings appSettings) {
		Settings = appSettings;
		OnMajorUpdateOccured();
	}

	public T GetAppSetting<T>(Enum property) => GetAppSetting<T>(property.ToString());

	private T GetAppSetting<T>(string propertyName) {
		PropertyInfo? property = Settings.GetType().GetProperty(propertyName);
		return (T)property?.GetValue(Settings)!;
	}

	public void SetAppSetting<T>(Enum property, T value) => SetAppSetting(property.ToString(), value);

	private void SetAppSetting<T>(string propertyName, T value) {
		Type settingsType = Settings.GetType();
		PropertyInfo? property = settingsType.GetProperty(propertyName);
		if (property is null)
			return;
		var valueAsBool = (bool)(object)value!;

		// toggles 'drawerOpen' when 'drawerOpenOnHover' is toggled
		// this avoids hover from being stuck open if the user toggles the setting
		if (property.Name == nameof(AppSettings.DrawerOpenOnHover)) {
			PropertyInfo? drawerOpenProperty = settingsType.GetProperty(nameof(AppSettings.DrawerOpen));
			drawerOpenProperty!.SetValue(Settings, !valueAsBool);
		}

		property.SetValue(Settings, value);
		SaveSettingsAsync();
		OnMajorUpdateOccured();
	}

	private readonly ILocalStorageService _localStorage;

	public DocViewService(ILocalStorageService localStorage) {
		_localStorage = localStorage;
	}

	public async Task LoadSettingsAsync() {
		foreach (PropertyInfo property in Settings.GetType().GetProperties()) {
			if ( await _localStorage.ContainKeyAsync(property.Name) == false) {
				// set the property to its default value
				property.SetValue(Settings, property.GetValue(Settings));
				// save the property to local storage
				await _localStorage.SetItemAsync(property.Name, property.GetValue(Settings));
				continue;
			}
			var value = await _localStorage.GetItemAsync<bool>(property.Name);
			property.SetValue(Settings, value);
		}
	}
	public async void SaveSettingsAsync() {
		foreach (PropertyInfo property in Settings.GetType().GetProperties()) {
			object? value = property.GetValue(Settings);
			if (value is not null) {
				await _localStorage.SetItemAsync(property.Name, value);
			}
		}
	}
}
