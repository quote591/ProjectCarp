/// <summary>
/// 
/// Oh how much I love this controller
/// 
/// I would recommend all lines with '// Multiplayer'
/// to be changed with caution
/// 
/// but all the physics code, go crazy
/// Currenlty the multiplayer synchroniser is only syncing the players'
/// 	Postion
/// 	Rotation
/// its possible to add more by going
/// 1) Into the MultiplayerSynchronizer
/// 2) Replication Window (bottom middle)
/// 3) + Add property to sync
/// 
/// TODO:
/// currently when the player spawns, its in the air			(low priority)
/// till the user clicks on the window
/// 
/// change the interval for the multiplayerSynchronizer			(low priority)
/// currently its set to 0 (fast as possible)
/// however as the scene gets more complicated and harder to run
/// we will need to increase this, 
/// and then fix the stuttering that happens because of it
/// 
/// add cool af movement stuff 									(High Priority)
/// 
/// /summary>

using Godot;
using Interaction;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float CamSensitivity = 0.006f;

	private Node3D _head;
	private Camera3D _cam;

	private bool camDisabled = false;
	private bool _isActive = true;

	public long PlayerId { get; set; } = 0; // Multiplayer

	// synced varibles
    public int Score { get; set; } = 0;

	// Label variables
	private Label3D hudLabel;

	public override void _Ready()
	{
		// When loading the players, and each one has a MultiplayerSynchronizer
		// we set the authority of each one to be the user's UniqueID (we are parsing name at the moment)
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name)); // Multiplayer
		SetMultiplayerAuthority(GetMultiplayerAuthority()); // Multiplayer


		_head = GetNode<Node3D>("Head");
		_cam = GetNode<Camera3D>("Head/Camera3D");

		if (GetMultiplayerAuthority() == PlayerId && GetMultiplayerAuthority() == Multiplayer.GetUniqueId()) // Multiplayer
			_cam.Current = true; // Multiplayer 
		else // Multiplayer
			_cam.Current = false; // Multiplayer

		// Hud text
		hudLabel = new Label3D();
		hudLabel.Hide();
		AddChild(hudLabel);

	}

	public void SetActive(bool active)
    {
        _isActive = active;
        _cam.Current = _isActive;

        Input.MouseMode = _isActive && !camDisabled ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
    }
	
	public override void _Input(InputEvent @event)
    {
        if (!_isActive) return;

		if (@event is InputEventMouseMotion m && !camDisabled)
		{
			//float yaw = -m.Relative.X * CamSensitivity;
			_head.RotateY(-m.Relative.X * CamSensitivity);
			_cam.RotateX(-m.Relative.Y * CamSensitivity);


			Vector3 camRot = _cam.Rotation;
			float pitch = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
			camRot.X = pitch;
			_cam.Rotation = camRot;
        }
		else if (@event is InputEventKey k && k.Keycode == Key.Escape && k.IsPressed())
		{
			camDisabled = !camDisabled;
			Input.MouseMode = camDisabled ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		}
    }

	public override void _PhysicsProcess(double delta)
	{
		// If player is a remote player (aka not the player in the window)
		// dont process anything
		if (PlayerId != Multiplayer.GetUniqueId()) return; // Multiplayer
		if (!DisplayServer.WindowIsFocused()) return; // Multiplayer

		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId()) // Multiplayer
		{
			// ===== Start Physics Code =====



			Vector3 velocity = Velocity;

			if (!IsOnFloor())
			{
				velocity += GetGravity() * (float)delta;
			}

			if (Input.IsActionJustPressed("jump") && IsOnFloor())
			{
				velocity.Y = JumpVelocity;
			}

			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
			Vector3 direction = (_head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed * (float)delta);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed * (float)delta);
			}

			Velocity = velocity;
			MoveAndSlide();


			// Raycasting for intersections
			var spaceState = GetWorld3D().DirectSpaceState;

			#region RayDebugging
			/*
			// Debugging code to show rays
			var mesh_intance = new MeshInstance3D();
			var immediate_mesh = new ImmediateMesh();
			var material = new OrmMaterial3D();

			mesh_intance.Mesh = immediate_mesh;

			immediate_mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
			immediate_mesh.SurfaceAddVertex(_cam.GlobalPosition);
			immediate_mesh.SurfaceAddVertex(_cam.GlobalPosition - _cam.GlobalTransform.Basis.Z * 2);
			immediate_mesh.SurfaceEnd();

			material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
			material.AlbedoColor = Color.Color8(1, 0, 0, 1);

			GetTree().Root.AddChild(mesh_intance);
			*/
			#endregion

			// Get any ray intersections here 
			var query = PhysicsRayQueryParameters3D.Create(_cam.GlobalPosition, _cam.GlobalPosition - _cam.GlobalTransform.Basis.Z * 2);
			query.CollisionMask = 1U << InteractionManager.collisionMask;
			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				Node collider = (Node)result["collider"];
				if (collider is Node3D and InteractableInterface interactCol)
				{
					// Can show any "press E to interact code here"
					if (Input.IsKeyPressed(Key.E))
					{
						interactCol.OnInteraction(this);
					}
				}
			}
			// ===== End Physics Code =====
			}
	}
}