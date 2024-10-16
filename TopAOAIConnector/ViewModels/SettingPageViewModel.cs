﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using TopAOAIConnector.Models;

namespace TopAOAIConnector.ViewModels;

internal partial class SettingPageViewModel : ViewModelBase
{
    private const string directoryName = "TopAOAIConnector";
    private const string settingsFileName = "settings.json";
    private const string chatSystemRolesFileName = "chat-system-roles.json";

    private readonly string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName, settingsFileName);
    private readonly string chatSystemRolesFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName, chatSystemRolesFileName);

    public SettingPageViewModel()
    {
        if (File.Exists(settingsFilePath))
        {
            var jsonSettings = File.ReadAllText(settingsFilePath);
            var aoaiSettings = JsonSerializer.Deserialize<AOAISettings>(jsonSettings);
            Settings = aoaiSettings;
        }
        else
        {
            Settings = new();
        }

        if (Settings is not null)
        {
            AOAISettings.Instance = Settings;
        }

        if (File.Exists(chatSystemRolesFilePath))
        {
            var jsonSettings = File.ReadAllText(chatSystemRolesFilePath);
            var chatSystemRoles = JsonSerializer.Deserialize<ObservableCollection<ChatSystemRole>>(jsonSettings);

            if (chatSystemRoles!.Count > 0)
                ChatSystemRoles = chatSystemRoles;
        }

        ChatSystemRole.InstanceItems = ChatSystemRoles ??= [new() { Name = "Default System Role", Prompt = "你是協助人員尋找資訊的 AI 助理。" }];

        CurrentChatSystemRole = new();
    }

    [ObservableProperty]
    private AOAISettings? _settings;

    [ObservableProperty]
    private ObservableCollection<ChatSystemRole>? _chatSystemRoles;

    [ObservableProperty]
    private ChatSystemRole? _currentChatSystemRole;

    [ObservableProperty]
    private bool _isEdit;

    [RelayCommand]
    private void Clear()
    {
        System.Diagnostics.Debug.WriteLine($"Clear...");

        if (File.Exists(settingsFilePath))
        {
            File.Delete(settingsFilePath);
        }

        var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath);
        }

        Settings = new();
    }

    [RelayCommand]
    private async Task Save()
    {
        System.Diagnostics.Debug.WriteLine($"Save to...{settingsFilePath}");

        var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var jsonSettings = JsonSerializer.Serialize(Settings);
        await File.WriteAllTextAsync(settingsFilePath, jsonSettings);
    }

    [RelayCommand]
    private async Task Add()
    {
        System.Diagnostics.Debug.WriteLine($"Add...{chatSystemRolesFilePath}");

        if (string.IsNullOrEmpty(CurrentChatSystemRole!.Name) || string.IsNullOrEmpty(CurrentChatSystemRole!.Prompt))
        {
            return;
        }

        ChatSystemRoles!.Add(CurrentChatSystemRole!);

        await SaveDataToFile();

        CurrentChatSystemRole = new();
    }

    [RelayCommand]
    private async Task Delete()
    {
        System.Diagnostics.Debug.WriteLine($"Delete...");

        ChatSystemRoles!.Remove(CurrentChatSystemRole!);

        await SaveDataToFile();

        CurrentChatSystemRole = new();
    }

    [RelayCommand]
    private async Task Edit()
    {
        System.Diagnostics.Debug.WriteLine($"Edit...{chatSystemRolesFilePath}");

        await SaveDataToFile();

        CurrentChatSystemRole = new();

        IsEdit = false;
    }

    [RelayCommand]
    private void Selected()
    {
        IsEdit = CurrentChatSystemRole is not null;        
    }

    private async Task SaveDataToFile()
    {
        var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var jsonSettings = JsonSerializer.Serialize(ChatSystemRoles, options: new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        });
        await File.WriteAllTextAsync(chatSystemRolesFilePath, jsonSettings);
    }
}