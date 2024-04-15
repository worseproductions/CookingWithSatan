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
    private SummonController _summonScreen;

    public override void _Ready()
    {
        currentIngredients = new List<RecipeIngredient>();
        allPossibleIngredients = new List<RecipeIngredient>();
        
       
        
        IngredientsScreen = GetNode<Control>("IngredientScreen");
        BookScreen = GetNode<Control>("BookScreen");
        _summonScreen = GetNode<SummonController>("SummonScreen");

        int ingrCount = 0;
        for (int i = 0; i < IngredientsScreen.GetChildCount(); i++)
        {
            Node child = IngredientsScreen.GetChild(i);
            if (child is Ingredient ingredient)
            {
                ingredientNodes[ingrCount] = ingredient;
                ingrCount++;
            }
        }
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
                _summonScreen.Visible = false;
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
        currentState = CookingStates.Summon;
        foreach (Ingredient ingredientNode in ingredientNodes)
        {
            if (ingredientNode.ingredientIsPrepared)
            {
                currentIngredients.Add(ingredientNode.RecipeIngredient);
            }
        }
        IngredientsScreen.Visible = false;
        _summonScreen.Visible = true;
        _summonScreen.StartSummon(currentIngredients);
        //TODO: start summon animation
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
                    currentState = CookingStates.Ingredients;
                }
                else
                {
                    _summonScreen.Visible = true;
                    currentState = CookingStates.Summon;
                }
                break;
            }
            case CookingStates.Summon:
            {
                _summonScreen.Visible = false;
                BookScreen.Visible = true;
                restoreIngredientScreen = false;
                currentState = CookingStates.Book;
                break;
            }
            case CookingStates.Ingredients:
            {
                IngredientsScreen.Visible = false;
                BookScreen.Visible = true;
                restoreIngredientScreen = true;
                currentState = CookingStates.Book;
                break;
            }
        }
    }
}
