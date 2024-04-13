using Godot;

namespace CookingWithSatan.scripts;


[GlobalClass]
public partial class ChatMessage : Resource
{
    [Export] public string User { get; set; }
    [Export] public string Message { get; set; }
    [Export] public int Mood { get; set; }

    public ChatMessage(string user, string message, int mood)
    {
        User = user;
        Message = message;
        Mood = mood;
    }

    public ChatMessage()
    {
    }
}