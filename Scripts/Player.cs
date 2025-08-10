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
/// /summary>

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

	private bool camDisabled = false;
	private bool _isActive = true;

	public long PlayerId { get; set; } = 0; // Multiplayer

	// synced varibles
	public int Score { get; set; } = 0;
	private bool _isSettingsVisible = false;

	[Export]
    public PackedScene SettingsMenuScene { get; set; }

	private Control _settingsMenuInstance;

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

		Input.MouseMode = Input.MouseModeEnum.Captured;
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
			SettingsMenuToggle();
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

			// ===== End Physics Code =====
		}
	}

	public void SettingsMenuToggle()
{
    if (_settingsMenuInstance == null || !_settingsMenuInstance.IsInsideTree())
    {
        // Instantiate and add to the UI tree
        _settingsMenuInstance = SettingsMenuScene.Instantiate<Control>();

        // Add it to your UI hierarchy (e.g., as a child of a CanvasLayer or main UI Control)
        // For example, add to the root viewport:
        GetTree().Root.CallDeferred("add_child", _settingsMenuInstance);

        // Or better: add to a dedicated UI parent (e.g., `CanvasLayer` or `UIRoot`)
        // GetNode("UIRoot").CallDeferred("add_child", _settingsMenuInstance);
    }
    else
    {
        // Remove the menu
        _settingsMenuInstance.QueueFree();
        _settingsMenuInstance = null;
    }
}
}

