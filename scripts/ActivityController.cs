using Godot;

namespace CookingWithSatan.scripts;

public partial class ActivityController : Control
{
    [Export] private int _maxDonations = 100;
    private RichTextLabel _activityFeed;
    private ProgressBar _donationProgress;
    private Label _donationProgressText;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _activityFeed = GetNode<RichTextLabel>("%ActivityFeed");
        _donationProgress = GetNode<ProgressBar>("%DonationProgress");
        _donationProgressText = GetNode<Label>("%DonationProgressText");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void AddDonation(string user, int amount)
    {
        _activityFeed.Text += $"[color=#ff7d46]{user}[/color] donated {amount}ᛋ!\n";
        var percentage = _donationProgress.Value + (float)amount / _maxDonations * 100;
        _donationProgress.Value += percentage;
        _donationProgressText.Text = $"{percentage:0D}% funding reached for the Satanic Deficiency Fund";
    }
}