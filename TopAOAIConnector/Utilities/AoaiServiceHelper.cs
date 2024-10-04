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

    public static async Task<ChatCompletion> Go(List<ChatMessage> chatMessage)
    {
        var azureOpenAIclient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key));
        var chatClient = azureOpenAIclient.GetChatClient(deployModelName);

        var completionChat = await chatClient.CompleteChatAsync(chatMessage);

        return completionChat;
    }
}
