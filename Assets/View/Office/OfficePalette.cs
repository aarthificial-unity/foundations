using UnityEngine;
using UnityEngine.Serialization;

namespace View.Office {
  public class OfficePalette : ScriptableObject {
    public Color Paper;
    public Color PaperSelected;
    public Color Ink;
    public Color InkDisabled;
    [FormerlySerializedAs("Cardboard")] public Color InkHovered;
  }
}
