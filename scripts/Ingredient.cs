using Godot;
using System;
namespace CookingWithSatan.scripts;

public partial class Ingredient : Node2D
{
    private Vector2 velocity;
    [Export] public float speed = 200f;
    private Rect2 bounceArea;
    private Sprite2D _ingredient_Sprite;

    [Export] public Texture2D[] Textures = new Texture2D[3];
    private int timesClicked = 0;
    [Export] public int clicksNeededForState2 = 10;
    [Export] public int clicksNeededForState3 = 20;
    RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _ingredient_Sprite = GetNode<Sprite2D>("Ingredient_Sprite");
        
        if (GetParent() is Control parent)
        {
            bounceArea = new Rect2(Vector2.Zero, parent.GetRect().Size);
        }
        
        velocity = new Vector2(speed, speed);
        Position = bounceArea.Position + new Vector2(bounceArea.Size.X / 2, bounceArea.Size.Y / 2);
        
        SetProcessInput(true);
    }

    public override void _Process(double delta)
    {
        Position += new Vector2(velocity.X * (float)delta, velocity.Y * (float)delta);
        
        if (Position.X < bounceArea.Position.X || Position.X > bounceArea.Position.X + bounceArea.Size.X - _ingredient_Sprite.Scale.X)
        {
            velocity.X = -velocity.X;
        }
        if (Position.Y < bounceArea.Position.Y || Position.Y > bounceArea.Position.Y + bounceArea.Size.Y - _ingredient_Sprite.Scale.Y)
        {
            velocity.Y = -velocity.Y;
        }
    }

    public void Clicked()
    {
        timesClicked++;
        rng.Randomize();
        var angle = rng.RandfRange(0, 2 * Mathf.Pi);
        var newVel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized() * speed;
        newVel *= (1 + 0.1f * (timesClicked - 1));
        velocity = newVel;
        
        if (timesClicked >= clicksNeededForState2 && timesClicked < clicksNeededForState3)
        {
            _ingredient_Sprite.Texture = Textures[1];
        }else if (timesClicked >= clicksNeededForState3)
        {
            _ingredient_Sprite.Texture = Textures[2];
            //trigger logic for ingredient confirmation
        }
    }
}
