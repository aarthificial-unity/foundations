using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View {
  public class ButtonNavigation : MonoBehaviour,
    IPointerMoveHandler,
    ISelectHandler {
    public Button Button;
    [SerializeField] private StudioEventEmitter _focusSound;
    [SerializeField] private StudioEventEmitter _clickSound;

    private bool _ignoreSound;

    private void Awake() {
      Button.onClick.AddListener(_clickSound.Play);
    }

    public void QuickSelect() {
      GameObject current = null;
      if (EventSystem.current != null) {
        current = EventSystem.current.currentSelectedGameObject;
      }

      if (current == gameObject) {
        return;
      }

      var currentColors = new ColorBlock();
      Button currentButton = null;
      if (current != null && current.TryGetComponent(out currentButton)) {
        currentColors = currentButton.colors;
        var currentQuick = currentColors;
        currentQuick.fadeDuration = 0;
        currentButton.colors = currentQuick;
      }

      var colors = Button.colors;
      var quick = colors;
      quick.fadeDuration = 0;
      Button.colors = quick;
      _ignoreSound = true;
      Button.Select();
      _ignoreSound = false;
      Button.colors = colors;

      if (currentButton != null) {
        currentButton.colors = currentColors;
      }
    }

    public void OnPointerMove(PointerEventData _) {
      if (enabled) {
        Button.Select();
      }
    }

    public void OnSelect(BaseEventData _) {
      if (enabled && !_ignoreSound) {
        _focusSound.Play();
      }
    }
  }
}
