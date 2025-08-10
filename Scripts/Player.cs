using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	public float Speed = 5.0f;
	public const float WalkSpeed = 5.0f;
	public const float SprintSpeed = 8.0f;
	public const float CrouchSpeed = 2.0f;
	public const float JumpVelocity = 4.5f;
	public const float CamSensitivity = 0.003f;

	private Node3D _head;
	private Camera3D _cam;
	private CollisionObject3D _body_collision;
	private AnimationPlayer _animation;
	private ShapeCast3D _crouch_shapecast;
	private CollisionObject3D _head_collision;

	
	private bool is_sprinting = false;
	private bool is_crouching = false;
	private bool camDisabled = false;


	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_head = GetNode<Node3D>("Head");
		_cam = GetNode<Camera3D>("Head/Camera3D");
		_head_collision = GetNode<CollisionObject3D>("Head/HeadMesh/HeadRigidBody");
		_body_collision = GetNode<CollisionShape3D>("BodyCollision").GetParent<CollisionObject3D>();
		_animation = GetNode<AnimationPlayer>("AnimationPlayer");
		_crouch_shapecast = GetNode<ShapeCast3D>("Head/CrouchShapeCast");
		_crouch_shapecast.AddException(_head_collision);
		_crouch_shapecast.AddException(_body_collision);
	}

	public override void _UnhandledInput(InputEvent @event)
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
		if (@event.IsActionPressed("ui_cancel"))
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

		// handle holding to crouch
		if (Input.IsActionPressed("crouch"))
		{
			crouch();
		}
		// uncrouch if we aren't crouching and pass the checks
		else if (is_crouching == true && _crouch_shapecast.IsColliding() == false)
		{
			_animation.Play("player_crouch", -1, -CrouchSpeed);
			is_crouching = false;
		}

		// handle speed, crouching takes priority
		if (is_crouching)
		{
			Speed = CrouchSpeed;
		}
		else
		{
			if (Input.IsActionPressed("sprint"))
			{
				Speed = SprintSpeed;
			}
			else
			{
				Speed = WalkSpeed;
			}
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

	public void crouch()
	{
		if (is_crouching == false)
		{
			_animation.Play("player_crouch", -1, CrouchSpeed);
			is_crouching = true;
		}
	}
}
