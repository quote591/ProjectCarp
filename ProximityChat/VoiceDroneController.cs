/// <summary>
/// 
/// Welcome to voice drone controller!
/// 
/// this works by dynamically making and assigning drones to everyone
/// a drone is a 3d node which has a 3d audio stream generator attached
/// each drone will be called Drone{playerUniqueId}
/// 
/// </summary>

using Godot;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

public partial class VoiceDroneController : Node3D
{
    private Dictionary<int, Drone> dronesByPlayerId = new Dictionary<int, Drone>();
    private List<Node3D> players = new List<Node3D>();

    [Export]
    public PackedScene DroneScene { get; set; }

    public override void _Ready()
    {

    }

    // this is called from the scene
    public void RegisterPlayer(Node3D player)
    {
        if (player.GetMultiplayer() == null || player.GetMultiplayer().GetMultiplayerPeer() == null)
        {
            GD.PrintErr("Player multiplayer peer not assigned yet");
            return;
        }
        int playerId = player.GetMultiplayerAuthority();

        if (dronesByPlayerId.ContainsKey(playerId))
        {
            GD.PrintErr($"RegisterPlayer: Drone already exists for player {playerId}");
            return;
        }

        players.Add(player);
        GD.Print($"[{Multiplayer.GetUniqueId()}] RegisterPlayer: Adding drone for player {playerId}");
        CreateDroneForPlayer(player);
    }

    public void UnregisterPlayer(Node3D player)
    {
        if (player.GetMultiplayer() == null || player.GetMultiplayer().GetMultiplayerPeer() == null)
        {
            GD.PrintErr("Player multiplayer peer not assigned yet");
            return;
        }
        int playerId = player.Multiplayer.GetUniqueId();

        if (players.Remove(player))
        {
            GD.Print($"UnregisterPlayer: Removing drone for player {playerId}");
            RemoveDroneForPlayer(playerId);
        }
        else
        {
            GD.PrintErr($"Player {playerId} was not registered.");
        }
    }

    private void CreateDroneForPlayer(Node3D player)
    {
        int playerId = player.GetMultiplayerAuthority();

        if (dronesByPlayerId.ContainsKey(playerId))
        {
            GD.PrintErr($"Drone already exists for player {playerId}");
            return;
        }

        if (DroneScene == null)
        {
            GD.PrintErr("DroneScene is not assigned in the inspector!");
            return;
        }

        Drone newDrone = DroneScene.Instantiate<Drone>();
        newDrone.Name = "Drone" + playerId.ToString();
        AddChild(newDrone);

        newDrone.SetTarget(player);

        dronesByPlayerId[playerId] = newDrone;

        GD.Print($"Created drone {newDrone.Name} for player {playerId}");
    }

    private void RemoveDroneForPlayer(int playerId)
    {
        if (dronesByPlayerId.TryGetValue(playerId, out Drone drone))
        {
            drone.QueueFree();
            dronesByPlayerId.Remove(playerId);
            GD.Print($"Removed drone for player {playerId}");
        }
        else
        {
            GD.PrintErr($"No drone found for player {playerId} to remove");
        }
    }

    private Node3D GetDroneByPlayerId(int playerId)
    {
        if (playerId <= 0)
        {
            GD.PrintErr($"Invalid playerId {playerId} passed to GetDroneByPlayerId");
            return null;
        }

        if (dronesByPlayerId.TryGetValue(playerId, out Drone drone))
            return drone;

        GD.PrintErr($"Drone for player {playerId} not found!");
        return null;
    }

    [Rpc(mode: MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReceiveMicDataFromPlayer(int playerId, byte[] audioData)
    {
        // log everything
        //GD.Print($"[{Multiplayer.GetUniqueId()}] ReceiveMicDataFromPlayer called with playerId {playerId}, audioData length: {audioData?.Length ?? 0}");

        if (playerId <= 0)
        {
            GD.PrintErr($"[{Multiplayer.GetUniqueId()}] ReceiveMicDataFromPlayer called with invalid playerId {playerId}");
            return;
        }
        PlayPlayerAudioOnDrone(playerId, audioData);
    }

    public void PlayPlayerAudioOnDrone(int playerId, byte[] audioData)
    {
        if (playerId <= 0)
        {
            GD.PrintErr($"PlayPlayerAudioOnDrone called with invalid playerId {playerId}");
            return;
        }

        var drone = GetDroneByPlayerId(playerId);
        if (drone == null)
            return;

        var audioPlayer = drone.GetNode<AudioStreamPlayer3D>("3DAudio");
        if (audioPlayer.Stream is AudioStreamGenerator)
        {
            if (!audioPlayer.Playing)
            {
                audioPlayer.Play();
                GD.Print("[" + Multiplayer.GetUniqueId() + "] drone audio is switch off for "+playerId+" ,Turned on now");
            }

            var playback = (AudioStreamGeneratorPlayback)audioPlayer.GetStreamPlayback();
            if (playback != null)
            {
                var samples = DecompressFloatArray(audioData);
                Vector2[] stereoSamples = MonoToStereoVector2Array(samples);
                playback.PushBuffer(stereoSamples);
            }
        }

    }

    public float[] DecompressFloatArray(byte[] compressedArray)
    {
        using var memoryStream = new MemoryStream(compressedArray);
        using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        gZipStream.CopyTo(resultStream);
        byte[] byteArray = resultStream.ToArray();
        float[] floatArray = new float[byteArray.Length / 4];
        Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
        return floatArray;
    }

    // currently we use mono but 3d requires stereo (but we just faked it)
    // should still work with no problem
    private Vector2[] MonoToStereoVector2Array(float[] monoSamples)
    {
        Vector2[] stereoSamples = new Vector2[monoSamples.Length];
        for (int i = 0; i < monoSamples.Length; i++)
            stereoSamples[i] = new Vector2(monoSamples[i], monoSamples[i]);
        return stereoSamples;
    }
}
