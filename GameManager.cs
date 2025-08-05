/// <summary>
/// 
/// Hello, welcome to the GameManager!
/// 
/// So this script runs literally everything and is always in the background
/// However, never use it for game logic
/// Currently we are storing the PlayerInfo
///     public string Name;
///     public long Id;
/// 
/// If you are making changes here, good luck x
/// Jk you will be fine
/// 
/// </summary>

using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static List<PlayerInfo> Players = new List<PlayerInfo>();
}
