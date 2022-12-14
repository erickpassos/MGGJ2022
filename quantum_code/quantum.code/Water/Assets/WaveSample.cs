using System;
using Photon.Deterministic;
namespace Quantum
{
  public abstract unsafe partial class WaveSampleBase : AssetObject
  {
    public abstract FP GetHeight(FPVector2 position, FP time);

    public bool CheckUnderwater(Frame f, FPVector3 position, out FP height)
    {
      height = GetHeight(position.XZ, f.Number * f.DeltaTime);
      return position.Y <= height;
    }
  }


  [Serializable]
  public class WaveSampleSine : WaveSampleBase
  {
    public Senoid[] WaveSources;

    public override FP GetHeight(FPVector2 position, FP time)
    {
      // reset on first sample
      var h = FP._0;
      for (int s = 0; s < WaveSources.Length; s++)
      {
        var source = WaveSources[s];
        FP axis = default;
        switch (source.Axis)
        {
          case Axis.X:
            axis = position.X;
            break;
          case Axis.Z:
            axis = position.Y;
            break;
          case Axis.XZ:
            axis = position.X + position.Y;
            break;
        }
        FP k = (2 * FP.Pi) / source.Wavelength;
        // accumulate all sources
        h += source.Amplitude * FPMath.Sin((axis - time * source.Speed) * k);
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
    public FP Speed = 1;
    public FP Wavelength = 10;
    public FP Amplitude = FP._0_25;
    public Axis Axis = Axis.X;
  }
}