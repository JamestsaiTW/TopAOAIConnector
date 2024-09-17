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
    private ViewModelBase? _currentPage = new ChatPageViewModel();

    [ObservableProperty]
    private ListItemTemplate? _selectedListItem;

    public ObservableCollection<ListItemTemplate> Items { get; } =
    [
        new ListItemTemplate(typeof(ChatPageViewModel)),
        new ListItemTemplate(typeof(SettingPageViewModel))
    ];

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value is null)
            return;

        var instance = Activator.CreateInstance(value.ModelType);
        if (instance is not null && instance is ViewModelBase currentPageViewModel)
            CurrentPage = currentPageViewModel;
    }

    [ObservableProperty]
    private bool _isPaneOpen;

    [RelayCommand]
    private void MenuButton()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}

public class ListItemTemplate(Type modelType)
{
    public string Name { get; } = modelType.Name.Replace("PageViewModel", "");
    public Type ModelType { get; } = modelType;
}