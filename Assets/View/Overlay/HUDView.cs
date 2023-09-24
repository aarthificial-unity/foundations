using Items;
using Player;
using UnityEngine;

namespace View.Overlay {
  public class HUDView : MonoBehaviour {
    public PlayerLookup<ItemSlot> ItemSlots;

    private CanvasGroup _canvasGroup;

    private void Awake() {
      _canvasGroup = GetComponent<CanvasGroup>();
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
