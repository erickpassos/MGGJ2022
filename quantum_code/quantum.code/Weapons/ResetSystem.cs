using Photon.Deterministic;

namespace Quantum
{
  public unsafe class ResetSystem : SystemMainThreadFilter<ResetSystem.Filter>, ISignalOnComponentAdded<TimedReset>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public TimedReset* Reset;
    }

    public override void Update(Frame f, ref Filter filter)
    {
      filter.Reset->TTL -= f.DeltaTime;
      if (filter.Reset->TTL < FP._0)
      {
        f.Remove<TimedReset>(filter.Entity);
        f.Signals.Reset(filter.Entity);
      }
    }


    public void OnAdded(Frame f, EntityRef entity, TimedReset* component)
    {
      component->TTL = 10;
    }
  }
}