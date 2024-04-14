using Godot;

namespace CookingWithSatan.scripts.resources;

[GlobalClass]
public partial class RecipeIngredient : Resource
{
    [Export] public string Name { get; set; }
    [Export] public Texture Image { get; set; }
}