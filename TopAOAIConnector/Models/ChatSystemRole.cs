using System.Collections.ObjectModel;

namespace TopAOAIConnector.Models;

internal class ChatSystemRole
{
    public static ObservableCollection<ChatSystemRole> InstanceItems { get; set; }
    static ChatSystemRole()
    {
        InstanceItems = [];
    }

    public string? Name { get; set; }
    public string? Prompt { get; set; }
}