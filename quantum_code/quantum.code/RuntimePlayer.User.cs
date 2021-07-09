using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum {
  partial class RuntimePlayer {

    public AssetRefEntityPrototype CharacterPrototype;
    public string PlayerName;

    partial void SerializeUserData(BitStream stream)
    {
      stream.Serialize(ref CharacterPrototype);
      stream.Serialize(ref PlayerName);
    }
  }
}
