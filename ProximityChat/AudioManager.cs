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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetupAudio(Multiplayer.GetUniqueId());      // DEFAULT VALUE 1 FOR DEBUGGING SOLO
        GD.Print("{" + Multiplayer.GetUniqueId() + "} Has set up Audio");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (IsMultiplayerAuthority())
        {
            processMic();
        }
        processVoice();
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
            Rpc("sendData", CompressFloatArray(data));      // original compression
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

        receiveBuffer.AddRange(DecompressFloatArray(data));     // base compression
        //receiveBuffer.AddRange(DecompressFloatArrayLossy(data));   // custom compression
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
}