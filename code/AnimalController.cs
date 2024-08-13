using Sandbox;

public sealed class AnimalController : Component
{
	[Property]
	public NavMeshAgent Agent { get; set; }

	public bool Set = false;
	protected override void OnUpdate()
	{
		//if ( IsProxy ) return;
		//if ( Set ) return;
		//Vector3? newPosition = Scene.NavMesh.GetRandomPoint( Transform.Position, 1000 );
		//Agent.MoveTo( Game.ActiveScene.GetAllComponents<PlayerController>().First().Transform.Position );
		//Log.Info( Agent.WishVelocity );
		//Set = true;
		return;
		Log.Info( Scene.NavMesh );
		Vector3? point = Scene.NavMesh.GetClosestPoint( new Vector3( 0, 0, 0 ), 1000 );
		//Vector3? point = Scene.NavMesh.GetRandomPoint();
		if ( point != null )
		{
			Log.Info( "f" + point );
			Agent.MoveTo( Game.ActiveScene.GetAllComponents<PlayerController>().First(  ).Transform.Position );
			Set = true;
		} else
		{
			Log.Info( "pn" );
		}
	}
}
