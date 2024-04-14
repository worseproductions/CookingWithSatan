using Godot;

namespace CookingWithSatan.scripts;

public partial class EndController : Control
{
    private ScoreService _scoreService;
    private SupabaseService _supabaseService;
    private Label _winLoseLabel;
    private Label _statsLabel;
    
    public override void _Ready()
    {
        _scoreService = GetNode<ScoreService>("/root/ScoreService");
        _supabaseService = GetNode<SupabaseService>("/root/SupabaseService");
        _winLoseLabel = GetNode<Label>("%WinLoseLabel");
        _statsLabel = GetNode<Label>("%StatsLabel");
        
        _winLoseLabel.Text = _scoreService.Win ? "You reached your donation goal!" : "You lost all your viewers...\n Try to keep them entertained next time!";
        
        _statsLabel.Text = $"Stats\nViewers: {_scoreService.Viewers}\nUptime: {_scoreService.Uptime}\nSubscribers: {_scoreService.Subs}";
        
        if (!_supabaseService.UseOnlineServices) return;
        _supabaseService.AddLeaderboardScore(_scoreService.Viewers, _scoreService.Uptime, _scoreService.Subs);
    }
}