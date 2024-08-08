using Sandbox;

public sealed class CameraComponentAssigner : Component
{
	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		PlayerController pc = Game.ActiveScene.GetAllComponents<PlayerController>().First( x => x.Network.IsOwner );
		if ( pc != null )
		{
			pc.Camera = this.GameObject;
		}
	}
}
