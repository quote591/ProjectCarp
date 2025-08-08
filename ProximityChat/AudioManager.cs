using Godot;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Encodings.Web;

public partial class AudioManager : Node
{
    // global 
    private AudioStreamPlayer input; // when we are talking and inputting into
    private int index; // bus index 
    private AudioEffectCapture effect; // capture of your voice (housed by input)
    private AudioStreamGeneratorPlayback playback; // playback

    [Export]
    public NodePath AudioOutputPath { get; set; }

    [Export]
    public float InputThreashold = 0.005f;


    // testing
    private const int MaxPacketSize = 1200; // testing
    private System.Collections.Generic.List<byte> incomingCompressedData = new();
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

        //GD.Print("AudioManager _Process running");

        // Node current = this;
        // while (current != null)
        // {
        //     GD.Print("Parent node: " + current.Name);
        //     current = current.GetParent();
        // }

        if (droneManager == null)
        {
            // Climb up to the TestMultiplayerScene node (parent of parent)
            var testMultiplayerScene = GetParent()?.GetParent();
            if (testMultiplayerScene != null)
            {
                droneManager = testMultiplayerScene.GetNodeOrNull<Node>("VoiceDroneController");
                if (droneManager != null)
                {
                    GD.Print("DroneManager found!");
                }
                else
                {
                    GD.Print("DroneManager NOT found under TestMultiplayerScene");
                }
            }
            else
            {
                GD.Print("TestMultiplayerScene node not found");
            }
        }
        else
        {
            if (IsMultiplayerAuthority())
            {
                processMic();
            }
            processVoice();
        }


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

        //input.Stream = new AudioStreamMicrophone(); // creating a new stream "creating microphone"
        //input.Play(); // starting capture
        //index = AudioServer.GetBusIndex("Record"); // setting which bus 
        //effect = (AudioEffectCapture)AudioServer.GetBusEffect(index, 0); // pulling back the effect to where we are gonna load that data

        // TODO MAKE AUDIO 3D
        // good luck x
        //AudioStreamPlayback player = GetNode<AudioStreamPlayer>(AudioOutputPath).GetStreamPlayback();     // HERE IS WHERE WE GET 2D AUDIO, THIS NEEDS TO BE FIXED TO BE 3D
        //playback = player as AudioStreamGeneratorPlayback;

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
            //GD.Print("{" + Multiplayer.GetUniqueId() + "} Audio loud enough");

            // if we have data and we has been averaged properly, pass on
            //Rpc("sendData", CompressFloatArray(data));      // original compression
            //Rpc("sendData", CompressFloatArray(data)); // non 3d audio
            try
            {
                if (droneManager != null)
                {
                    // Call the RPC on droneManager
                    GD.Print("[" + Multiplayer.GetUniqueId() + "] im sending RecieveMicDataFromPlayer "+Multiplayer.GetUniqueId() );
                    droneManager.Rpc("ReceiveMicDataFromPlayer", Multiplayer.GetUniqueId() , CompressFloatArray(data));
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
    [Rpc(mode: MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void sendData(byte[] data)
    {
        //GD.Print("Received voice data from another player!");

        // when a person rpcs that data over, but not all the data has arrived yet
        // "chuncks of data"
        // we are gonna have a recieve buffer

        //receiveBuffer.AddRange(DecompressFloatArray(data));     // base compression

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
        //var id = GetMultiplayer().GetUniqueId();
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