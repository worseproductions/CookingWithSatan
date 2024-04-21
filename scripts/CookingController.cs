using Godot;
using System;
using System.Collections.Generic;
using CookingWithSatan.scripts.resources;

namespace CookingWithSatan.scripts;

public partial class CookingController : Control
{
    private List<RecipeIngredient> _currentIngredients;
    [Export] public Ingredient[] IngredientNodes = new Ingredient[6];


    private enum CookingStates
    {
        Ingredients,
        Book,
        Summon
    }

    private CookingStates _currentState = CookingStates.Ingredients;
    private bool _restoreIngredientScreen = true;
    private Control _ingredientsScreen;
    private HBoxContainer _currentIngredientsContainer;
    private BookController _bookScreen;
    private SummonController _summonScreen;

    public override void _Ready()
    {
        _currentIngredients = new List<RecipeIngredient>();
        _currentIngredientsContainer = GetNode<HBoxContainer>("%CurrentIngredients");
        _ingredientsScreen = GetNode<Control>("IngredientScreen");
        _bookScreen = GetNode<BookController>("BookScreen");
        _summonScreen = GetNode<SummonController>("SummonScreen");
        
        _summonScreen.RecipeSuccess += recipe =>
        {
            _bookScreen.AddRecipe(recipe);
        };

        var ingredientCount = 0;
        for (var i = 0; i < _ingredientsScreen.GetChildCount(); i++)
        {
            var child = _ingredientsScreen.GetChild(i);
            if (child is not Ingredient ingredient) continue;
            IngredientNodes[ingredientCount] = ingredient;
            ingredientCount++;
            ingredient.IngredientPrepared += recipeIngredient =>
            {
                _currentIngredientsContainer.AddChild(recipeIngredient);
            };
        }
    }

    public void ResetIngredients()
    {
        switch (_currentState)
        {
            case CookingStates.Book:
            {
                _bookScreen.Visible = false;
                _ingredientsScreen.Visible = true;
                break;
            }
            case CookingStates.Summon:
            {
                _summonScreen.Visible = false;
                _ingredientsScreen.Visible = true;
                break;
            }
            case CookingStates.Ingredients:
            {
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        SpawnIngredients();
        _currentState = CookingStates.Ingredients;
    }

    private void SpawnIngredients()
    {
        _currentIngredients = new List<RecipeIngredient>();
        foreach (var ingredient in IngredientNodes)
        {
            ingredient.ResetIngredient();
            if (ingredient.GetParent() != _currentIngredientsContainer) continue;
            _currentIngredientsContainer.RemoveChild(ingredient);
            _ingredientsScreen.AddChild(ingredient);
        }
    }

    public void StartSummon()
    {
        _currentState = CookingStates.Summon;
        foreach (var ingredientNode in IngredientNodes)
        {
            if (ingredientNode.IngredientIsPrepared)
            {
                _currentIngredients.Add(ingredientNode.RecipeIngredient);
            }
        }

        _ingredientsScreen.Visible = false;
        _summonScreen.Visible = true;
        _summonScreen.StartSummon(_currentIngredients);
    }

    public void OpenRecipeBook()
    {
        switch (_currentState)
        {
            case CookingStates.Book:
            {
                _bookScreen.Visible = false;
                if (_restoreIngredientScreen)
                {
                    _ingredientsScreen.Visible = true;
                    _currentState = CookingStates.Ingredients;
                }
                else
                {
                    _summonScreen.Visible = true;
                    _currentState = CookingStates.Summon;
                }

                break;
            }
            case CookingStates.Summon:
            {
                _summonScreen.Visible = false;
                _bookScreen.Visible = true;
                _restoreIngredientScreen = false;
                _currentState = CookingStates.Book;
                break;
            }
            case CookingStates.Ingredients:
            {
                _ingredientsScreen.Visible = false;
                _bookScreen.Visible = true;
                _restoreIngredientScreen = true;
                _currentState = CookingStates.Book;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}