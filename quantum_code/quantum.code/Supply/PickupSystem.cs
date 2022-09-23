using Photon.Deterministic;

namespace Quantum
{
  public unsafe class PickupSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D, ISignalOnTrigger3D
  {
    public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
    {
      if (info.IsStatic == false) return;
      
      Log.Info("pickup" + info.Entity);

      if (info.StaticData.Asset != default && f.Unsafe.TryGetPointer<Barge>(info.Entity, out var barge))
      {
        if (f.Exists(barge->Supply) == false)
        {
          var prototype = f.FindAsset<EntityPrototype>(info.StaticData.Asset);
          if (prototype != null)
          {
            var supplyEntity = f.Create(prototype);
            barge->Supply = supplyEntity;
            if (f.Unsafe.TryGetPointer<Transform3D>(supplyEntity, out var transform))
            {
              var bargeTransform = f.Get<Transform3D>(info.Entity);
              *transform = bargeTransform;
              transform->Position += transform->Up;
            }
            if (f.Unsafe.TryGetPointer<Supply>(supplyEntity, out var supply))
            {
              supply->Barge = info.Entity;
            }
            f.Events.PickedSupply(barge->Player);
          }
        }
      }
    }

    public void OnTrigger3D(Frame f, TriggerInfo3D info)
    {
      OnTriggerEnter3D(f, info);
    }
  }
}