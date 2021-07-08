using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapDataBakerCallback {
  public abstract void OnBake(MapData data);
  public abstract void OnBeforeBake(MapData data);
  public virtual void OnBakeNavMesh(MapData data) { }
  public virtual void OnBeforeBakeNavMesh(MapData data) { }
}
