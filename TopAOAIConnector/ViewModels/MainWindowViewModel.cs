using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace TopAOAIConnector.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

    [ObservableProperty]
    private UserControl? _currentPage = new ChatPageView() { DataContext = new ChatPageViewModel()};

    [ObservableProperty]
    private ListItemTemplate? _selectedListItem;

    public ObservableCollection<ListItemTemplate> Items { get; } =
    [
        new ListItemTemplate("Chat", typeof(ChatPageView), typeof(ChatPageViewModel)),
        new ListItemTemplate("Settings", typeof(SettingPageView), typeof(SettingPageViewModel))
    ];

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value is null)
            return;

        var instance = Activator.CreateInstance(value.PageViewType);
        if (instance is not null && instance is UserControl currentPageView)
        {
            currentPageView.DataContext = Activator.CreateInstance(value.PaggViewModelType);
            CurrentPage = currentPageView;
        }
    }

    [ObservableProperty]
    private bool _isPaneOpen;

    [RelayCommand]
    private void MenuButton()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}

public class ListItemTemplate(string icon, Type pageViewType, Type pageViewModelType)
{
    public string Icon { get; } = icon;
    public string Name { get; } = pageViewType.Name.Replace("PageView", "");
    public Type PageViewType { get; } = pageViewType;
    public Type PaggViewModelType { get; } = pageViewModelType;
}