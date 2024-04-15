using System;
using System.Linq;
using CookingWithSatan.scripts.resources;
using Godot;
using Godot.Collections;

namespace CookingWithSatan.scripts;

[Icon("res://images/icons/AudioStreamPlayer.svg")]
public partial class AudioService : Control
{
    public enum StreamType
    {
        MainMenu,
        Game,
        Loading
    }

    public enum SoundEffectType
    {
        Hit,
        ChangeTool,
        StartRitual
    }

    private GlobalInputHandler _globalInputHandler;

    [ExportCategory("Music")] private AudioStreamPlayer _audioStreamPlayer;
    [Export] private AudioStream _mainMenuMusic;
    [Export] private AudioStream _gameMusic;
    [Export] private AudioStream _loadingMusic;
    [Export] private int _maxVolume = -30;
    [Export] private int _minVolume = -60;

    [ExportCategory("SFX")] private AudioStreamPlayer _sfxPlayer;
    [Export] private Array<SoundEffect> _soundEffects;
    [Export] private int _sfxVolume = -30;

    [ExportCategory("Fade Timings")] private bool _fadeOutActive;
    [Export] private double _initialFadeOutTime = 1;
    private double _fadeOutTime;
    private double _fadeOutTimer;
    private bool _fadeInActive;
    [Export] private double _initialFadeInTime = 1;
    private double _fadeInTime;
    private double _fadeInTimer;

    private StreamType _nextStream;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _globalInputHandler = GetNode<GlobalInputHandler>("/root/GlobalInputHandler");
        _sfxPlayer = GetNode<AudioStreamPlayer>("SfxPlayer");
        _audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _audioStreamPlayer.Stream = _mainMenuMusic;
        _audioStreamPlayer.VolumeDb = _maxVolume;
        _audioStreamPlayer.Play();

        _globalInputHandler.SoundEnabledChanged += (enabled) =>
        {
            _audioStreamPlayer.VolumeDb = enabled ? _maxVolume : _minVolume;
        };
        
        TraverseTree(GetTree().Root);

        GetTree().NodeAdded += OnSceneTreeNodeAdded;
        GetTree().NodeRemoved += OnSceneTreeNodeRemoved;
        return;

        void TraverseTree(Node node)
        {
            foreach (var child in node.GetChildren())
            {
                if (child is BaseButton button)
                {
                    button.Pressed += OnButtonPressed;
                }
                TraverseTree(child);
            }
        }
    }

    private void OnSceneTreeNodeAdded(Node node)
    {
        if (node is not BaseButton button) return;
        button.Pressed += OnButtonPressed;

    }

    private void OnSceneTreeNodeRemoved(Node node)
    {
        if (node is BaseButton button)
            button.Pressed -= OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        PlaySfx(SoundEffectType.Hit);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_fadeOutActive)
        {
            _fadeOutTimer += delta;
            _audioStreamPlayer.VolumeDb = _globalInputHandler.SoundEnabled
                ? Mathf.Lerp(_maxVolume, _minVolume, (float)(_fadeOutTimer / _fadeOutTime))
                : _minVolume;
            if (_fadeOutTimer >= _fadeOutTime)
            {
                _fadeOutActive = false;
                _fadeOutTimer = 0;
                _audioStreamPlayer.Stop();
                _audioStreamPlayer.Stream = _nextStream switch
                {
                    StreamType.MainMenu => _mainMenuMusic,
                    StreamType.Game => _gameMusic,
                    StreamType.Loading => _loadingMusic,
                    _ => _audioStreamPlayer.Stream
                };
                _audioStreamPlayer.Play();
                _fadeInActive = true;
            }
        }

        if (!_fadeInActive) return;
        _fadeInTimer += delta;
        _audioStreamPlayer.VolumeDb = _globalInputHandler.SoundEnabled
            ? Mathf.Lerp(_minVolume, _maxVolume, (float)(_fadeInTimer / _fadeInTime))
            : _minVolume;

        if (!(_fadeInTimer >= _fadeInTime)) return;
        _fadeInActive = false;
        _fadeInTimer = 0;
    }

    /// <summary>
    /// Plays one of the streams defined in the StreamType enum.
    /// </summary>
    /// <param name="type">The stream type</param>
    /// <param name="fadeInTime">The fade in time. If null, it uses the initial value</param>
    /// <param name="fadeOutTime">The fade out time. If null, it uses the fade in time or the initial value</param>
    public void PlayStream(StreamType type, double? fadeInTime = null, double? fadeOutTime = null)
    {
        // fade between songs
        _fadeOutActive = true;
        _fadeOutTimer = 0;
        _fadeInTime = Math.Max(0.1, fadeInTime ?? _initialFadeInTime);
        _fadeOutTime = Math.Max(0.1, fadeOutTime ?? fadeInTime ?? _initialFadeOutTime);
        _nextStream = type;
    }

    public void PlaySfx(SoundEffectType changeTool)
    {
        GD.Print("Playing sfx");
        var soundEffect = _soundEffects.First(x => x.Type == changeTool);
        _sfxPlayer.VolumeDb = _globalInputHandler.SoundEnabled
            ? soundEffect.UseGlobalVolume ? _sfxVolume : soundEffect.VolumeDb
            : _minVolume;
        _sfxPlayer.Stream = soundEffect.Stream;
        _sfxPlayer.Play();
    }
}