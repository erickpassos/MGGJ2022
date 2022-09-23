using Photon.Deterministic;

namespace Quantum
{
  public unsafe class WeaponSystem : SystemMainThreadFilter<WeaponSystem.Filter>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Weapon* Weapon;
      public BoatControl* Control;
      public Transform3D* Transform;
    }

    public override void Update(Frame f, ref Filter filter)
    {
      filter.Weapon->Time -= f.DeltaTime;
      if (filter.Weapon->Time > FP._0) return;
      
      var input = f.GetPlayerInput(filter.Control->Player);
      if (input->Use.WasPressed)
      {
        var bomb = f.Create(filter.Weapon->Bomb);
        if (f.Unsafe.TryGetPointer<Transform3D>(bomb, out var transform))
        {
          transform->Position = filter.Transform->TransformPoint(filter.Weapon->Offset);
          transform->Rotation = filter.Transform->Rotation;
        }

        if (f.Unsafe.TryGetPointer<Bomb>(bomb, out var bombData))
        {
          filter.Weapon->Time = bombData->TTL;
        }
        
        
      }
    }


    public void OnAdded(Frame f, EntityRef entity, TimedReset* component)
    {
      component->TTL = 10;
    }
  }
}