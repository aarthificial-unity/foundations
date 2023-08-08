using UnityEngine;

namespace Player.Surfaces {
  public class PaletteSurface : MonoBehaviour, ISurfaceProvider {
    private const int _v = -11;
    [SerializeField] private Texture2D _palette;

    public int GetSurface(RaycastHit hit) {
      var u = (int)(hit.textureCoord.x * 32);
      var pixel = (Color32)_palette.GetPixel(u, _v);
      return pixel.r;
    }
  }
}
