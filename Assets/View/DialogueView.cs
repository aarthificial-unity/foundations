using UnityEngine;
using Utils;
using View.Dialogue;

namespace View {
  public class DialogueView : MonoBehaviour {
    [Inject] [SerializeField] private ViewChannel _view;
    public DialogueTrack Track;
    public DialogueWheel Wheel;

    private CanvasGroup _canvasGroup;

    private void Awake() {
      _canvasGroup = GetComponent<CanvasGroup>();
      _view.Dialogue = this;
      SetActive(false);
    }

    private void OnDestroy() {
      _view.Dialogue = null;
    }

    public void SetActive(bool value) {
      _canvasGroup.interactable = value;
      _canvasGroup.blocksRaycasts = value;
      _canvasGroup.alpha = value ? 1 : 0;
    }
  }
}
