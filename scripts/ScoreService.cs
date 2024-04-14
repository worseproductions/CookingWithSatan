using Godot;

namespace CookingWithSatan.scripts;

public partial class ScoreService : Node
{
    public bool Win { get; set; }
    public int Viewers { get; set; }
    public int Uptime { get; set; }
    public int Subs { get; set; }
    
    public override void _Ready()
    {
    }
}