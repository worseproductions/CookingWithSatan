using System.Linq;
using Godot;

namespace CookingWithSatan.scripts.ui;

public partial class Leaderboard : Control
{
    [Export] private PackedScene _leaderboardItemScene;
    
    private SupabaseService _supabaseService;
    private VBoxContainer _leaderboardContainer;

    public override async void _Ready()
    {
        _supabaseService = GetNode<SupabaseService>("/root/SupabaseService");
        _leaderboardContainer = GetNode<VBoxContainer>("%Leaderboard");

        if (!_supabaseService.UseOnlineServices)
        {
            Visible = false;
            return;
        }
        var leaderboard = await _supabaseService.GetLeaderboard();
        var sortedLeaderboard = leaderboard.Models.OrderByDescending(x => 0.33 * x.Viewers + 0.33 * x.Uptime + 0.33 * x.Subs); // Sort by weighted score
        var ownEntryExists = false;
        foreach (var item in sortedLeaderboard)
        {
            if (item.UserId == _supabaseService.CurrentUser.Id) ownEntryExists = true;
            AddLeaderboardItem(item);
        }

        if (!ownEntryExists)
        {
            AddLeaderboardItem(_supabaseService.CurrentLeaderboard);
        }
    }

    private void AddLeaderboardItem(dto.Leaderboard item)
    {
        var leaderboardItem = _leaderboardItemScene.Instantiate<LeaderboardItem>();
        leaderboardItem.Username = item.User.DisplayName;
        leaderboardItem.Viewers = item.Viewers;
        leaderboardItem.Uptime = item.Uptime;
        leaderboardItem.Subs = item.Subs;
        _leaderboardContainer.AddChild(leaderboardItem);
    }
}