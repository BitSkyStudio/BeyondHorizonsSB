using Sandbox;

public sealed class ForgingController : Component
{
	[Property]
	public InventoryComponent Inventory {  get; set; }
	[Property]
	public Recipe SelectedRecipe { get; set; } = null;
	[Sync]
	public string SelectedRecipeId { get; set; } = "";
	[Sync]
	[Property]
	public int Progress { get; set; }
	protected override void OnUpdate()
	{
		
	}
	[Authority]
	public void SelectRecipe( string recipe )
	{
		SelectedRecipe = Game.ActiveScene.GetAllComponents<RecipeRegistry>().First().Recipes[recipe];
		if ( !SelectedRecipe.CanCraft( Inventory ) )
		{
			SelectedRecipe = null;
			return;
		}
		SelectedRecipe.ConsumeInputs( Inventory );
		SelectedRecipeId = recipe;
		Progress = 0;
	}
	[Authority]
	public void ForgeStep( ForgingStep step )
	{
		if ( SelectedRecipe == null )
			return;
		Progress += GetStepOffset( step );
		if ( Progress < 0 || Progress > 100 )
		{
			SelectedRecipe = null;
			SelectedRecipeId = "";
			Progress = 0;
		} else if ( Progress == SelectedRecipe.ProcessingTime )
		{
			SelectedRecipe.AddOutputs( Inventory );
			Progress = 0;
			SelectedRecipe = null;
			SelectedRecipeId = "";
		}
	}

	public enum ForgingStep
	{
		HitLight,
		HitMedium,
		HitHard,
		Draw,
		Punch,
		Bend,
		Upset,
		Shrink
	}
	public static int GetStepOffset( ForgingStep step )
	{
		return step switch
		{
			ForgingStep.HitLight => -3,
			ForgingStep.HitMedium => -6,
			ForgingStep.HitHard => -9,
			ForgingStep.Draw => -15,
			ForgingStep.Punch => 2,
			ForgingStep.Bend => 7,
			ForgingStep.Upset => 13,
			ForgingStep.Shrink => 16,
			_ => throw new System.Exception( "invalid step" ),
		};
	}
}
