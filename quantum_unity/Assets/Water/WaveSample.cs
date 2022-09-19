using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveSample", menuName = "Wave Simulation/Wave Sample", order = 100)]
public class WaveSample : ScriptableObject
{
  public Senoid[] WaveSources;

  public float GetHeight(Vector2 position, float time)
  {
    // reset on first sample
    var h = 0f;
    for (int s = 0; s < WaveSources.Length; s++)
    {
      var source = WaveSources[s];
      var axis = source.Axis == Axis.X ? position.x : source.Axis == Axis.Z ? position.y : position.y + position.x;
      // accumulate all sources
      float k = 2 * Mathf.PI / source.Wavelength;
      h += source.Amplitude * Mathf.Sin((axis - time * source.Speed) * k);
    }
    return h;
  }

}
public enum Axis
{
  X, XZ, Z
}

[Serializable]
public class Senoid
{
  public float Speed = 1;
  public float Wavelength = 1;
  public float Amplitude = 0.25f;
  public Axis Axis = Axis.X;
}
