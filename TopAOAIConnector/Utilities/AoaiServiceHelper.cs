using Azure.AI.OpenAI;
using Azure;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TopAOAIConnector.Models;

namespace TopAOAIConnector.Utilities;

internal class AoaiServiceHelper
{
    static readonly string endpoint = AOAISettings.Instance.Endpoint!;
    static readonly string key = AOAISettings.Instance.SecretKey1!;
    static readonly string deployModelName = AOAISettings.Instance.DeployModelName!;

    public static async Task<string> Go(string userMessage)
    {
        var azureOpenAIclient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        var chatClient = azureOpenAIclient.GetChatClient(deployModelName);

        var completionChat = await chatClient.CompleteChatAsync(
            new List<ChatMessage>()
            {
                new UserChatMessage(userMessage)
            });

        var result = $"{completionChat.Value.Role}:{Environment.NewLine}{completionChat.Value.Content[0].Text}";
        return result;
    }
}
