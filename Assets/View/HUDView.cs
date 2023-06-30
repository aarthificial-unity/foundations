using UnityEngine;
using Utils;

namespace View {
  public class HUDView : MonoBehaviour {
    [Inject] [SerializeField] private ViewChannel _view;
    private CanvasGroup _canvasGroup;

    private void Awake() {
      _view.HUD = this;
      _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnDestroy() {
      _view.HUD = null;
    }

    public void SetActive(bool value) {
      _canvasGroup.interactable = value;
      _canvasGroup.alpha = value ? 1 : 0;
    }

    public void SetInteractive(bool value) {
      _canvasGroup.blocksRaycasts = value;
    }
  }
}
