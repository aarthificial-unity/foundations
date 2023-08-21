using UnityEngine;
using Utils;
using View.Overlay;

namespace View.Dialogue {
  public class DialogueView : MonoBehaviour {
    [Inject] [SerializeField] private OverlayChannel _overlay;
    public DialogueTrack Track;
    public DialogueWheel Wheel;

    private CanvasGroup _canvasGroup;

    private void Awake() {
      _canvasGroup = GetComponent<CanvasGroup>();
      _overlay.Dialogue = this;
      SetActive(false);
    }

    private void OnDestroy() {
      _overlay.Dialogue = null;
    }

    public void SetActive(bool value) {
      _canvasGroup.interactable = value;
      _canvasGroup.blocksRaycasts = value;
      _canvasGroup.alpha = value ? 1 : 0;
    }
  }
}
