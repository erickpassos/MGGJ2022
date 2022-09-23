using Photon.Deterministic;

namespace Quantum
{
  public unsafe partial class BoatConfig : AssetObject
  {
    public FP Accel = 10;
    public FP AutoAccel = 0;
    public FP KeelArea = 10;
    public FPVector3 KeelOffset = FPVector3.Forward;
    public FPVector3 RudderOffset = FPVector3.Back;
    public FP RudderArea = 1;
    public FP MaxRudderAngle = 30;

    // TODO
    // check underwater engine
    // add keel DONE
    // add steering DONE
    // (rudder?)
    public void UpdateBoat(Frame f, ref BoatSystem.Filter filter, WaveSampleBase waves)
    {
      var backPosition = filter.Transform->TransformPoint(-KeelOffset);
      var frontPosition = filter.Transform->TransformPoint(KeelOffset);
      // two points for keel
      var keelForce = UpdateWaterfoil(f, ref filter, -KeelOffset, filter.Transform->Right) * KeelArea * FP._0_50;
      filter.Body->AddForceAtPosition(keelForce, backPosition, filter.Transform);
      keelForce = UpdateWaterfoil(f, ref filter, KeelOffset, filter.Transform->Right) * KeelArea * FP._0_50;
      filter.Body->AddForceAtPosition(keelForce, frontPosition, filter.Transform);

      
      Input input = default;
      // rudder and engine only if controlled by player (in case not waiting for respawn
      if (f.Has<TimedReset>(filter.Entity) == false && f.TryGet<BoatControl>(filter.Entity, out var control))
      {
        input = *f.GetPlayerInput(control.Player);
      }

      var forward = filter.Transform->Forward;

      filter.Boat->CurrentRudderAngle = FP._0;
      if (input.Left.IsDown)
      {
        filter.Boat->CurrentRudderAngle = MaxRudderAngle;
      }
      else if (input.Right.IsDown)
      {
        filter.Boat->CurrentRudderAngle = -MaxRudderAngle;
      }

      // rudder physics
      var rudderPosition = filter.Transform->TransformPoint(RudderOffset);

      // both rudder force and engine force depend on it being under water
      var rudderInWater = waves.CheckUnderwater(f, rudderPosition, out var heightAtRudder);
      if (rudderInWater == false) return;
      
      var localRudderRotation = FPQuaternion.Euler(0, filter.Boat->CurrentRudderAngle, 0);
      var localRudderRight = localRudderRotation * FPVector3.Right;
      var rudderRight = filter.Transform->TransformDirection(localRudderRight);
      var rudderForce = UpdateWaterfoil(f, ref filter, localRudderRight, rudderRight) * RudderArea;
      
      filter.Body->AddForceAtPosition(rudderForce, rudderPosition, filter.Transform);
      
      if (input.Forward.IsDown)
      {
        filter.Body->AddForce(forward * Accel);
      }
      if (input.Backward.IsDown)
      {
        filter.Body->AddForce(-forward * Accel);
      }
      filter.Body->AddForce(forward * AutoAccel);
      
#if DEBUG
      Draw.Sphere(rudderPosition, FP._0_10, ColorRGBA.Black);
      Draw.Line(rudderPosition, rudderPosition + rudderForce, ColorRGBA.Red);
#endif
    }

    public FPVector3 UpdateWaterfoil(Frame f, ref BoatSystem.Filter filter, FPVector3 offset, FPVector3 sideways)
    {
      var velocity = filter.Body->GetRelativePointVelocity(offset, filter.Transform);
      var sidewaysVelocity = FPVector3.Project(velocity, sideways);
      return -sidewaysVelocity;
      
    }
  }
}