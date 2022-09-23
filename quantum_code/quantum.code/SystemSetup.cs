
namespace Quantum {
  public static class SystemSetup {
    public static SystemBase[] CreateSystems(RuntimeConfig gameConfig, SimulationConfig simulationConfig) {
      return new SystemBase[] {
        // pre-defined core systems
        new Core.CullingSystem3D(),
        
        new MultiBuoySystem(),
        new BoatSystem(),
        
        new Core.PhysicsSystem3D(),

        Core.DebugCommand.CreateSystem(),
        
        new Core.EntityPrototypeSystem(),
        new Core.PlayerConnectedSystem(),

        // user systems go here 
        new BombSystem(),
        new ResetSystem(),
        new SupplySystem(),
        new PickupSystem(),
        new WeaponSystem(),
      };
    }
  }
}
