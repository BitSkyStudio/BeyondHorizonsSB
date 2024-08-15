using Sandbox;

public sealed class GameObjectRespawner : Component
{
	[Property]
	public GameObject Prefab {  get; set; }
	[Property]
	public float RespawnTime { get; set; }
	public GameObject Object { get; set; } = null;
	public float Timer { get; set; } = 0;
	protected override void OnEnabled()
	{
		Timer = RespawnTime;
	}
	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		if(Object == null || !Object.IsValid )
		{
			Timer += Time.Delta;
			if(Timer >= RespawnTime )
			{
				Object = Prefab.Clone(Transform.Position, Transform.Rotation);
				Object.NetworkSpawn();
			}
		} else
		{
			Timer = 0;
		}
	}
}
