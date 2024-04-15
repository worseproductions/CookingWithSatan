using System.Collections.Generic;
using System.Linq;
using CookingWithSatan.scripts.resources;
using Godot;
using Godot.Collections;

namespace CookingWithSatan.scripts;

public partial class SummonController : Control
{
    [Export] private int _animationDuration = 5;
    private bool _animationRunning;
    private double _animationTimer;
    private List<PathFollow2D> _ingredientPaths;
    
    [Export] private Array<Recipe> _recipes;
    private Recipe _currentRecipe;
    private Control _recipeContainer;
    [Export] private Texture2D _failedRecipeImage;
    
    [Signal]
    public delegate void RecipeFailedEventHandler();
    
    [Signal]
    public delegate void RecipeSuccessEventHandler(Recipe recipe);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _recipeContainer = GetNode<Control>("RecipeContainer");
        _ingredientPaths = new List<PathFollow2D>();
        foreach (var child in GetNode("IngredientPaths").GetChildren())
        {
            if (child is Path2D path)
            {
                _ingredientPaths.Add(path.GetChild<PathFollow2D>(0));
            }
        }
        _animationRunning = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!_animationRunning) return;
        _animationTimer += delta;
        foreach (var path in _ingredientPaths)
        {
            path.ProgressRatio = (float)(_animationTimer / _animationDuration);
        }
        if (!(_animationTimer >= _animationDuration)) return;
        foreach (var path in _ingredientPaths)
        {
            path.GetChild<GpuParticles2D>(1).Emitting = false;
        }

        if (_currentRecipe == null)
        {
            _recipeContainer.GetChild<TextureRect>(0).Texture = _failedRecipeImage;
            _recipeContainer.GetChild<Label>(1).Text = "You failed";
            EmitSignal(SignalName.RecipeFailed);
        }
        else
        {
            _recipeContainer.GetChild<TextureRect>(0).Texture = _currentRecipe.Image;
            _recipeContainer.GetChild<Label>(1).Text = _currentRecipe.Name;
            EmitSignal(SignalName.RecipeSuccess, _currentRecipe);
        }
        _animationRunning = false;
        _animationTimer = 0;
    }

    public void StartSummon(List<RecipeIngredient> currentIngredients)
    {
        var matchingRecipes = from recipe in _recipes
            where !recipe.Ingredients.Except(currentIngredients).Any()
            select recipe;
        
        _currentRecipe = matchingRecipes.ToList().First();
        
        for (var index = 0; index < _ingredientPaths.Count; index++)
        {
            var path = _ingredientPaths[index];
            path.Progress = 0;
            var sprite = path.GetChild<Sprite2D>(0);
            sprite.Texture = currentIngredients[index].Image as Texture2D;
        }
        _animationRunning = true;
    }
}