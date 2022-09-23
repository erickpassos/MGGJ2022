namespace Quantum
{
  public unsafe class BoatSystem : SystemMainThreadFilter<BoatSystem.Filter>, ISignalOnComponentAdded<Boat>, ISignalReset
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
      var waves = f.Assets.WaveSampleBase(f.RuntimeConfig.WaveSample);
      var boatConfig = f.Assets.BoatConfig(filter.Boat->Config); 
      boatConfig.UpdateBoat(f, ref filter, waves);
    }

    public void OnAdded(Frame f, EntityRef entity, Boat* component)
    {
      if (f.Unsafe.TryGetPointer<Transform3D>(entity, out var transform))
      {
        component->Spawn = *transform;
      }
    }

    public void Reset(Frame f, EntityRef entity)
    {
      // only boats are reset from here
      if (f.Unsafe.TryGetPointer<Boat>(entity, out var boat) && f.Unsafe.TryGetPointer<Transform3D>(entity, out var transform))
      {
        *transform = boat->Spawn;
        // also reset barge if necessary
        if (f.Exists(boat->Barge) && f.Has<TimedReset>(boat->Barge) == false)
        {
          f.Signals.Reset(boat->Barge);
        }
      }
    }
  }
}