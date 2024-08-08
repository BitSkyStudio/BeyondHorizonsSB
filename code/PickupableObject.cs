using Sandbox;

public sealed class PickupableObject : Component
{
	[Property]
	public string ItemId { get; set; }
	[Property]
	public float Time { get; set; } = 1f;
}
