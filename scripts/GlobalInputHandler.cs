using Godot;

namespace CookingWithSatan.scripts;

public partial class GlobalInputHandler : Node
{
    
    public bool SoundEnabled { get; set; } = true;
    
    [Signal] public delegate void SoundEnabledChangedEventHandler(bool enabled);
    
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_toggle_sound"))
        {
            SoundEnabled = !SoundEnabled;
            EmitSignal(SignalName.SoundEnabledChanged, SoundEnabled);
        }
        if (!@event.IsActionPressed("ui_toggle_fullscreen")) return;
        var currentMode = DisplayServer.WindowGetMode();
        DisplayServer.WindowSetMode(currentMode == DisplayServer.WindowMode.Fullscreen
            ? DisplayServer.WindowMode.Windowed
            : DisplayServer.WindowMode.Fullscreen);
    }
}