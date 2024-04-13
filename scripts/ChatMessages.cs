using Godot;

namespace CookingWithSatan.scripts;

[GlobalClass]
public partial class ChatMessages : Resource
{
    [Export] public ChatMessage[] Messages;
}