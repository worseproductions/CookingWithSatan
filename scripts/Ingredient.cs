using Godot;
using System;
using CookingWithSatan.scripts.resources;

namespace CookingWithSatan.scripts;

public partial class Ingredient : Control
{
    private AudioService _audioService;
    
    [Export] public RecipeIngredient RecipeIngredient;
    public bool IngredientIsPrepared;
    
    private Vector2 _velocity;
    [Export] public float StartSpeed = 200f;
    private float _speed;
    private Rect2 _bounceArea;
    private TextureRect _ingredientSprite;
    private Button _button;

    [Export] public Texture2D[] Textures = new Texture2D[3];
    private int _timesClicked;
    [Export] public int ClicksNeededForState2 = 10;
    [Export] public int ClicksNeededForState3 = 20;
    [Export] public float SpeedIncreaseFactor = 1.08f;
    private RandomNumberGenerator _rng = new();

    private bool _fadingOut;
    private float _fadeTime = 1f;
    private float _fadeTimer;
    
    [Signal]
    public delegate void IngredientPreparedEventHandler(Ingredient ingredient);

    public override void _Ready()
    {
        _audioService = GetNode<AudioService>("/root/AudioService");
        _ingredientSprite = GetNode<TextureRect>("IngredientSprite");
        _button = GetNode<Button>("Button");
        _button.Pressed += Clicked;

        _ingredientSprite.Texture = Textures[0];
        
        if (GetParent() is Control parent)
        {
            _bounceArea = new Rect2(Vector2.Zero, parent.GetRect().Size - Size);
        }

        ResetIngredient();
        SetProcessInput(true);
        _speed = StartSpeed;
    }

    public override void _Process(double delta)
    {
        if (IngredientIsPrepared)
        {
            _fadeTimer += (float)delta;
            Modulate = new Color(1, 1, 1, 1 - _fadeTimer / _fadeTime);
            if (!(_fadeTimer >= _fadeTime)) return;
            _fadingOut = false;
            GetParent().RemoveChild(this);
            Modulate = Colors.White;
            EmitSignal(SignalName.IngredientPrepared, this);
            return;
        }
        Position += new Vector2(_velocity.X * (float)delta, _velocity.Y * (float)delta);
        
        if (Position.X <= _bounceArea.Position.X || Position.X >= _bounceArea.Position.X + _bounceArea.Size.X - _ingredientSprite.Scale.X)
        {
            _velocity.X = -_velocity.X;
        }
        if (Position.Y <= _bounceArea.Position.Y || Position.Y >= _bounceArea.Position.Y + _bounceArea.Size.Y - _ingredientSprite.Scale.Y)
        {
            _velocity.Y = -_velocity.Y;
        }
    }

    private void Clicked()
    {
        GD.Print("test");
        if (IngredientIsPrepared) return;
        _audioService.PlaySfx(AudioService.SoundEffectType.Hit);
        _timesClicked++;
        _speed *= SpeedIncreaseFactor;
        var newVelocity = RandomizeDirection() * _speed;
        _velocity = newVelocity;
        
        if (_timesClicked >= ClicksNeededForState2 && _timesClicked < ClicksNeededForState3)
        {
            _ingredientSprite.Texture = Textures[1];
        }else if (_timesClicked == ClicksNeededForState3)
        {
            _ingredientSprite.Texture = Textures[2];
            IngredientIsPrepared = true;
            _fadingOut = true;
        }
    }

    private Vector2 RandomizeDirection()
    {
        _rng.Randomize();
        var angle = _rng.RandfRange(0, 2 * Mathf.Pi);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();
    }

    public void ResetIngredient()
    {
        _speed = StartSpeed;
        _velocity = RandomizeDirection() * _speed;
        var randomPosition = _bounceArea.Position + new Vector2(
            (_rng.Randf() * _bounceArea.Size.X),
            (_rng.Randf() * _bounceArea.Size.Y)
        );
        Position = randomPosition;
        _timesClicked = 0;
        _ingredientSprite.Texture = Textures[0];
        IngredientIsPrepared = false;
        Visible = true;
    }
}
