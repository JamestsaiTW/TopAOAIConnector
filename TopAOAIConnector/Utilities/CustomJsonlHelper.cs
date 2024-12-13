using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Tmds.DBus.Protocol;
using TopAOAIConnector.Models;

namespace TopAOAIConnector.Utilities
{
    internal class CustomJsonlHelper
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        internal static string ConveterToJsonl(List<AoaiMessage> aoaiMessages)
        {
            var systemAoaiMessage = aoaiMessages.First();
            var pairedMessages = aoaiMessages
                                    .Select((message, index) => new { Message = message, Index = index })
                                    .Where(x => x.Message.Role == "user" && x.Index + 1 < aoaiMessages.Count && aoaiMessages[x.Index + 1].Role == "assistant")
                                    .Select(x => new
                                    {
                                        User = JsonSerializer.Serialize(x.Message, jsonSerializerOptions),
                                        Assistant = JsonSerializer.Serialize(aoaiMessages[x.Index + 1], jsonSerializerOptions)
                                    })
                                    .ToList();
            var messageData = string.Empty;
            foreach (var pairedMessage in pairedMessages)
            {
                messageData = $"{messageData}{{\"messages\": [{JsonSerializer.Serialize(systemAoaiMessage, jsonSerializerOptions)},{pairedMessage.User},{pairedMessage.Assistant}]}}{Environment.NewLine}";
            }
            return messageData;
        }
    }
}