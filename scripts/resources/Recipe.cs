using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CookingWithSatan.scripts.resources;

[GlobalClass]
public partial class Recipe : Resource
{
    [Export] public Array<RecipeIngredient> Ingredients { get; set; }
    [Export] public string Name { get; set; }
    [Export(PropertyHint.MultilineText)] public string Description { get; set; }
    [Export] public Texture2D Image { get; set; }
}