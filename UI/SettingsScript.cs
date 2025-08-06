using Godot;
using System;

public partial class SettingsScript : Control
{
    public void _on_quit_button_down()
    {
        GetTree().Quit();
    }
}
