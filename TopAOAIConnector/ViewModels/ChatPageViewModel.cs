﻿using Avalonia.Controls;
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
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using TopAOAIConnector.Utilities;

namespace TopAOAIConnector.ViewModels;

internal partial class ChatPageViewModel : ViewModelBase
{
    private readonly List<ChatMessage> messages = [];

    private string fileName = string.Empty;
    private string fileContent = string.Empty;

    private string appandText = string.Empty;
    private string lastFileName = string.Empty;

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

        fileContent = await reader.ReadToEndAsync();

        if (!string.IsNullOrEmpty(fileContent) && messages.Count > 1 && !string.IsNullOrEmpty(lastFileName))
        {
            lastFileName = string.Empty;
            appandText = $"{Environment.NewLine}======= Another Attach TextFile =======";

            ChatText = $"{ChatText}{appandText}";

            messages.Clear();
            messages.Add(ChatMessage.CreateSystemMessage(SelectedSystemRole!.Prompt));
        }

        fileName = resultFiles[0].Name;

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

        if (!isInputTextEmpty || messages.Count > 2)
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
    private void Selected(Control control)
    {
        System.Diagnostics.Debug.WriteLine("Selected...");

        if (control is not null && control is SelectableTextBlock selectableTextBlock)
        {
            messages.Clear();
            messages.Add(ChatMessage.CreateSystemMessage(SelectedSystemRole!.Prompt));

            InputText = string.Empty;

            fileName = string.Empty;
            fileContent = string.Empty;

            selectableTextBlock.Inlines?.Clear();

            appandText = $"Welcome to TopAOAIConnector!{Environment.NewLine}";
            selectableTextBlock.Text = ChatText = appandText;
        }
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
            if (!string.IsNullOrEmpty(fileName))
            {
                appandText = appandText.Replace(fileContent, string.Empty);

                selectableTextBlock.Inlines?.Add(new Run(appandText));

                var fileStackPanel = new StackPanel()
                {
                    Children =
                    {
                        new SymbolIcon() { FontSize = 60, Symbol = FluentIcons.Common.Symbol.Document, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left},
                        new TextBlock() { FontSize = 20, Text = fileName }
                    }
                };

                selectableTextBlock.Inlines?.Add(fileStackPanel);

                lastFileName = fileName;
                fileName = string.Empty;
            }
            else
            {
                selectableTextBlock.Inlines?.Add(new Run(appandText));
            }

            selectableTextBlock.Inlines?.Add(new LineBreak());
            appandText = string.Empty;
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        var storageProvider = Utilities.AttachFileHelper.StorageProvider;
        var saveFile = await storageProvider!.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            FileTypeChoices = [FilePickerFileTypes.All],
            Title = "請匯出聊天過程檔(.jsonl)",
            SuggestedStartLocation = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop),
            DefaultExtension = "jsonl",
            SuggestedFileName = $"ChatRecords_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
            ShowOverwritePrompt = true,
        });

        if (saveFile is not null)
        {
            await using var stream = await saveFile.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);

            var aoaiMessages = new List<AoaiMessage>();
            foreach (var message in messages)
            {
                aoaiMessages.Add(new AoaiMessage() { Role = message.GetType().Name, Content = message.Content[0].Text });
            }

            var aoaiMessagesData = CustomJsonlHelper.ConveterToJsonl(aoaiMessages); 
            await streamWriter.WriteAsync(aoaiMessagesData);
        }
    }

    private void BuildChatText(string textContent)
    {
        appandText = $"{Environment.NewLine}You:{Environment.NewLine}{textContent}";
        ChatText = $"{ChatText}{appandText}";
       
        InputText = string.Empty;
    }

    private async Task BuildAoaiResultToChatText()
    {
        await Utilities.AoaiServiceHelper.Go(messages).ContinueWith(async task =>
        {
            var chatCompletion = await task;

            appandText = $"{Environment.NewLine}{chatCompletion.Role}:{Environment.NewLine}{chatCompletion.Content[0].Text}";
            ChatText = $"{ChatText}{Environment.NewLine}{appandText}{Environment.NewLine}";

            messages.Add(ChatMessage.CreateAssistantMessage(chatCompletion.Content[0].Text));
        });
    }
}
