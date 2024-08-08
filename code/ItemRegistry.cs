using Sandbox;

public sealed class ItemRegistry : Component
{
	[Property]
	public Dictionary<string, Item> Items { get; set; }
}
