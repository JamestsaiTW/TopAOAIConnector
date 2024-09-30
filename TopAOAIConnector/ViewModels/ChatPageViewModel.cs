using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace TopAOAIConnector.ViewModels;

internal partial class ChatPageViewModel : ViewModelBase
{
    private readonly List<ChatMessage> messages = [];

    public ChatPageViewModel()
    {
        SelectedSystemRole = SystemRoles[0];
        messages.Add(ChatMessage.CreateSystemMessage(SelectedSystemRole.Value));
    }

    public ObservableCollection<KeyValuePair<string, string>> SystemRoles { get; } =
    [
        new ("Default System Role", "你是協助人員尋找資訊的 AI 助理。"),
        new ("Movie Reviewer Role", "你是一位專業的電影評論員，請根據使用者的輸入評論來做回答。" +
                                    "僅能回答 \"好\" 或 \"壞\" 並針對使用者輸入的評論給予最多 10 字的反饋。")
    ];

    [ObservableProperty]
    public KeyValuePair<string, string> _selectedSystemRole;

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

        if (!string.IsNullOrEmpty(fileContent) && messages.Count > 1)
        {
            ChatText = $"{Environment.NewLine}{ChatText}{Environment.NewLine}=======Another Attach TextFile======={Environment.NewLine}";
            
            messages.Clear();
            messages.Add(ChatMessage.CreateSystemMessage(SelectedSystemRole.Value));
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

    [RelayCommand]
    private void Selected()
    {
        System.Diagnostics.Debug.WriteLine("Selected...");

        messages.Clear();
        messages.Add(ChatMessage.CreateSystemMessage(SelectedSystemRole.Value));
    }

    private void BuildChatText(string textContent)
    {
        ChatText = $"{ChatText}{Environment.NewLine}You:{Environment.NewLine}{textContent}{Environment.NewLine}";
        
        InputText = string.Empty;
    }

    private async Task BuildAoaiResultToChatText()
    {
        await Utilities.AoaiServiceHelper.Go(messages).ContinueWith(async task =>
        {
            var chatCompletion = await task;
            var textResult = $"{chatCompletion.Role}:{Environment.NewLine}{chatCompletion.Content[0].Text}";

            ChatText = $"{ChatText}{Environment.NewLine}{textResult}{Environment.NewLine}";

            messages.Add(ChatMessage.CreateAssistantMessage(chatCompletion.Content[0].Text));
        });
    }
}
