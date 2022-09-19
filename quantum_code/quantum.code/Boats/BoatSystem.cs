namespace Quantum
{
  public unsafe class BoatSystem : SystemMainThreadFilter<BoatSystem.Filter>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Transform3D* Transform;
      public PhysicsBody3D* Body;
      public Boat* Boat;
    }
    
    public override void Update(Frame f, ref Filter filter)
    {
      var boatConfig = f.Assets.BoatConfig(filter.Boat->Config); 
      boatConfig.UpdateBoat(f, ref filter);
    }
  }
}