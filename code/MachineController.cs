using Sandbox;

public sealed class MachineController : Component
{
	[Property]
	public InventoryComponent Inventory{ get; set; }
	[Property]
	public string RecipeType { get; set; }


	private Recipe CurrentRecipe;

	[Sync]
	public float ProcessingTimeLeft { get; set; }
	[Sync]
	public float ProcessingTimeTotal { get; set; }

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		IPowerSource powerSource = Components.Get<IPowerSource>();
		if ( powerSource != null && !powerSource.IsPowered() )
			return;
		if(CurrentRecipe == null )
		{
			CurrentRecipe = Game.ActiveScene.GetAllComponents<RecipeRegistry>().First().ofType( RecipeType ).FirstOrDefault( recipe => recipe.CanCraft( Inventory ), null );
			if ( CurrentRecipe != null )
			{
				CurrentRecipe.ConsumeInputs( Inventory );
				ProcessingTimeLeft = CurrentRecipe.ProcessingTime;
				ProcessingTimeTotal = CurrentRecipe.ProcessingTime;
			} else
			{
				ProcessingTimeTotal = 0;
			}
		}
		ProcessingTimeLeft -= Time.Delta;
		if ( ProcessingTimeLeft < 0 ) ProcessingTimeLeft = 0;
		if(ProcessingTimeLeft == 0 && CurrentRecipe != null)
		{
			CurrentRecipe.AddOutputs( Inventory );
			CurrentRecipe = null;
		}
	}
	public interface IPowerSource
	{
		bool IsPowered();
	}
}
