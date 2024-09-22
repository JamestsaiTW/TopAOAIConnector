using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using TopAOAIConnector.Models;

namespace TopAOAIConnector.ViewModels;

internal partial class SettingPageViewModel : ViewModelBase
{
    private readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "TopAOAIConnector", "settings.json");

    [ObservableProperty]
    private AOAISettings? _settings;

    [RelayCommand]
    private void Clear()
    {
        System.Diagnostics.Debug.WriteLine($"Clear...");
    }

    [RelayCommand]
    private void Save()
    {
        System.Diagnostics.Debug.WriteLine($"Save to...{settingsPath}");
    }
}