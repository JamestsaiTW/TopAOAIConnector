using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls.Documents;
using System.Threading.Tasks;
using TopAOAIConnector.Models;

namespace TopAOAIConnector.ViewModels;

internal partial class ChatPageViewModel : ViewModelBase
{
    private readonly List<ChatMessage> messages = [];

    private string fileName = string.Empty;
    private string fileContent = string.Empty;

    public ChatPageViewModel()
    {
        if (SystemRoles.Count > 0)
        {
            SelectedSystemRole = SystemRoles[0];
            messages.Add(ChatMessage.CreateSystemMessage(SelectedSystemRole.Prompt));
        }
    }

    public ObservableCollection<ChatSystemRole> SystemRoles { get; } = ChatSystemRole.InstanceItems;

    [ObservableProperty]
    public ChatSystemRole? _selectedSystemRole;

    [ObservableProperty]
    private string _chatText = $"Welcome to TopAOAIConnector!{Environment.NewLine}";

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

        fileName = resultFiles[0].Name;
        fileContent = await reader.ReadToEndAsync();

        if (!string.IsNullOrEmpty(fileContent) && messages.Count > 1)
        {
            ChatText = $"{Environment.NewLine}{ChatText}{Environment.NewLine}=======Another Attach TextFile======={Environment.NewLine}";
            
            messages.Clear();
            messages.Add(ChatMessage.CreateSystemMessage(SelectedSystemRole!.Prompt));
        }

        var isInputTextEmpty = string.IsNullOrEmpty(InputText);

        var textContent = isInputTextEmpty ? fileContent : $"{InputText}{Environment.NewLine}{fileContent}";

        messages.Add(textContent);

        if (fileContent.Length > 750)
        {
            fileContent = $"{fileContent[..500]}{Environment.NewLine}{Environment.NewLine}" +
              $"...略...{Environment.NewLine}{Environment.NewLine}" +
              $"{fileContent[(fileContent.Length - 250)..fileContent.Length]}";

            textContent = isInputTextEmpty ? fileContent : $"{InputText}{Environment.NewLine}{fileContent}";
        }
        
        BuildChatText(textContent);            

        if (!isInputTextEmpty)
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
        messages.Add(ChatMessage.CreateSystemMessage(SelectedSystemRole!.Prompt));

        ChatText = $"Welcome to TopAOAIConnector!{Environment.NewLine}";
    }

    [RelayCommand]
    private static void Scroll(ContentControl contentControl)
    {
        System.Diagnostics.Debug.WriteLine("Scroll...");

        if (contentControl is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToEnd();
        }
    }

    [RelayCommand]
    private void BuildInline(Control control)
    {
        System.Diagnostics.Debug.WriteLine("BuildInline...");

        if (control is not null && control is SelectableTextBlock selectableTextBlock)
        {
            selectableTextBlock.Text = string.Empty;
            selectableTextBlock.Inlines?.Clear();

            int index = ChatText.IndexOf(fileContent);

            selectableTextBlock.Inlines?.Add(new Run(ChatText[..index]));

            var fileStackPanel = new StackPanel()
            {
                Children =
                {
                    new SymbolIcon() { FontSize = 60, Symbol = FluentIcons.Common.Symbol.Document, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left},
                    new TextBlock() { FontSize = 20, Text = fileName }
                }
            };

            selectableTextBlock.Inlines?.Add(fileStackPanel);
            selectableTextBlock.Inlines?.Add(new Run(ChatText[(index + fileContent.Length)..]));
        }
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
