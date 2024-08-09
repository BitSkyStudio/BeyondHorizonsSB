using Sandbox;
using System;

public sealed class HealthComponent : Component
{
	[Property]
	public float MaxHealth { get; set; } = 100f;
	[Property]
	public float Health { get; set; } = 100f;
	[Property]
	public float Regeneration { get; set; } = 0f;
	[Property]
	public Dictionary<ToolType, float> ToolDamageModifiers { get; set; }
	[Property]
	public int Lives { get; set; } = 1;
	[Property]
	public ItemStackRaw LootItem { get; set; } = null;
	protected override void OnUpdate()
	{
		Health = MathF.Min( MaxHealth, Health + Regeneration * Time.Delta );
	}

	[Authority]
	public void Damage(float damage, ToolType tool, InventoryComponent lootInventory )
	{
		Health -= damage * ToolDamageModifiers.GetValueOrDefault(tool, 0);
		if ( Health <= 0 )
		{
			Lives -= 1;
			Health = MaxHealth;
			if(LootItem != null && LootItem.Id.Length > 0 && lootInventory != null)
			{
				lootInventory.NetAddItem( LootItem );
			}
			if ( Lives <= 0 )
			{
				if(Components.GetAll<PlayerController>().Count() > 0 )
				{
					Lives = 1;
					Transform.Position = Game.ActiveScene.GetAllComponents<SpawnPoint>().First().Transform.Position;
				} else
				{
					GameObject.Destroy();
				}
			}
		}
	}
}
