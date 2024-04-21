using Godot;
namespace CookingWithSatan.scripts;
public partial class ArmManager : Control
{
    private AudioService _audioService;
    private Arm _arm;
    public override void _Ready()
    {
        _audioService = GetNode<AudioService>("/root/AudioService");
        _arm = GetNode<Arm>("Arm");
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;
        _arm?.CursorChange();
        _audioService.PlaySfx(AudioService.SoundEffectType.ChangeTool);
    }
}
