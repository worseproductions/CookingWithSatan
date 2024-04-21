using System.Collections.Generic;
using CookingWithSatan.scripts.resources;
using Godot;

namespace CookingWithSatan.scripts;

public partial class BookController : Control
{
    [Export] private Texture2D _normalBook;
    [Export] private Texture2D _zoomedBook;

    private readonly List<Recipe> _unlockedRecipes = new();
    private TextureRect _bookImage;
    private int _currentRecipeIndex;
    private Control _currentRecipeContainer;
    private bool _isZoomed;
    private Control _zoomedRecipeContainer;
    private Button _nextButton;
    private Button _zoomButton;
    private Button _previousButton;

    public override void _Ready()
    {
        _bookImage = GetNode<TextureRect>("%BookImage");
        _currentRecipeContainer = GetNode<Control>("%CurrentRecipe");
        _zoomedRecipeContainer = GetNode<Control>("%ZoomedRecipe");
        _nextButton = GetNode<Button>("%NextButton");
        _zoomButton = GetNode<Button>("%ZoomButton");
        _previousButton = GetNode<Button>("%PreviousButton");

        _nextButton.Pressed += () => ShowRecipe(_currentRecipeIndex + 1);
        _previousButton.Pressed += () => ShowRecipe(_currentRecipeIndex - 1);
        _zoomButton.Pressed += ZoomRecipe;
    }

    public override void _Process(double delta)
    {
    }

    public void AddRecipe(Recipe recipe)
    {
        _unlockedRecipes.Add(recipe);
        ShowRecipe(_unlockedRecipes.Count - 1); // Show the newly added recipe by default
    }

    private void ShowRecipe(int index)
    {
        _currentRecipeIndex = Mathf.Clamp(index, 0, _unlockedRecipes.Count - 1);
        var recipe = _unlockedRecipes[_currentRecipeIndex];
        _currentRecipeContainer.GetNode<TextureRect>("RecipeImage").Texture = recipe.Image;
        _currentRecipeContainer.GetNode<Label>("RecipeName").Text = recipe.Name;
        _currentRecipeContainer.GetNode<Label>("RecipeDescription").Text = recipe.Description;
        _zoomedRecipeContainer.GetNode<TextureRect>("RecipeImage").Texture = recipe.Image;
        _zoomedRecipeContainer.GetNode<Label>("RecipeName").Text = recipe.Name;
        _zoomedRecipeContainer.GetNode<Label>("RecipeDescription").Text = recipe.Description;
    }

    private void ZoomRecipe()
    {
        _isZoomed = !_isZoomed;
        _bookImage.Texture = _isZoomed ? _zoomedBook : _normalBook;
        _currentRecipeContainer.Visible = !_isZoomed;
        _zoomedRecipeContainer.Visible = _isZoomed;
    }
}