using UnityEngine;
using System.Collections;
public class WaveTypes {

	public static float SinXWave( Vector3 Position,
                                float speed,
                                float scale,
                                float waveDistance,
                                float noiseStrength,
                                float noiseWalk,
                                float timeSinceStart)
    {
        float x = Position.x;
        float y = 0f;
        float z = Position.z;
        float WaveType = z;

        y += Mathf.Sin((timeSinceStart * speed + WaveType) / waveDistance) * scale;
        y += Mathf.PerlinNoise(x + noiseWalk, y + Mathf.Sin(timeSinceStart * 0.1f)) * noiseStrength;

        return y;
    }
}
