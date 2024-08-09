using Sandbox;
using System;

public sealed class HealthComponent : Component
{
	[Property]
	[Sync]
	public float MaxHealth { get; set; } = 100f;
	[Property]
	[Sync]
	public float Health { get; set; } = 100f;
	[Property]
	public float Regeneration { get; set; } = 0f;
	[Property]
	public Dictionary<ToolType, float> ToolDamageModifiers { get; set; }
	[Property]
	public int Lives { get; set; } = 1;
	[Property]
	public ItemStackRaw LootItem { get; set; } = null;
	[Property]
	public float RegenerationTime {  get; set; } = 10;
	public float SinceLastHit { get; set; } = 0;
	protected override void OnUpdate()
	{
		SinceLastHit += Time.Delta;
		if ( SinceLastHit > RegenerationTime )
		{
			Health = MathF.Min( MaxHealth, Health + Regeneration * Time.Delta );
		}
	}

	[Authority]
	public void Damage(float damage, ToolType tool, InventoryComponent lootInventory )
	{
		SinceLastHit = 0;
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
