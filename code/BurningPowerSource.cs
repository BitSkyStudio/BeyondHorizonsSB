using Sandbox;

public sealed class BurningPowerSource : Component, MachineController.IPowerSource
{
	[Sync]
	[Property]
	public float Fuel { get; set; }
	[Property]
	public float MaxFuel { get; set; } = 100;
	[Property]
	public float FuelConsumptionRate { get; set; } = 1;
	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;
		Fuel -= Time.Delta * FuelConsumptionRate;
		if(Fuel < 0 )
		{
			Fuel = 0;
		}
	}
	public bool IsPowered()
	{
		return Fuel > 0;
	}
	[Authority]
	public void AddFuel( float fuel )
	{
		Fuel = MathX.Clamp(Fuel+fuel, 0, MaxFuel);
	}
}
