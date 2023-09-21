using Items;
using Player;
using UnityEngine;
using Utils;

namespace View.Overlay {
  public class HUDView : MonoBehaviour {
    [Inject] [SerializeField] private OverlayChannel _overlay;
    public PlayerLookup<ItemSlot> ItemSlots;

    private CanvasGroup _canvasGroup;

    private void Awake() {
      _overlay.HUD = this;
      _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnDestroy() {
      _overlay.HUD = null;
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
