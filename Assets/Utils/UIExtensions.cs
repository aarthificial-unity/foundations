using UnityEngine.UI;
using View.Controls;

namespace Utils {
  public static class UIExtensions {
    public static void QuietSelect(this Selectable selectable) {
      if (selectable.TryGetComponent(out FocusSound focusSound)) {
        focusSound.QuietSelect();
      } else {
        selectable.Select();
      }
    }
  }
}
