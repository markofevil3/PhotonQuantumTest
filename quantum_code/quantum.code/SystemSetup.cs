using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantum.Utils;

namespace Quantum {
  public static class SystemSetup {
    public static SystemBase[] CreateSystems(RuntimeConfig gameConfig, SimulationConfig simulationConfig) {
      UnitySystemConsoleRedirector.Redirect();
      return new SystemBase[] {
        // pre-defined core systems
        new Core.CullingSystem2D(), 
        new Core.CullingSystem3D(),
        
        new Core.PhysicsSystem2D(),
        new Core.PhysicsSystem3D(),
        
        new Core.NavigationSystem(),
        new Core.EntityPrototypeSystem(),

        // user systems go here
        new MovementSystem(),
      };
    }
  }
}