using UnityEngine;

namespace Player.Surfaces {
  public class TerrainSurface : MonoBehaviour, ISurfaceProvider {
    private const int _grassId = 1;
    private const int _dirtId = 2;

    private MeshFilter _mesh;
    private int[] _triangles;
    private Color[] _colors;

    private void Awake() {
      _mesh = GetComponent<MeshFilter>();
      var mesh = _mesh.sharedMesh;
      _triangles = mesh.triangles;
      _colors = mesh.colors;
    }

    public int GetSurface(RaycastHit hit) {
      var v0 = _triangles[hit.triangleIndex * 3 + 0];
      var v1 = _triangles[hit.triangleIndex * 3 + 1];
      var v2 = _triangles[hit.triangleIndex * 3 + 2];
      var bary = hit.barycentricCoordinate;
      var color = _colors[v0] * bary.x
        + _colors[v1] * bary.y
        + _colors[v2] * bary.z;

      if (color.r > color.g && color.r > color.b) {
        return _grassId;
      }

      if (color.g > color.r && color.g > color.b) {
        return _dirtId;
      }

      return 0;
    }
  }
}
