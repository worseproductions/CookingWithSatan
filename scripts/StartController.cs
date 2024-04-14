using System.Collections.Generic;
using Godot;

namespace CookingWithSatan.scripts;

public partial class StartController : Control
{
    private SupabaseService _supabaseService;
    private Button _startButton;
    private Button _exitButton;
    private ProgressBar _loadingBar;
    private Panel _loadingPanel;
    private Label _loadingLabel;
    private CheckBox _useOnlineServices;
    private Control _loadingContainer;
    private Control _signInContainer;
    private Control _usernameContainer;
    private LineEdit _usernameInput;
    private Button _saveUsernameButton;
    private LineEdit _emailInput;
    private LineEdit _passwordInput;
    private Button _signInOrRegisterButton;
    private Button _cancelOnlineServicesButton;

    private bool _loading;
    private double _loadingTime;
    private double _loadingDuration = 10;

    private readonly List<string> _loadingMessages = new()
    {
        "Connecting to hellkick servers...",
        "Summoning d(a)emons...",
        "Initializing brimstone...",
        "Damning souls for eternity...",
        "You're live in 3... 2... 1..."
    };

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetTree().Paused = true;
        _supabaseService = GetNode<SupabaseService>("/root/SupabaseService");
        _startButton = GetNode<Button>("%StartButton");
        _exitButton = GetNode<Button>("%ExitButton");
        _loadingBar = GetNode<ProgressBar>("%LoadingBar");
        _loadingPanel = GetNode<Panel>("%LoadingPanel");
        _loadingLabel = GetNode<Label>("%LoadingLabel");
        _loadingContainer = _loadingPanel.GetNode<Control>("LoadingContainer");
        _signInContainer = _loadingPanel.GetNode<Control>("LoginContainer");
        _usernameContainer = _loadingPanel.GetNode<Control>("UsernameContainer");
        _emailInput = _signInContainer.GetNode<LineEdit>("Email");
        _passwordInput = _signInContainer.GetNode<LineEdit>("Password");
        _signInOrRegisterButton = _signInContainer.GetNode<Button>("SignInOrRegisterButton");
        _usernameInput = _usernameContainer.GetNode<LineEdit>("Username");
        _saveUsernameButton = _usernameContainer.GetNode<Button>("SaveUsernameButton");
        _useOnlineServices = GetNode<CheckBox>("%OnlineServicesCheckbox");
        _cancelOnlineServicesButton = _signInContainer.GetNode<Button>("CancelOnlineServicesButton");

        _loadingPanel.Visible = false;
        _startButton.Pressed += OnStartButtonPressed;
        _exitButton.Pressed += OnExitButtonPressed;
        _cancelOnlineServicesButton.Pressed += () =>
        {
            _supabaseService.UseOnlineServices = false;
            _loading = true;
            _loadingContainer.Visible = true;
            _signInContainer.Visible = false;
            _usernameContainer.Visible = false;
        };
        _signInOrRegisterButton.Pressed += async () =>
        {
            var signedUp = await _supabaseService.LoginOrSignup(_emailInput.Text, _passwordInput.Text);
            if (signedUp)
            {
                _signInContainer.Visible = false;
                _usernameContainer.Visible = true;
            }
            else
            {
                _loading = true;
                _loadingContainer.Visible = true;
                _signInContainer.Visible = false;
            }
        };
        _saveUsernameButton.Pressed += async () =>
        {
            var username = _usernameInput.Text;
            await _supabaseService.SetUsername(username);
            _loading = true;
            _loadingContainer.Visible = true;
            _usernameContainer.Visible = false;
        };
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
        _loadingPanel.Visible = true;
        if (_useOnlineServices.ButtonPressed)
        {
            _supabaseService.UseOnlineServices = true;
            _loadingContainer.Visible = false;
            _signInContainer.Visible = true;
            _usernameContainer.Visible = false;
        }
        else
        {
            _loading = true;
            _loadingContainer.Visible = true;
            _signInContainer.Visible = false;
            _usernameContainer.Visible = false;
        }
    }

    private void OnExitButtonPressed()
    {
        GetTree().Quit();
    }
}