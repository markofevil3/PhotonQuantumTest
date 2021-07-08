using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantum.Prototypes;

namespace Quantum {
  public interface IPrototypeAdapter<PrototypeType> where PrototypeType : IPrototype { 
    PrototypeType Convert(EntityPrototypeConverter converter);
  }
}