using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View {
  public class ButtonNavigation : MonoBehaviour, IPointerMoveHandler {
    private Selectable _button;

    private void Start() {
      _button = GetComponent<Selectable>();
    }

    public void OnPointerMove(PointerEventData _) {
      if (enabled) {
        _button.Select();
      }
    }
  }
}
