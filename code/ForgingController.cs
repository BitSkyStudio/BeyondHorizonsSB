using Sandbox;

public sealed class ForgingController : Component
{
	[Property]
	public InventoryComponent Inventory {  get; set; }
	[Property]
	public Recipe SelectedRecipe { get; set; }
	protected override void OnUpdate()
	{

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
		switch ( step )
		{
			case ForgingStep.HitLight: return -3;
			case ForgingStep.HitMedium: return -6;
			case ForgingStep.HitHard: return -9;
			case ForgingStep.Draw: return -15;
			case ForgingStep.Punch: return 2;
			case ForgingStep.Bend: return 7;
			case ForgingStep.Upset: return 13;
			case ForgingStep.Shrink: return 16;
		}
		throw new System.Exception( "invalid step" );
	}
}
