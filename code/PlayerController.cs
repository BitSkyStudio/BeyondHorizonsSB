using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerController : Component
{
	public GameObject Camera { get; set; }

	[Property]
	[Category( "Components" )]
	public CharacterController Controller { get; set; }

	[Property]
	[Category( "Components" )]
	public CitizenAnimationHelper Animator { get; set; }

	/// <summary>
	/// How fast you can walk (Units per second)
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 400f, 1f )]
	public float WalkSpeed { get; set; } = 120f;

	/// <summary>
	/// How fast you can run (Units per second)
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 800f, 1f )]
	public float RunSpeed { get; set; } = 250f;

	/// <summary>
	/// How powerful you can jump (Units per second)
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 1000f, 10f )]
	public float JumpStrength { get; set; } = 400f;

	[Property]
	[Category( "Stats" )]
	[Range( 0f, 1000f, 10f )]
	public float EyeHeight { get; set; } = 55f;

	[Property]
	public float MaxHealth { get; set; } = 100f;

	[Property]
	public float Health { get; set; } = 75f;

	[Property]
	public float InteractionDistance { get; set; } = 200f;

	public Angles EyeAngles { get; set; }

	public Vector3 EyeWorldPosition => Transform.Local.PointToWorld( Vector3.Up * EyeHeight );

	public Inventory PlayerInventory = new Inventory(10);

	public int SelectedSlot = 0;
	public int Slots => PlayerInventory.items.Length;

	public float PickupProgress = 0f;
	public PickupableObject PickingUpObject = null;


	protected override void OnUpdate()
	{
		foreach ( SkinnedModelRenderer renderer in Components.GetAll<SkinnedModelRenderer>() ) {
			renderer.RenderType = Network.IsProxy ? ModelRenderer.ShadowRenderType.On : ModelRenderer.ShadowRenderType.ShadowsOnly;
		}

		if ( IsProxy )
			return;

		//Camera.Components.GetAll<CameraComponent>().First().Enabled = true;

		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( MathX.Clamp( EyeAngles.pitch, -80f, 80f ) );
		Transform.Rotation = Rotation.FromYaw( EyeAngles.yaw );
		//Transform.Rotation = EyeAngles;	
		if ( Camera != null )
		{
			Camera.Transform.LocalRotation = Rotation.From( EyeAngles );
			Camera.Transform.LocalPosition = EyeWorldPosition;
		}

		if ( Input.Pressed( "SlotPrev" ) )
		{
			SelectedSlot -= 1;
		}
		if ( Input.Pressed( "SlotNext" ) )
		{
			SelectedSlot += 1;
		}
		for ( int i = 0; i < Slots; i++ )
		{
			if ( Input.Pressed( "Slot" + i ) )
			{
				SelectedSlot = i;
			}
		}
		SelectedSlot = ((SelectedSlot % Slots) + Slots) % Slots;
		var cameraTrace = Scene.Trace.Ray( EyeWorldPosition, EyeWorldPosition + EyeAngles.Forward * InteractionDistance ).Size( 5f ).IgnoreGameObjectHierarchy( GameObject ).Run();
		bool keepPickupProgress = false;
		if ( cameraTrace.Hit )
		{
			if ( Input.Down( "attack1" ) )
			{
				PickupableObject pickupableObject = cameraTrace.GameObject.Components.Get<PickupableObject>();
				if ( pickupableObject != null )
				{
					if ( pickupableObject == PickingUpObject )
					{
						PickupProgress += Time.Delta;
					} else
					{
						PickupProgress = 0;
						PickingUpObject = pickupableObject;
					}
					keepPickupProgress = true;
					if ( PickupProgress >= pickupableObject.Time )
					{
						cameraTrace.GameObject.Network.TakeOwnership();
						cameraTrace.GameObject.Destroy();
						PlayerInventory.addItem( ItemStack.Create( pickupableObject.ItemId ) );
						keepPickupProgress = false;
					}
				}
			}
		}
		if ( !keepPickupProgress )
		{
			PickupProgress = 0;
			PickingUpObject = null;
		}
		
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsProxy )
			return;

		if ( Controller == null ) return;

		var wishSpeed = Input.Down( "Run" ) ? RunSpeed : WalkSpeed;
		var wishVelocity = Input.AnalogMove.Normal * wishSpeed * Transform.Rotation;

		Controller.Accelerate( wishVelocity );

		if ( Controller.IsOnGround )
		{
			Controller.Acceleration = 10f;
			Controller.ApplyFriction( 5f, 20f );

			if ( Input.Pressed( "Jump" ) )
			{
				Controller.Punch( Vector3.Up * JumpStrength );

				if ( Animator != null )
					Animator.TriggerJump();
			}
		}
		else
		{
			Controller.Acceleration = 5f;
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}

		Controller.Move();

		if ( Animator != null )
		{
			Animator.IsGrounded = Controller.IsOnGround;
			Animator.WithVelocity( Controller.Velocity );
		}
	}

	protected override void OnStart()
	{
		if ( Components.TryGet<SkinnedModelRenderer>( out var model ) )
		{
			var clothing = ClothingContainer.CreateFromLocalUser();
			clothing.Apply( model );
		}
	}
}
