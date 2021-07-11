using System;
using Photon.Deterministic;

namespace Quantum {
  public class ChooseMapNodeCommand : DeterministicCommand {
    // Index of Next Node in MapNode.NextNodes
    public Int32 NextNodeIndex;
    
    public override void Serialize(BitStream stream) {
      stream.Serialize(ref NextNodeIndex);
    }
  }
}