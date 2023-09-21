using Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace View.Controls {
  public class PaperDropdown : TMP_Dropdown {
    [SerializeField] private FMODEventInstance _openSound;
    [SerializeField] private FMODEventInstance _closeSound;
    private PaperStyle _style;

    protected override void Awake() {
      _openSound.Setup();
      _closeSound.Setup();
      _style = GetComponent<PaperStyle>();
      base.Awake();
    }

    protected override void OnDestroy() {
      _openSound.Release();
      _closeSound.Release();
      base.OnDestroy();
    }

    public override void OnPointerClick(PointerEventData eventData) {
      base.OnPointerClick(eventData);
      if (IsInteractable()) {
        _openSound.Play();
      }
    }

    public override void OnSubmit(BaseEventData eventData) {
      base.OnSubmit(eventData);
      if (IsInteractable()) {
        _openSound.Play();
      }
    }

    protected override void DestroyBlocker(GameObject blocker) {
      base.DestroyBlocker(blocker);
      if (IsInteractable()) {
        _closeSound.Play();
      }
    }

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      _style?.DoStateTransition((PaperStyle.SelectionState)state);
    }
  }
}
