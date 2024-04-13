using Godot;

namespace CookingWithSatan.scripts.ui;

public partial class HeaderPanel : Control
{
    private GameController _gameController;
    private Label _watching;
    private Label _duration;
    private Label _subs;
    
    
    public override void _Ready()
    {
        _gameController = GetNode<GameController>("/root/GameController");
        _watching = GetNode<Label>("%WatchingLabel");
        _duration = GetNode<Label>("%DurationLabel");
        _subs = GetNode<Label>("%SubsLabel");
    }

    public override void _Process(double delta)
    {
        _watching.Text = $"{Util.FormatK(_gameController.Viewers)} watching";
        _duration.Text = $"Live for: {Util.FormatTime(_gameController.Duration)}";
        _subs.Text = $"{Util.FormatK(_gameController.Subs)} Subs";
    }
}