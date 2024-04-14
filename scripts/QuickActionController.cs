using Godot;

namespace CookingWithSatan.scripts;

public partial class QuickActionController : Control
{

    private GameController _gameController;
    private Button _hypeButton;
    private Button _resetIngredientsButton;
    private Button _recipeBookButton;
    private Button _endStreamButton;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _gameController = GetNode<GameController>("/root/GameController");
        _hypeButton = GetNode<Button>("%HypeButton");
        _resetIngredientsButton = GetNode<Button>("%ResetIngredientsButton");
        _recipeBookButton = GetNode<Button>("%RecipeBookButton");
        _endStreamButton = GetNode<Button>("%EndStreamButton");
        
        _hypeButton.Pressed += OnHypeButtonPressed;
        _resetIngredientsButton.Pressed += OnResetIngredientsButtonPressed;
        _recipeBookButton.Pressed += OnRecipeBookButtonPressed;
        _endStreamButton.Pressed += OnEndStreamButtonPressed;
    }

    private void OnHypeButtonPressed()
    {
        _gameController.HypeUpChat();
    }
    
    private void OnResetIngredientsButtonPressed()
    {
        _gameController.ResetIngredients();
    }
    
    private void OnRecipeBookButtonPressed()
    {
        _gameController.OpenRecipeBook();
    }
    
    private void OnEndStreamButtonPressed()
    {
        _gameController.EndStream();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}