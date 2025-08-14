using Godot;
using System;

public partial class LightsWithFlickerScript : Node3D
{
    [Export] public SpotLight3D SpotLight;
    [Export] public OmniLight3D OmniLight;

    [Export] public float FlickerSpeed = 3.0f; // Lower = slower variation
    [Export] public float MinEnergy = 0.2f;
    [Export] public float MaxEnergy = 1.2f;
    [Export] public bool Randomised = true;

    private float _time;
    private FastNoiseLite _noise;

    public override void _Ready()
    {
        _noise = new FastNoiseLite();
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
    }

    public override void _Process(double delta)
    {
        _time += (float)delta * FlickerSpeed;
        float newEnergy;

        if (Randomised)
        {
            // Jittery flicker
            newEnergy = (float)GD.RandRange(MinEnergy, MaxEnergy);
        }
        else
        {
            // Smooth flicker using noise
            float noiseValue = (_noise.GetNoise1D(_time) + 1f) / 2f; // Convert -1..1 to 0..1
            newEnergy = Mathf.Lerp(MinEnergy, MaxEnergy, noiseValue);
        }

        if (SpotLight != null)
            SpotLight.LightEnergy = newEnergy;

        if (OmniLight != null)
            OmniLight.LightEnergy = newEnergy;
    }
}
