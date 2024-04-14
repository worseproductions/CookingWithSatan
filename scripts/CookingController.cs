using Godot;
using System;
namespace CookingWithSatan.scripts;
public partial class CookingController : Control
{
    enum CookingStates
    {
        Ingredients,
        Book,
        Summon
    }

    private CookingStates currentState = CookingStates.Ingredients;
    private Control IngredientsScreen;
    private Control BookScreen;
    private Control SummonScreen;

    public override void _Ready()
    {
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
        //TODO: spawn ingredients
        currentState = CookingStates.Ingredients;
    }

    public void SpawnIngredients()
    {
        
    }

    public void StartSummon()
    {
        currentState = CookingStates.Ingredients;
        IngredientsScreen.Visible = false;
        SummonScreen.Visible = true;
    }

    public void OpenRecipeBook()
    {
        //TODO: Handle close book (and return to ingredient/Summon state
        switch (currentState)
        {
            case CookingStates.Book:
            {
                break;
            }
            case CookingStates.Summon:
            {
                SummonScreen.Visible = false;
                BookScreen.Visible = true;
                break;
            }
            case CookingStates.Ingredients:
            {
                IngredientsScreen.Visible = false;
                BookScreen.Visible = true;
                break;
            }
        }
        currentState = CookingStates.Ingredients;
    }
}
