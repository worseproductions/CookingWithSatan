using Godot;

namespace CookingWithSatan.scripts.ui;

public partial class LeaderboardItem : Control
{
    [Export] public string Username { get; set; }
    [Export] public int Viewers { get; set; }
    [Export] public int Uptime { get; set; }
    [Export] public int Subs { get; set; }
    
    private Label _username;
    private Label _viewers;
    private Label _uptime;
    private Label _subs;
    
    public override void _Ready()
    {
        _username = GetNode<Label>("%Username");
        _viewers = GetNode<Label>("%Viewers");
        _uptime = GetNode<Label>("%Uptime");
        _subs = GetNode<Label>("%Subscribers");
        _username.Text = Username;
        _viewers.Text = Util.FormatK(Viewers);
        _uptime.Text = Util.FormatTime(Uptime);
        _subs.Text = Util.FormatK(Subs);
    }
}