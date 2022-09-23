using System.Reflection;
using System.Runtime.CompilerServices;
using Photon.Deterministic;

namespace Quantum
{
  public unsafe class BombSystem : SystemMainThreadFilter<BombSystem.Filter>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public Transform3D* Transform;
      public Bomb* Bomb;
    }

    public override void Update(Frame f, ref Filter filter)
    {
      var waves = f.Assets.WaveSampleBase(f.RuntimeConfig.WaveSample);
      var anchorPoint = filter.Transform->TransformPoint(FPVector3.Down);

      var inWasser = waves.CheckUnderwater(f, anchorPoint, out var height);
      if (inWasser)
      {
        filter.Bomb->TTL -= f.DeltaTime;
        if (filter.Bomb->TTL < FP._0)
        {
          // boom
          Explode(f, ref filter);
        }
      }
    }

    private void Explode(Frame f, ref Filter filter)
    {
      if (f.Exists(filter.Entity) == false) return;
      var shape = Shape3D.CreateSphere(filter.Bomb->Radius);
      var hits = f.Physics3D.OverlapShape(*filter.Transform, shape);
      for (int i = 0; i < hits.Count; i++)
      {
        var hit = hits[i];
        if (hit.IsDynamic == false) continue;

        if (f.Unsafe.TryGetPointer<PhysicsBody3D>(hit.Entity, out var b) && f.Unsafe.TryGetPointer<Transform3D>(hit.Entity, out var t))
        {
          var direction = (t->Position - filter.Transform->Position).Normalized;
          var forceDirection = direction + FPVector3.Up;
          b->AddLinearImpulse(forceDirection.Normalized * filter.Bomb->Power);

          // add a timer for respawn on TARGET
          if (f.Has<TimedReset>(hit.Entity) == false) f.Add(hit.Entity, new TimedReset());
        }
      }

      f.Destroy(filter.Entity);
    }
  }
}