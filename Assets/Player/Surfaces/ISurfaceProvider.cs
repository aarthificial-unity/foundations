using UnityEngine;

namespace Player.Surfaces {
  public interface ISurfaceProvider {
    int GetSurface(RaycastHit hit);
  }
}
