using System;
using Photon.Deterministic;

namespace Quantum
{
  [Serializable]
  public class GerstnerWaves : WaveSampleBase
  {
    public FP Gravity = FP._10;
    public Gerstner[] WaveSources;

    public override FP GetHeight(FPVector2 position, FP time)
    {
      FP height = default;
      for (int i = 0; i < WaveSources.Length; i++)
      {
        var source = WaveSources[i];
        height += source.Height(position, time);
      }
      return height;
    }

    public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
    {
      for (int i = 0; i < WaveSources.Length; i++)
      {
        var source = WaveSources[i];
        source.Init(Gravity);
      }
    }
  }
  
  [Serializable]
  public class Gerstner
  {
    public FP Wavelength = 10;
    public FP Steepness = FP._0_25;
    public FPVector2 Direction = FPVector2.Right;

    private FP _k;
    private FP _c;
    private FP _a;
    private FPVector2 _direction;

    public void Init(FP gravity)
    {
      _k = FP.PiTimes2 / Wavelength;
      _c = FPMath.Sqrt(gravity / _k);
      _a = Steepness / _k;
      _direction = Direction.Normalized;
    }
  
    public FP Height(FPVector2 position, FP time)
    {
      FP f = _k * (FPVector2.Dot(_direction, position) - _c * time);

      // local space
      // p.x = d.x * (a * cos(f));
      // p.y = a * sin(f);
      // p.z = d.y * (a * cos(f));
      
      // simplified (enough for the physics smoke-and-mirrors
      return _a * FPMath.Sin(f);
    }

  }
  
}
