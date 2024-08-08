using Sandbox;

public sealed class RecipeRegistry : Component
{
	[Property]
	public Dictionary<string,Recipe> recipes {  get; set; }

	public IEnumerable<Recipe> ofType(string type )
	{
		return recipes.Values.Where( recipe => recipe.RecipeType == type );
	}
}
