using UnityEngine;

namespace Utils.Tweening {
  [CreateAssetMenu(
    fileName = "ToggleSpring",
    menuName = "Toggle Spring Preset",
    order = 0
  )]
  public class ToggleSpringPreset : ScriptableObject {
    public SpringConfig On;
    public SpringConfig Off;
    public bool CustomOff;

    public ToggleSpring Create() {
      return new ToggleSpring(this);
    }
  }
}
