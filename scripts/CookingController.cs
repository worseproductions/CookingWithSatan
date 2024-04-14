using Godot;
using System;
using System.Collections.Generic;
using CookingWithSatan.scripts.resources;

namespace CookingWithSatan.scripts;
public partial class CookingController : Control
{
    private List<RecipeIngredient> currentIngredients;
    private List<RecipeIngredient> allPossibleIngredients;
    [Export] public Ingredient[] ingredientNodes= new Ingredient[6];
    
    
    enum CookingStates
    {
        Ingredients,
        Book,
        Summon
    }

    private CookingStates currentState = CookingStates.Ingredients;
    private bool restoreIngredientScreen = true;
    private Control IngredientsScreen;
    private Control BookScreen;
    private Control SummonScreen;

    public override void _Ready()
    {
        currentIngredients = new List<RecipeIngredient>();
        allPossibleIngredients = new List<RecipeIngredient>();
        IngredientsScreen = GetNode<Control>("IngredientScreen");
        BookScreen = GetNode<Control>("BookScreen");
        SummonScreen = GetNode<Control>("SummonScreen");
    }

    public void ResetIngredients()
    {
        switch (currentState)
        {
            case CookingStates.Book:
            {
                BookScreen.Visible = false;
                IngredientsScreen.Visible = true;
                break;
            }
            case CookingStates.Summon:
            {
                SummonScreen.Visible = false;
                IngredientsScreen.Visible = true;
                break;
            }
            case CookingStates.Ingredients:
            {
                break;
            }
        }
        SpawnIngredients();
        currentState = CookingStates.Ingredients;
        restoreIngredientScreen = true;
    }

    private void SpawnIngredients()
    {
        currentIngredients = new List<RecipeIngredient>();
        foreach (Ingredient ingredient in ingredientNodes)
        {
            ingredient.ResetIngredient();
        }
    }

    public void StartSummon()
    {
        currentState = CookingStates.Ingredients;
        IngredientsScreen.Visible = false;
        SummonScreen.Visible = true;
    }

    public void OpenRecipeBook()
    {
        switch (currentState)
        {
            case CookingStates.Book:
            {
                BookScreen.Visible = false;
                if (restoreIngredientScreen)
                {
                    IngredientsScreen.Visible = true;
                }
                else
                {
                    SummonScreen.Visible = true;
                }
                break;
            }
            case CookingStates.Summon:
            {
                SummonScreen.Visible = false;
                BookScreen.Visible = true;
                restoreIngredientScreen = false;
                break;
            }
            case CookingStates.Ingredients:
            {
                IngredientsScreen.Visible = false;
                BookScreen.Visible = true;
                restoreIngredientScreen = true;
                break;
            }
        }
        currentState = CookingStates.Ingredients;
    }
}
