using Sandbox;

public sealed class RecipeRegistry : Component
{
	[Property]
	public Dictionary<string,Recipe> Recipes {  get; set; }

	public IEnumerable<Recipe> ofType(string type )
	{
		return Recipes.Values.Where( recipe => recipe.RecipeType == type );
	}
}
