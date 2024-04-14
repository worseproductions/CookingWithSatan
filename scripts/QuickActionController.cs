using Godot;

namespace CookingWithSatan.scripts;

public partial class QuickActionController : Control
{

    private GameController _gameController;
    private Button _hypeButton;
    private Button _resetIngredientsButton;
    private Button _recipeBookButton;
    private Button _summonButton;
    private Button _endStreamButton;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _gameController = GetNode<GameController>("/root/GameController");
        _hypeButton = GetNode<Button>("%HypeButton");
        _resetIngredientsButton = GetNode<Button>("%ResetIngredientsButton");
        _recipeBookButton = GetNode<Button>("%RecipeBookButton");
        _summonButton = GetNode<Button>("%SummonButton");
        _endStreamButton = GetNode<Button>("%EndStreamButton");
        
        _hypeButton.Pressed += () => _gameController.HypeUpChat();
        _resetIngredientsButton.Pressed += () => _gameController.ResetIngredients();
        _recipeBookButton.Pressed += () => _gameController.OpenRecipeBook();
        _summonButton.Pressed += () => _gameController.SummonRecipe();
        _endStreamButton.Pressed += () => _gameController.EndStream();
    }
}