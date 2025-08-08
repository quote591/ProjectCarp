/// <summary>
/// 
/// Welcome to Audio Manager
/// 
/// currently we have two modes
/// - Voice Chat    (everyone hears everyone all the time)
/// - Proxy Chat    (drones)
/// 
/// currently we have made proxy chat the default option for playerss
/// 
/// </summary>

using Godot;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Encodings.Web;

public partial class AudioManager : Node
{
    private AudioStreamPlayer input; // when we are talking and inputting into
    private int index; // bus index 
    private AudioEffectCapture effect; // capture of your voice (housed by input)
    private AudioStreamGeneratorPlayback playback; // playback

    [Export]
    public NodePath AudioOutputPath { get; set; }

    [Export]
    public float InputThreashold = 0.005f;

    private bool receivingData = false;
    private int expectedPacketLength = -1; // or some protocol to know full size

    // forces to be a godot collection instead of a c# array
    private Godot.Collections.Array<float> receiveBuffer = new Godot.Collections.Array<float>();


    //private Node droneManager;
    private Node droneManager = null;
    private int playerId;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetupAudio(Multiplayer.GetUniqueId());
        GD.Print("{" + Multiplayer.GetUniqueId() + "} Has set up Audio");
        SetProcess(true);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // this fixes issue with game crashing after getting force disconnected from host
        if (GetMultiplayer().GetMultiplayerPeer() == null)
        {
            GD.Print("GetMultiplayer().GetMultiplayerPeer() == null");
            return;
        }

        // find the drone manager
        // root
        // - scene
        // - - PlayerSpawnPoints
        // - - VoiceDroneController
        if (droneManager == null)
        {
            Node current = this;
            while (current != null)
            {
                // Try to find "VoiceDroneController" directly under this node
                droneManager = current.GetNodeOrNull<Node>("VoiceDroneController");
                if (droneManager != null)
                    break;
                current = current.GetParent();
            }
        }
        else
        {
            try
            {
                if (IsMultiplayerAuthority())
                {
                    processMic();
                }
                processVoice();
            }
            catch (Exception e)
            {
                GD.Print("Caught exception: " + e.Message);
            }
        }

        // use this commented out code if you dont want drones (proxy chat)
        // if (IsMultiplayerAuthority())
        // {
        //     processMic();
        // }
        // processVoice();

    }

    public void SetupAudio(long id)
    {
        input = GetNode<AudioStreamPlayer>("Input"); // pulling back our input (microphone input) "hold microphone input"

        SetMultiplayerAuthority(Convert.ToInt32(id));
        // check if player is allowed to send data
        if (IsMultiplayerAuthority())
        {
            input.Stream = new AudioStreamMicrophone(); // creating a new stream "creating microphone"
            input.Play(); // starting capture
            index = AudioServer.GetBusIndex("Record"); // setting which bus 
            effect = (AudioEffectCapture)AudioServer.GetBusEffect(index, 0); // pulling back the effect to where we are gonna load that data
        }

        // Get the output player node
        var outputPlayer = GetNode<AudioStreamPlayer>(AudioOutputPath);

        // Set up the generator stream
        outputPlayer.Stream = new AudioStreamGenerator(); // Required before calling Play
        outputPlayer.Play(); // Must play before calling GetStreamPlayback

        // Get the playback stream
        var player = outputPlayer.GetStreamPlayback();
        playback = player as AudioStreamGeneratorPlayback;

        // Optional safety check
        if (playback == null)
        {
            GD.PrintErr("Failed to get AudioStreamGeneratorPlayback. Check if stream was set correctly.");
        }
    }

    private void processMic()
    {
        // grabbing data from the audio buffer
        // we should define it, but instead we are gonna get the entire thing
        var sterioData = effect.GetBuffer(effect.GetFramesAvailable());

        // if there is any data (aka mic is on)
        if (sterioData.Length > 0)
        {
            var data = new float[sterioData.Length];

            float maxAmplitude = 0.0f;
            for (int i = 0; i < sterioData.Length; i++)
            {
                var value = (sterioData[i].X + sterioData[i].Y) / 2;
                maxAmplitude = Math.Max(value, maxAmplitude); // if has data, will pass over
                data[i] = value;
            }

            // what if a person is not speaking at all, then dont send anything
            if (maxAmplitude < InputThreashold)
            {
                return;
            }

            // if you want non 3D audio, uncomment below
            //Rpc("sendData", CompressFloatArray(data)); // non 3d audio

            // if drone manager exists then send the player voice to it
            try
            {
                if (droneManager != null)
                {
                    // Call the RPC on droneManager
                    //GD.Print("[" + Multiplayer.GetUniqueId() + "] im sending RecieveMicDataFromPlayer "+Multiplayer.GetUniqueId() );
                    droneManager.Rpc("ReceiveMicDataFromPlayer", Multiplayer.GetUniqueId(), CompressFloatArray(data));
                }
                else
                {
                    GD.PrintErr("DroneManager node not found!");
                }
            }
            catch (Exception e)
            {
                GD.PushError($"Error sending voice data: {e.Message}");
            }
        }
    }

    // this is only used for general voice chat (non 3D)
    private void processVoice()
    {
        if (receiveBuffer.Count <= 0) return;

        // this checks if it still has audio to play before playing recieving audio
        for (int i = 0; i < Math.Min(playback.GetFramesAvailable(), receiveBuffer.Count); i++)
        {
            playback.PushFrame(new Vector2(receiveBuffer[0], receiveBuffer[0]));
            receiveBuffer.RemoveAt(0); // once processed, then remove
        }
    }

    // any player in the server can call this
    // call local false so we dont hear ourselves
    // unrealiable allows to send the data no matter what (it wont wait for recieve)
    // "throw all the words out and see if the person catches it"
    // [08/082025] this is now only used for voice chat (non 3D)
    [Rpc(mode: MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void sendData(byte[] data)
    {
        try
        {
            var floats = DecompressFloatArray(data);
            receiveBuffer.AddRange(floats);
        }
        catch (Exception e)
        {
            GD.PushError($"Error receiving voice data: {e.Message}");
        }
    }

    public byte[] CompressFloatArray(float[] floatArray)
    {
        byte[] byteArray = new byte[floatArray.Length * 4];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);

        using (var memoryStream = new MemoryStream())
        {
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                gZipStream.Write(byteArray, 0, byteArray.Length);
            }
            return memoryStream.ToArray();
        }
    }

    public float[] DecompressFloatArray(byte[] compressedArray)
    {
        using (var memoryStream = new MemoryStream(compressedArray))
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            gZipStream.CopyTo(resultStream);
            byte[] byteArray = resultStream.ToArray();
            float[] floatArray = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            return floatArray;
        }
    }

    // drone manager 3d audio
    // proxy chat
    [Rpc(mode: MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendMicDataToManager(byte[] audioData)
    {
        int id = Multiplayer.GetUniqueId();
        if (id <= 0)
        {
            GD.PrintErr("Trying to send mic data with invalid player ID " + id);
            return;
        }

        var droneManager = GetTree().Root.GetNode<VoiceDroneController>("root/VoiceDroneController");
        droneManager.RpcId(1, "ReceiveMicDataFromPlayer", id, audioData);
    }


    Node FindNodeByName(Node root, string name)
    {
        if (root.Name == name)
            return root;

        foreach (Node child in root.GetChildren())
        {
            Node found = FindNodeByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}