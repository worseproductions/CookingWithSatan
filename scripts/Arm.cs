using Godot;
using System;
namespace CookingWithSatan.scripts;

public partial class Arm : Sprite2D
{
    [Export] public Texture2D[] Textures = new Texture2D[16];
    [Export] public float YOffset = 0; // Adjust this value to your needs

    private Vector2 screenCenter;
    
    private Rect2 bounds;
    
    public override void _Ready()
    {
        //Calculate the center of the screen
        screenCenter = GetViewportRect().Size / 2;
        
        if (GetParent() is Control parent)
        {
            bounds = new Rect2(Vector2.Zero, parent.GetRect().Size);
        }
    }
    
    public override void _Process(double delta)
    {
        Vector2 mousePosition = GetGlobalMousePosition();
        Position = PositionWithinBounds(new Vector2(mousePosition.X, mousePosition.Y + YOffset));
        
        Vector2 direction = screenCenter - GlobalPosition;
        float angle = direction.Angle();
        Rotation = angle;
    }

    public void _CursorChange()
    {
        int randomIndex = new Random().Next(0, 16);
        Texture = Textures[randomIndex];
    }
    
    private Vector2 PositionWithinBounds(Vector2 currentPosition)
    {
        return new Vector2(
            Mathf.Clamp(currentPosition.X, bounds.Position.X, bounds.End.X),
            Mathf.Clamp(currentPosition.Y, bounds.Position.Y, bounds.End.Y)
        );
    }
}
