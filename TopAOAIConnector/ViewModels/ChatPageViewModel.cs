using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TopAOAIConnector.ViewModels;

internal partial class ChatPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _chatText = "Chat Page!";

    [ObservableProperty]
    private string _inputText = string.Empty;

    [RelayCommand]
    private void Attach()
    {
        System.Diagnostics.Debug.WriteLine("Attach...");
    }

    [RelayCommand]
    private void Send()
    {
        System.Diagnostics.Debug.WriteLine("Send...");
    }
}
