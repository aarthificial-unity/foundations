using UnityEngine;
using UnityEngine.AI;

namespace Utils {
  public static class NavMeshExtensions {
    public static Vector3 ToNavMesh(this Vector3 position) {
      if (NavMesh.SamplePosition(
          position,
          out var hit,
          float.MaxValue,
          NavMesh.AllAreas
        )) {
        return hit.position;
      }

      return position;
    }
  }
}
