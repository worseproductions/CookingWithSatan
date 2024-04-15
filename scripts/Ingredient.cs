using Godot;
using System;
using CookingWithSatan.scripts.resources;

namespace CookingWithSatan.scripts;

public partial class Ingredient : Node2D
{
    [Export] public RecipeIngredient RecipeIngredient;
    public bool ingredientIsPrepared = false;
    
    private Vector2 velocity;
    [Export] public float startSpeed = 200f;
    private float speed;
    private Rect2 bounceArea;
    private Sprite2D _ingredient_Sprite;

    [Export] public Texture2D[] Textures = new Texture2D[3];
    private int timesClicked = 0;
    [Export] public int clicksNeededForState2 = 10;
    [Export] public int clicksNeededForState3 = 20;
    [Export] public float speedIncreaseFactor = 1.08f;
    RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _ingredient_Sprite = GetNode<Sprite2D>("Ingredient_Sprite");
        
        if (GetParent() is Control parent)
        {
            bounceArea = new Rect2(Vector2.Zero, parent.GetRect().Size);
        }

        ResetIngredient();
        SetProcessInput(true);
        speed = startSpeed;
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
        speed *= speedIncreaseFactor;
        Vector2 newVel = randomizeDirection() * speed;
        velocity = newVel;
        
        if (timesClicked >= clicksNeededForState2 && timesClicked < clicksNeededForState3)
        {
            _ingredient_Sprite.Texture = Textures[1];
        }else if (timesClicked == clicksNeededForState3)
        {
            _ingredient_Sprite.Texture = Textures[2];
            ingredientIsPrepared = true;
        }
    }

    private Vector2 randomizeDirection()
    {
        rng.Randomize();
        var angle = rng.RandfRange(0, 2 * Mathf.Pi);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();
    }

    public void ResetIngredient()
    {
        speed = startSpeed;
        velocity = randomizeDirection() * speed;
        Vector2 randomPosition = bounceArea.Position + new Vector2(
            (rng.Randf() * bounceArea.Size.X),
            (rng.Randf() * bounceArea.Size.Y)
        );
        Position = randomPosition;
        timesClicked = 0;
        _ingredient_Sprite.Texture = Textures[0];
        ingredientIsPrepared = false;
        Visible = true;
    }
}
