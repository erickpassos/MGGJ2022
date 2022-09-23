using Photon.Deterministic;

namespace Quantum
{
  public unsafe class SupplySystem : SystemMainThreadFilter<SupplySystem.Filter>, ISignalReset, ISignalOnTriggerEnter3D
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Supply* Supply;
      public Transform3D* Transform;
    }

    public override void Update(Frame f, ref Filter filter)
    {
      var waves = f.Assets.WaveSampleBase(f.RuntimeConfig.WaveSample);
      var anchor = filter.Transform->Position + FPVector3.Down * filter.Supply->RangeCheck;
      var unterWasser = waves.CheckUnderwater(f, anchor, out var height);
      if (unterWasser)
      {
        if (f.Has<TimedReset>(filter.Entity) == false)
        {
          f.Add(filter.Entity, new TimedReset());
        }
      }
      else
      {
        if (f.Has<TimedReset>(filter.Entity))
        {
          f.Remove<TimedReset>(filter.Entity);
        }
      }
    }
    
    public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
    {
      if (info.IsStatic == false) return;

      // supply is delivered where there is no asset
      if (info.StaticData.Asset == default && f.Unsafe.TryGetPointer<Supply>(info.Entity, out var supply))
      {
        f.Signals.ScoreReconnect();
        f.Destroy(info.Entity);
        if (f.Unsafe.TryGetPointer<Barge>(supply->Barge, out var barge))
        {
          f.Events.DeliveredSupply(barge->Player);
        }
      }
      
    }

    public void Reset(Frame f, EntityRef entity)
    {
      // in case of supply, reset means destroy
      if (f.Unsafe.TryGetPointer<Supply>(entity, out var supply))
      {
        f.Signals.ScoreDisconnect();
        f.Destroy(entity);
      }
    }
  }
}