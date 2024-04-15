using Godot;

namespace CookingWithSatan.scripts;

public partial class ChatController : Control
{
    private SupabaseService _supabaseService;
    private GameController _gameController;
    private Button _chatButton;
    private RichTextLabel _chat;
    private LineEdit _chatInput;
    private double _timeSinceLastMessage;
    private int _messagesInLastMinute;

    public override void _Ready()
    {
        var root = GetTree().Root;
        _supabaseService = root.GetNode<SupabaseService>("SupabaseService");
        _gameController = root.GetNode<GameController>("GameController");
        _chatButton = GetNode<Button>("%ChatButton");
        _chat = GetNode<RichTextLabel>("%Chat");
        _chatInput = GetNode<LineEdit>("%ChatInput");

        _chatInput.TextSubmitted += OnChatEditOnTextSubmitted;
        _chatButton.Pressed += OnChatButtonOnPressed;
        _ = _supabaseService.SubscribeToChat();
        _supabaseService.ChatMessageReceived += OnChatMessageReceived;
    }

    private void OnChatEditOnTextSubmitted(string newText)
    {
        if (newText.Length > 0)
            OnChatButtonOnPressed();
    }

    private void OnChatButtonOnPressed()
    {
        var user = "Satan";
        var message = _chatInput.Text;
        if (_supabaseService.UseOnlineServices)
        {
            user = _supabaseService.CurrentUser?.DisplayName;
            _supabaseService.SendChatMessage(message);
        }

        AddMessage($"[color=#a530F0]{user}[/color]: {message}\n");
        _chatInput.Text = "";
        _gameController.TriggerResponseFlood();
    }

    private void OnChatMessageReceived(string username, string message)
    {
        AddMessage($"[color=#a530F0]{username}[/color]: {message}\n");
    }

    public void AddMessage(string message)
    {
        _chat.Text += message;
    }
}