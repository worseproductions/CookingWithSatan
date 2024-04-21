using Godot;
using System;

namespace CookingWithSatan.scripts;

public partial class Arm : TextureRect
{
    [Export] public Texture2D[] Textures = new Texture2D[16];
    [Export] public float YOffset = 0;

    private Vector2 _screenCenter;

    private Rect2 _bounds;

    public override void _Ready()
    {
        var parent = GetParent<Control>();
        GD.Print($"Parent: {parent}");
        //Calculate the center of the screen
        _screenCenter = GetViewportRect().Size / 2;
        _bounds = new Rect2(Vector2.Zero, parent.GetRect().Size);
    }

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        Position = PositionWithinBounds(new Vector2(mousePosition.X, mousePosition.Y + YOffset));

        var direction = _screenCenter - GlobalPosition;
        var angle = direction.Angle() + Math.PI / 2;
        Rotation = (float)angle;
    }

    public void CursorChange()
    {
        var randomIndex = new Random().Next(0, Textures.Length - 1);
        Texture = Textures[randomIndex];
    }

    private Vector2 PositionWithinBounds(Vector2 currentPosition)
    {
        return new Vector2(
            Mathf.Clamp(currentPosition.X, _bounds.Position.X, _bounds.End.X),
            Mathf.Clamp(currentPosition.Y, _bounds.Position.Y, _bounds.End.Y)
        );
    }
}