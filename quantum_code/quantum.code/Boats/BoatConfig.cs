using Photon.Deterministic;

namespace Quantum
{
  public unsafe partial class BoatConfig : AssetObject
  {
    public FP Accel = 10;
    public FP RotationSpeed = 1;
    public FP KeelArea = 10;
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
      var rudderPosition = filter.Transform->TransformPoint(RudderOffset);
      var frontPosition = filter.Transform->TransformPoint(-RudderOffset);
      // two points for keel
      var keelForce = UpdateWaterfoil(f, ref filter, RudderOffset, filter.Transform->Right) * KeelArea * FP._0_50;
      filter.Body->AddForceAtPosition(keelForce, rudderPosition, filter.Transform);
      keelForce = UpdateWaterfoil(f, ref filter, -RudderOffset, filter.Transform->Right) * KeelArea * FP._0_50;
      filter.Body->AddForceAtPosition(keelForce, frontPosition, filter.Transform);

      // rudder and engine only if controlled by player
      if (filter.Boat->Player == default) return;
      
      var input = f.GetPlayerInput(filter.Boat->Player);
      var forward = filter.Transform->Forward;

      filter.Boat->CurrentRudderAngle = FP._0;
      if (input->Left.IsDown)
      {
        filter.Boat->CurrentRudderAngle = MaxRudderAngle;
      }
      else if (input->Right.IsDown)
      {
        filter.Boat->CurrentRudderAngle = -MaxRudderAngle;
      }

      // rudder physics
      var localRudderRotation = FPQuaternion.Euler(0, filter.Boat->CurrentRudderAngle, 0);
      var localRudderRight = localRudderRotation * FPVector3.Right;
      var rudderRight = filter.Transform->TransformDirection(localRudderRight);
      var rudderForce = UpdateWaterfoil(f, ref filter, localRudderRight, rudderRight) * RudderArea;
     

      // both rudder force and engine force depend on it being under water
      var rudderInWater = waves.CheckUnderwater(f, rudderPosition, out var heightAtRudder);
      if (rudderInWater == false) return;
      
      filter.Body->AddForceAtPosition(rudderForce, rudderPosition, filter.Transform);
      
      if (input->Forward.IsDown)
      {
        filter.Body->AddForce(forward * Accel);
      }
      if (input->Backward.IsDown)
      {
        filter.Body->AddForce(-forward * Accel);
      }
      
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