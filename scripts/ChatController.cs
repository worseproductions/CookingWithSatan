using Godot;

namespace CookingWithSatan.scripts;

public partial class ChatController : Control
{
    private GameController _gameController;
    private Button _chatButton;
    private RichTextLabel _chat;
    private LineEdit _chatInput;
    private double _timeSinceLastMessage;
    private int _messagesInLastMinute;
    
    public override void _Ready()
    {
        _gameController = GetNode<GameController>("/root/GameController");
        _chatButton = GetNode<Button>("%ChatButton");
        _chat = GetNode<RichTextLabel>("%Chat");
        _chatInput = GetNode<LineEdit>("%ChatInput");
        _chatInput.TextSubmitted += OnChatEditOnTextSubmitted;
        _chatButton.Pressed += OnChatButtonOnPressed;
    }

    private void OnChatEditOnTextSubmitted(string newText)
    {
            OnChatButtonOnPressed();
    }
    
    private void OnChatButtonOnPressed()
    {
        AddMessage($"[color=#a530F0]Satan[/color]: {_chatInput.Text}\n");
        _chatInput.Text = "";
        _gameController.TriggerResponseFlood();
    }

    public override void _Process(double delta)
    {
        
    }

    public void AddMessage(string message)
    {
        _chat.Text += message;
    }
}