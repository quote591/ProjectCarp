using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	public const float Speed = 1000.0f;
	public const float JumpVelocity = 4.5f;
	public const float CamSensitivity = 0.006f;
	
	private Node3D _head;
	private Camera3D _cam;

	private bool camDisabled = false;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_head = GetNode<Node3D>("Head");
		_cam = GetNode<Camera3D>("Head/Camera3D");
	}

	public override void _Input(InputEvent @event)
	{
		
		if (@event is InputEventMouseMotion m && !camDisabled)
		{
			_head.RotateY(-m.Relative.X * CamSensitivity);
			_cam.RotateX(-m.Relative.Y * CamSensitivity);

			Vector3 camRot = _cam.Rotation;
			camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
			_cam.Rotation = camRot;
		}

		// exit mouse captured mode with Escape
		else if (@event is InputEventKey k && k.Keycode == Key.Escape)
		{
			camDisabled = !camDisabled; 
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}


	public override void _PhysicsProcess(double delta)
	{
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
	}
}
