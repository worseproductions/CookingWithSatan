using Godot;

namespace CookingWithSatan.scripts;

public partial class QuickActionController : Control
{

    private GameController _gameController;
    private Button _hypeButton;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _gameController = GetNode<GameController>("/root/GameController");
        _hypeButton = GetNode<Button>("%HypeButton");
        _hypeButton.Pressed += OnHypeButtonPressed;
    }

    private void OnHypeButtonPressed()
    {
        _gameController.HypeUpChat();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}