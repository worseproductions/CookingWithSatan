using System.Collections.Generic;
using Godot;

namespace CookingWithSatan.scripts;

public partial class StartController : Control
{
    private Button _startButton;
    private Button _exitButton;
    private ProgressBar _loadingBar;
    private Panel _loadingPanel;
    private Label _loadingLabel;

    private bool _loading;
    private double _loadingTime;
    private double _loadingDuration = 10;

    private readonly List<string> _loadingMessages = new()
    {
        "Setting up hellfire servers...",
        "Summoning d(a)emons...",
        "Initializing brimstone...",
        "Damning souls for eternity...",
        "You're live in 3... 2... 1..."
    };
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetTree().Paused = true;
        _startButton = GetNode<Button>("%StartButton");
        _exitButton = GetNode<Button>("%ExitButton");
        _loadingBar = GetNode<ProgressBar>("%LoadingBar");
        _loadingPanel = GetNode<Panel>("%LoadingPanel");
        _loadingLabel = GetNode<Label>("%LoadingLabel");

        _loadingPanel.Visible = false;
        _startButton.Pressed += OnStartButtonPressed;
        _exitButton.Pressed += OnExitButtonPressed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!_loading) return;
        _loadingTime += delta;
        _loadingBar.Value = (float)(_loadingTime / _loadingDuration);
        var messageIndex = (int)(_loadingTime / _loadingDuration * _loadingMessages.Count);
        _loadingLabel.Text = _loadingMessages[Mathf.Clamp(messageIndex, 0, _loadingMessages.Count - 1)];
        if (!(_loadingTime >= _loadingDuration)) return;
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/stream.tscn");
    }
    
    private void OnStartButtonPressed()
    {
        _loading = true;
        _loadingPanel.Visible = true;
    }
    
    private void OnExitButtonPressed()
    {
        GetTree().Quit();
    }
}