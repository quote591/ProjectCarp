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


    // forces to be a godot collection instead of a c# array
    private Godot.Collections.Array<float> receiveBuffer = new Godot.Collections.Array<float>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetupAudio(1);      // DEFAULT VALUE 1 FOR DEBUGGING SOLO
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
        AudioStreamPlayback player = GetNode<AudioStreamPlayer2D>(AudioOutputPath).GetStreamPlayback();     // HERE IS WHERE WE GET 2D AUDIO, THIS NEEDS TO BE FIXED TO BE 3D
        playback = player as AudioStreamGeneratorPlayback;

        // ===== OLD CODE START ===== 
        // testing if there is data
        //var player = GetNode<AudioStreamPlayer3D>(AudioOutputPath).GetStreamPlayback();
        //if (player is AudioStreamGeneratorPlayback)
        //{
        //    playback = player as AudioStreamGeneratorPlayback;
        //}
        // playback = (AudioStreamGeneratorPlayback)GetNode<AudioStreamPlayer3D>(AudioOutputPath).GetStreamPlayback();  // which node to play back to    // need for later
        // ===== OLD CODE END ===== 
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

            // if we have data and we has been averaged properly, pass on
            Rpc("sendData", CompressFloatArray(data));                                      // THIS HAS BEEN COMMENTED OUT FOR LOCAL TESTING
            //sendData(data);                                                     // COMMENT THIS OUT WHEN DEPLOYED MULTIPLAYER
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
    [Rpc(mode: MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void sendData(byte[] data)
    {
        // when a person rpcs that data over, but not all the data has arrived yet
        // "chuncks of data"
        // we are gonna have a recieve buffer

        receiveBuffer.AddRange(DecompressFloatArray(data));
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
