﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Services;
public enum HubServers
{
	DirectoryHub,
	ChatHub
}

public class HubClientService
{
	public event Action<string[]>? ReceiveAllFolders;
	public event Action<string, string[]?, string[]?>? ReceiveFolderContent;
	public event Action? WindingCodesDbUpdated;
	public event EventHandler<IWindingCode>? CurrentWindingStopUpdated;

	public event Action? WindingCodeTypeChanged;
	public event Action<string, string>? NewChatMessage;
	public HubClientService(NavigationManager navigationManager, ILogger<HubClientService> logger) {
		_navigationManager = navigationManager;
		_logger = logger;
		InitializeDirectoryHub();
		InitializeChatHub();
		FileServerUrl = _navigationManager
			.ToAbsoluteUri("/files/");
		GetServerDocsFolder();
	}

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<HubClientService> _logger;
	private readonly NavigationManager _navigationManager;
	private Uri? FileServerUrl { get; init; }
	public HubConnection DirectoryHub { get; private set; } = null!;
	private HubConnection ChatHub { get; set; } = null!;

	private WindingCodeType _windingCodeType = WindingCodeType.Z80;
	public WindingCodeType WindingCodeType {
		get => _windingCodeType;
		set {
			_windingCodeType = value;
			WindingCodeTypeChanged?.Invoke();
		}
	}
	public string WindingDocsFolder { get; private set; } = string.Empty;
// TODO: GetHubConnection is redundant, remove it
	public HubConnection GetHubConnection(HubServers hubServer) {
		return hubServer switch {
			HubServers.DirectoryHub => DirectoryHub,
			HubServers.ChatHub => ChatHub,
			_ => throw new ArgumentOutOfRangeException(nameof(hubServer), hubServer, null)
		};
	}
	private async void GetServerDocsFolder() {
		WindingDocsFolder = await DirectoryHub.InvokeAsync<string>("GetServerWindingDocsFolder");
	}
	private async void InitializeDirectoryHub() {
		DirectoryHub = new HubConnectionBuilder()
			.WithUrl(_navigationManager.ToAbsoluteUri("/directoryHub"))
			.WithAutomaticReconnect()
			.AddJsonProtocol(options =>
			{
				options.PayloadSerializerOptions.Converters.Add(new WindingCodeJsonConverter());
			})
			.Build();

		RegisterHubEvents(DirectoryHub);
		await DirectoryHub.StartAsync();
		await DirectoryHub.InvokeAsync<string>("GetServerWindingDocsFolder");
	}
	private void InitializeChatHub() {
		ChatHub = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithUrl(_navigationManager.ToAbsoluteUri("/chatHub"))
			.Build();

		ChatHub.On<string, string>(nameof(IChatHub.NewMessage), (user, message) => {
			NewChatMessage?.Invoke(user, message);
		});

		ChatHub.StartAsync();
	}
	private void RegisterHubEvents(HubConnection hubConnection) {
		hubConnection.On<string[]>("ReceiveAllFolders", folders => {
			ReceiveAllFolders?.Invoke(folders);
		});
		hubConnection.On<string, string[]?, string[]?>(
		"ReceiveFolderContent",
		(path, files, folders) => {
			ReceiveFolderContent?.Invoke(path, files, folders);
		});
		hubConnection.On("WindingCodesDbUpdated", () => { WindingCodesDbUpdated?.Invoke(); });
		hubConnection.On<IWindingCode>("CurrentWindingStopUpdated", ParseWindingCodeMedia);
	}

	#region DirectoryInfo
	public async Task<IEnumerable<string>> GetFoldersInPath(string? path = null) {
		var folders = await DirectoryHub.InvokeAsync<IEnumerable<string>>("GetAllFolders", path);
		return folders;
	}

	public async Task<(List<string>, List<string>)> ListMediaFiles(string? path = null) {
		var pdfFiles = await DirectoryHub.InvokeAsync<List<string>>("ListPdfFiles", path);
		var videoFiles = await DirectoryHub.InvokeAsync<List<string>>("ListVideoFiles", path);
		return (pdfFiles, videoFiles);
	}
	public async Task<List<string>> ListPdfFiles(string? path = null) {
		var files = await DirectoryHub.InvokeAsync<List<string>>("ListPdfFiles", path);
		return files;
	}

	public async Task<List<string>> ListVideoFiles(string? path = null)
	{
		var files = await DirectoryHub.InvokeAsync<List<string>>("ListVideoFiles", path);
		return files;
	}
	#endregion

	public async Task<IWindingCode?> GetCurrentCoilWinderStop() {
		return await DirectoryHub.InvokeAsync<IWindingCode>("GetCurrentWindingStop");
	}
	public async Task SendChatMessage(string user, string message) {
		await ChatHub.InvokeAsync("SendMessage", user, message, null);
	}
	public async void SetCurrentCoilWinderStop(IWindingCode code) {
		await DirectoryHub.InvokeAsync("UpdateCurrentWindingStop", code);
	}
	private void ParseWindingCodeMedia(IWindingCode code) {
		if (code.Media.Video != null)
			code.Media.Video = FileServerUrl + code.Media.Video;
		if (code.Media.Pdf != null)
			code.Media.Pdf = FileServerUrl + code.Media.Pdf;
		// append 'FileServerUrl' to each item in code.Media.RefMedia list
		if (code.Media.RefMedia != null && code.Media.RefMedia.Any())
			// iterate through each item in the list
			for (var i = 0; i < code.Media.RefMedia.Count; i++)
				// append 'FileServerUrl' to each item in the list
				code.Media.RefMedia[i] = FileServerUrl + code.Media.RefMedia[i];

		CurrentWindingStopUpdated?.Invoke(this, code);
	}

	#region WindingCodeDB Crud
	public async Task<IEnumerable<IWindingCode>?> GetCodeList(Division? division = null) {
		var windingCodesList = await DirectoryHub.InvokeAsync<List<IWindingCode>?>("GetWindingCodes", division, WindingCodeType);
		return windingCodesList ?? null;
	}
	public async Task<IWindingCode?> GetWindingCode(int codeId) {
		return await DirectoryHub.InvokeAsync<IWindingCode>("GetWindingCode", codeId, WindingCodeType);
	}
	public async Task<bool> AddWindingCode(IWindingCode code) {
		return await DirectoryHub.InvokeAsync<bool>("CreateWindingCode", code, WindingCodeType);
	}
	public async Task<bool> UpdateWindingCodeDb(IWindingCode code) {
		return await DirectoryHub.InvokeAsync<bool>("UpdateWindingCode", code, WindingCodeType);
	}
	public async Task<IWindingCode?> DeleteWindingCode(IWindingCode code) {
		return await DirectoryHub.InvokeAsync<IWindingCode>("DeleteWindingCode", code, WindingCodeType);
	}
	#endregion

}
