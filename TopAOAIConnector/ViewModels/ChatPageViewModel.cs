using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TopAOAIConnector.ViewModels;

internal partial class ChatPageViewModel : ViewModelBase
{
    private readonly List<ChatMessage> messages = [];

    [ObservableProperty]
    private string _chatText = $"Wellcome to TopAOAIConnector!{Environment.NewLine}";

    [ObservableProperty]
    private string _inputText = string.Empty;

    [RelayCommand]
    private async Task Attach()
    {
        System.Diagnostics.Debug.WriteLine("Attach...");

        var storageProvider = Utilities.AttachFileHelper.StorageProvider;
        var resultFiles = await storageProvider!.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.TextPlain],
            Title = "請選擇文字檔(.txt)",
            SuggestedStartLocation = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop),
        });

        if (resultFiles.Count <= 0) 
            return;

        await using var stream = await resultFiles[0].OpenReadAsync();
        using var reader = new StreamReader(stream);
        var fileContent = await reader.ReadToEndAsync();

        if (!string.IsNullOrEmpty(fileContent) && messages.Count > 0)
        {
            ChatText = $"{Environment.NewLine}{ChatText}{Environment.NewLine}=======Another Attach TextFile======={Environment.NewLine}";
            messages.Clear();
        }

        var textContent = InputText == string.Empty ? fileContent : $"{InputText}{Environment.NewLine}{fileContent}";

        BuildChatText(textContent);

        messages.Add(textContent);

        await BuildAoaiResultToChatText();
    }

    [RelayCommand]
    private async Task Send()
    {
        System.Diagnostics.Debug.WriteLine("Send...");

        var textContent = InputText;

        BuildChatText(textContent);

        messages.Add(textContent);

        await BuildAoaiResultToChatText();
    }

    private void BuildChatText(string textContent)
    {
        ChatText = $"{ChatText}{Environment.NewLine}You:{Environment.NewLine}{textContent}{Environment.NewLine}";
        
        InputText = string.Empty;
    }

    private async Task BuildAoaiResultToChatText()
    {
        var assistantMessage = await Utilities.AoaiServiceHelper.Go(messages).ContinueWith(async task =>
        {
            var result = await task;
            ChatText = $"{ChatText}{Environment.NewLine}{result}{Environment.NewLine}";

            messages.Add(result);
        });
    }
}
