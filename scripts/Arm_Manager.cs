using Godot;
using System;
namespace CookingWithSatan.scripts;
public partial class Arm_Manager : Control
{
    private Node2D _arm;
    public override void _Ready()
    {
        _arm = GetNode<Node2D>("Arm");
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
        {
            _arm?.Call("_CursorChange");
        }
    }
}
