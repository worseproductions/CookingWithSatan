using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CookingWithSatan.scripts.resources;

[GlobalClass]
public partial class Recipe : Resource
{
    [Export] public Array<RecipeIngredient> Ingredients { get; set; }
}