using Photon.Deterministic;

namespace Quantum
{
  // public unsafe class BuoySystem : SystemMainThreadFilter<BuoySystem.Filter>
  // {
  //   public struct Filter
  //   {
  //     public EntityRef Entity;
  //     public Transform3D* Transform;
  //     public PhysicsBody3D* Body;
  //     public Buoy* Buoy;
  //   }
  //
  //   public override void Update(Frame f, ref Filter filter)
  //   {
  //     var waves = f.Assets.WaveSample(f.RuntimeConfig.WaveSample);
  //     FPVector3 force = default;
  //     filter.Buoy->SpringDamper.ComputeSpringDamperForce(f, waves, filter.Transform->Position, out force);
  //     filter.Body->AddForce(force);
  //   }
  // }
  //
  public unsafe class MultiBuoySystem : SystemMainThreadFilter<MultiBuoySystem.Filter>, ISignalOnComponentAdded<MultiBuoy>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Transform3D* Transform;
      public PhysicsBody3D* Body;
      public MultiBuoy* Buoys;
    }
    public override void Update(Frame f, ref Filter filter)
    {
      var waves = f.Assets.WaveSampleBase(f.RuntimeConfig.WaveSample);
      
      for (int i = 0; i < 4; i++)
      {
        FPVector3 force = default;
        var buoy = filter.Buoys->Buoys.GetPointer(i);
        var worldPos = filter.Transform->TransformPoint(buoy->LocalOffset);
        buoy->ComputeSpringDamperForce(f, waves, worldPos, out force);
        filter.Body->AddForceAtPosition(force, worldPos, filter.Transform);
      }
    }

    public void OnAdded(Frame f, EntityRef entity, MultiBuoy* component)
    {
      var spring = component->Spring;
      var damper = component->Damper;
      var offset = component->Offset;
      component->Buoys.GetPointer(0)->Init(spring, damper, offset);
      offset.X = -offset.X;
      component->Buoys.GetPointer(1)->Init(spring, damper, offset);
      offset.X = -offset.X;
      offset.Z = -offset.Z;
      component->Buoys.GetPointer(2)->Init(spring, damper, offset);
      offset.X = -offset.X;
      component->Buoys.GetPointer(3)->Init(spring, damper, offset);
    }
 
  }
}