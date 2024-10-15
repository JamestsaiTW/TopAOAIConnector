
using System.Text.Json.Serialization;


namespace TopAOAIConnector.Models;

internal class AoaiMessage
{
    private  string _role;
    [JsonPropertyName("role")]
    public string Role { get { return _role; }
        
        set {
            switch (value)
            {
                case "SystemChatMessage":
                    _role = "system";
                    break;
                case "UserChatMessage":
                    _role = "user";
                    break;
                case "AssistantChatMessage":
                    _role = "assistant";
                    break;
                default:
                    break;
            }
        } }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}