using Items;
using Player;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;
using View.Dialogue;

namespace View.Overlay {
  public class HUDView : MonoBehaviour {
    [Inject] [SerializeField] private OverlayChannel _overlay;
    [SerializeField] private DialogueButton[] _buttons;
    public PlayerLookup<ItemSlot> ItemSlots;

    private CanvasGroup _canvasGroup;
    private int _currentDialogueButtonIndex;

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

    public DialogueButton BorrowButton() {
      Assert.IsTrue(_currentDialogueButtonIndex < _buttons.Length);
      return _buttons[_currentDialogueButtonIndex++];
    }

    public void ReleaseButton(DialogueButton button) {
      Assert.IsTrue(_currentDialogueButtonIndex > 0);
      _buttons[--_currentDialogueButtonIndex] = button;
    }
  }
}
