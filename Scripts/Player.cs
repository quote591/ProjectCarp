using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float CamSensitivity = 0.006f;
	
	private Node3D _head;
	private Camera3D _cam;

	// no clue how camDisabled works but im gonna change it a bit and make escape work a little nicer
	private bool camDisabled = false;
	private bool _isActive = true;

	// more boring ahh multiplayer varibles
	public long PlayerId { get; set; } = 0;

	public override void _Ready()
	{
		// // Multiplayer Check (check authority)
		// // honeslty dont worry about this x
		// //GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		// GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer")
		// 	.SetMultiplayerAuthority((int)PlayerId); // cast if the API takes int
		// GD.Print($"[Player {PlayerId}] GetMultiplayerAuthority: {GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority()}");
		// GD.Print($"[Player {PlayerId}] GetUniqueId: {Multiplayer.GetUniqueId()}");

		// //Input.MouseMode = Input.MouseModeEnum.Captured;
		// _head = GetNode<Node3D>("Head");
		// _cam = GetNode<Camera3D>("Head/Camera3D");

		var authority = (int)PlayerId;
		var localId = Multiplayer.GetUniqueId();

		//GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer")
		//	.SetMultiplayerAuthority(authority);
		var ms = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
    	ms.SetMultiplayerAuthority((int)PlayerId);

		bool isLocal = authority == localId;

		_head = GetNode<Node3D>("Head");
		_cam = GetNode<Camera3D>("Head/Camera3D");

		_cam.Current = isLocal; // only set active camera for local player

		GD.Print($"[{Name}] isLocal={isLocal}, authority={authority}, localId={localId}");
	}

	public void SetActive(bool active)
    {
        _isActive = active;
        _cam.Current = _isActive;

        // If inactive, show mouse; if active and camDisabled false, capture mouse
        Input.MouseMode = _isActive && !camDisabled ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
    }

	// public override void _Input(InputEvent @event)
	// {

	// 	if (@event is InputEventMouseMotion m && !camDisabled)
	// 	{
	// 		_head.RotateY(-m.Relative.X * CamSensitivity);
	// 		_cam.RotateX(-m.Relative.Y * CamSensitivity);

	// 		Vector3 camRot = _cam.Rotation;
	// 		camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
	// 		_cam.Rotation = camRot;
	// 	}

	// 	// exit mouse captured mode with Escape
	// 	else if (@event is InputEventKey k && k.Keycode == Key.Escape)
	// 	{
	// 		camDisabled = !camDisabled;
	// 		Input.MouseMode = Input.MouseModeEnum.Visible;
	// 	}
	// }
	
	public override void _Input(InputEvent @event)
    {
        if (!_isActive) return;

        if (@event is InputEventMouseMotion m && !camDisabled)
        {
            _head.RotateY(-m.Relative.X * CamSensitivity);
            _cam.RotateX(-m.Relative.Y * CamSensitivity);

            Vector3 camRot = _cam.Rotation;
            camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
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
		// before doing anything, check that we are the boss
		// oh and checking the client is actully actibe
		if (PlayerId != Multiplayer.GetUniqueId())
		{
			// This is a remote player, don't process input movement here
			return;
		}
		if (!DisplayServer.WindowIsFocused())
		{
			return;
		}

		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			// ===== Start Physics Code =====



			Vector3 velocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor())
			{
				velocity += GetGravity() * (float)delta;
			}

			// Handle Jump.
			if (Input.IsActionJustPressed("jump") && IsOnFloor())
			{
				velocity.Y = JumpVelocity;
			}

			// Get the input direction and handle the movement/deceleration.
			// As good practice, you should replace UI actions with custom gameplay actions.
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

			// ===== End Physics Code =====
		}
	}
}