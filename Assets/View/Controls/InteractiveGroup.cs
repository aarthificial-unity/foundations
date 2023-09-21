using UnityEngine;

namespace View.Controls {
  public class InteractiveGroup : MonoBehaviour {
    private Canvas _canvas;
    private CanvasGroup _group;

    public void DrivenAwake(Camera worldCamera) {
      _canvas = GetComponent<Canvas>();
      _group = GetComponent<CanvasGroup>();
      _canvas.worldCamera = worldCamera;
    }

    public void SetInteractive(bool value) {
      _group.interactable = value;
      _group.blocksRaycasts = value;
    }
  }
}
