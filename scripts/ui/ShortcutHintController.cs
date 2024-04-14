using Godot;

namespace CookingWithSatan.scripts.ui;

public partial class ShortcutHintController : Control
{
    private GlobalInputHandler _globalInputHandler;
    private Label _label;
    
    public override void _Ready()
    {
        _globalInputHandler = GetNode<GlobalInputHandler>("/root/GlobalInputHandler");
        _label = GetNode<Label>("Label");
    }
    public override void _Process(double delta)
    {
        _label.Text = _globalInputHandler.SoundEnabled ? _label.Text.Replace("Unmute", "Mute") : _label.Text.Replace("Mute", "Unmute");
        _label.Text = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? _label.Text.Replace("Fullscreen", "Windowed") : _label.Text.Replace("Windowed", "Fullscreen");
    }
}