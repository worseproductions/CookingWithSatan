using Godot;
namespace CookingWithSatan.scripts;
public partial class Arm_Manager : Control
{
    private AudioService _audioService;
    private Node2D _arm;
    public override void _Ready()
    {
        _audioService = GetNode<AudioService>("/root/AudioService");
        _arm = GetNode<Node2D>("Arm");
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;
        _arm?.Call("_CursorChange");
        _audioService.PlaySfx(AudioService.SoundEffectType.ChangeTool);
    }
}
