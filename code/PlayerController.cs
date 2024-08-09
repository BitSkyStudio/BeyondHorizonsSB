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
	public float InteractionDistance { get; set; } = 200f;

	public Angles EyeAngles { get; set; }

	public Vector3 EyeWorldPosition => Transform.Local.PointToWorld( Vector3.Up * EyeHeight );

	public int SelectedSlot = 0;
	public int Slots => PlayerInventory.Size;
	public InventoryComponent PlayerInventory => Components.Get<InventoryComponent>();

	public HealthComponent Health => Components.Get<HealthComponent>();

	public float PickupProgress = 0f;
	public PickupableObject PickingUpObject = null;

	public GameObject placingObject;
	public string placingObjectId;

	public float AttackCooldown = 0;
	public float MaxAttackCooldown = 0;

	public HealthComponent TargetedHealth = null;

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
		for ( int i = 1; i < Slots; i++ )
		{
			if ( Input.Pressed( "Slot" + i ) )
			{
				SelectedSlot = i-1;
			}
		}
		if ( Input.Pressed( "Slot0" ) )
		{
			SelectedSlot = Slots-1;
		}
		SelectedSlot = ((SelectedSlot % Slots) + Slots) % Slots;
		var cameraTraceSetup = Scene.Trace.Ray( EyeWorldPosition, EyeWorldPosition + EyeAngles.Forward * InteractionDistance ).Size( 5f ).IgnoreGameObjectHierarchy( GameObject );
		if(placingObject != null )
		{
			cameraTraceSetup = cameraTraceSetup.IgnoreGameObjectHierarchy( placingObject );
		}
		var cameraTrace = cameraTraceSetup.Run();
		bool keepPickupProgress = false;
		bool keepPlacing = false;
		if ( cameraTrace.Hit && AttackCooldown == 0)
		{
			PickupableObject pickupableObject = cameraTrace.GameObject.Components.Get<PickupableObject>();
			if ( Input.Pressed( "attack2" ) || (Input.Down("attack2") && PickingUpObject != null) )
			{
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
						PlayerInventory.AddItem( ItemStack.Create( pickupableObject.ItemId, 1 ) );
						keepPickupProgress = false;
					}
				}
			}
			if ( cameraTrace.GameObject.Components.Get<Terrain>() != null)
			{
				ItemStack stack = PlayerInventory.GetAt(SelectedSlot);
				if ( stack != null )
				{
					if ( !stack.ItemType.Id.Equals(placingObjectId))
					{
						placingObjectId = stack.ItemType.Id;
						if ( placingObject != null )
							placingObject.Destroy();
						placingObject = stack.ItemType.Prefab.Clone( cameraTrace.EndPosition, Rotation.FromYaw(EyeAngles.yaw) );
						placingObject.BreakFromPrefab();
						placingObject.Components.Get<BoxCollider>().IsTrigger = true;
					} else
					{
						placingObject.Transform.Position = cameraTrace.EndPosition;
						placingObject.Transform.Rotation = Rotation.FromYaw( EyeAngles.yaw );
						bool blocked = placingObject.Components.Get<BoxCollider>().Touching.Count() > 0;
						placingObject.Components.GetAll<ModelRenderer>().First().Tint = blocked?Color.Red:Color.Green;
						if ( Input.Pressed( "attack2" ) && !blocked )
						{
							var realObject = stack.ItemType.Prefab.Clone( cameraTrace.EndPosition, Rotation.FromYaw( EyeAngles.yaw ) );
							realObject.NetworkSpawn(Connection.Host);
							stack.Count -= 1;
							PlayerInventory.SetAt( SelectedSlot, stack );
						}
					}

					keepPlacing = true;
				}
			}
		}
		TargetedHealth = cameraTrace.Hit?cameraTrace.GameObject.Components.Get<HealthComponent>():null;

		AttackCooldown -= Time.Delta;
		if(AttackCooldown < 0 )
		{
			AttackCooldown = 0;
			Animator.HoldType = CitizenAnimationHelper.HoldTypes.None;
		}
		if ( Input.Down( "attack1" ) && AttackCooldown == 0)
		{
			Animator.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
			Animator.Target.Set( "b_attack", true );
			AttackCooldown = 0.5f;
			ItemStack stack = PlayerInventory.GetAt( SelectedSlot );
			ToolType tool = ToolType.None;
			float damage = 10;
			if ( stack != null )
			{
				AttackCooldown = stack.ItemType.UseTime;
				tool = stack.ItemType.ToolType;
				damage = stack.ItemType.Damage;
			}
			MaxAttackCooldown = AttackCooldown;
			if ( cameraTrace.Hit )
			{
				if(cameraTrace.Surface != null )
				{
					Sound.Play( cameraTrace.Surface.Sounds.ImpactHard, cameraTrace.EndPosition );
				}
				if ( TargetedHealth != null )
				{

					TargetedHealth.Damage( damage, tool, PlayerInventory );
				}
			}
		}
		if ( !keepPlacing )
		{
			if ( placingObject != null )
				placingObject.Destroy();
			placingObject = null;
			placingObjectId = null;
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
