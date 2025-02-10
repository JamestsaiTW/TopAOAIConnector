using Azure.AI.OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TopAOAIConnector.Models;
using System.ClientModel;

namespace TopAOAIConnector.Utilities;

internal class AoaiServiceHelper
{
    static readonly string endpoint = AOAISettings.Instance.Endpoint!;
    static readonly string key = AOAISettings.Instance.SecretKey1!;
    static readonly string deployModelName = AOAISettings.Instance.DeployModelName!;
    
    static readonly AzureOpenAIClient _instance = new(new Uri(endpoint), new ApiKeyCredential(key));
    static AzureOpenAIClient Instance => _instance;

    public static async Task<ChatCompletion> Go(List<ChatMessage> chatMessage)
    {
        var chatClient = Instance.GetChatClient(deployModelName);

        var completionChat = await chatClient.CompleteChatAsync(chatMessage, new ChatCompletionOptions() { ReasoningEffortLevel = "medium" });

        return completionChat;
    }
}
