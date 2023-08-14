using UnityEngine;

[ExecuteAlways]
public class PaintProjection : MonoBehaviour {
  private static readonly int _paintMatrix =
    Shader.PropertyToID("_PaintMatrix");

  private void Update() {
    Shader.SetGlobalMatrix(_paintMatrix, transform.worldToLocalMatrix);
  }
}
