using Photon.Deterministic;

namespace Quantum
{
  unsafe partial struct SpringDamper
  {
    public bool ComputeSpringDamperForce(Frame frame, WaveSample waves, FPVector3 position, out FPVector3 force)
    {
      var desiredHeight = waves.GetHeight(new FPVector2(position.X, position.Z), frame.DeltaTime * frame.Number);
      var strength = FPMath.Clamp01(desiredHeight - position.Y) * Spring;

      FP damping = 0;
      if (_previousStrength != 0)
      {
        damping = strength - _previousStrength;
        damping = damping * frame.SessionConfig.UpdateFPS * Damper; // NM
      }

      var forceMagnitude = FPMath.Clamp(strength + damping, FP._0, FP.UseableMax);
      force = new FPVector3(0, forceMagnitude, 0);

      _previousStrength = strength;
#if DEBUG
      Draw.Sphere(position, FP._0_10, ColorRGBA.Red);
      position.Y = desiredHeight;
      Draw.Sphere(position, FP._0_10, ColorRGBA.Gray);
#endif
      if (strength > 0) return true;
      return false;
    }

    public void Init(FP spring, FP damper, FPVector3 offset)
    {
      Spring = spring;
      Damper = damper;
      LocalOffset = offset;
    }
  }
}