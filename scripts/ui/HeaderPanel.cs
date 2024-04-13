using Godot;

namespace CookingWithSatan.scripts.ui;

public partial class HeaderPanel : Control
{
    private Label _watching;
    private Label _duration;
    private Label _followers;
    
    
    public override void _Ready()
    {
        _watching = GetNode<Label>("%WatchingLabel");
        _duration = GetNode<Label>("%DurationLabel");
        _followers = GetNode<Label>("%FollowersLabel");
    }

    public override void _Process(double delta)
    {
        _watching.Text = $"{Util.FormatK(GameController.Viewers)} watching";
        _duration.Text = $"Live for: {Util.FormatTime(GameController.Duration)}";
        _followers.Text = $"{Util.FormatK(GameController.Followers)} Followers";
    }
}