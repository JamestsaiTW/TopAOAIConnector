using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TopAOAIConnector.Models;

namespace TopAOAIConnector.ViewModels;

internal partial class SettingPageViewModel : ViewModelBase
{
    private const string directoryName = "TopAOAIConnector";
    private const string settingsFileName = "settings.json";

    private readonly string settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), directoryName, settingsFileName);

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
    }

    [ObservableProperty]
    private AOAISettings? _settings;

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
}