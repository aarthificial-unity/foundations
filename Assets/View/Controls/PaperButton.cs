using Audio.Events;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace View.Controls {
  public class PaperButton : Selectable, IPointerClickHandler, ISubmitHandler {
    public event Action Clicked;

    [SerializeField] private FMODEventInstance _clickSound;
    private PaperStyle _style;

    protected override void Awake() {
      _clickSound.Setup();
      _style = GetComponent<PaperStyle>();
      base.Awake();
    }

    protected override void OnDestroy() {
      _clickSound.Release();
      base.OnDestroy();
    }

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      _style?.DoStateTransition((PaperStyle.SelectionState)state);
    }

    public void OnPointerClick(PointerEventData eventData) {
      OnSubmit(eventData);
    }

    public void OnSubmit(BaseEventData eventData) {
      if (IsInteractable()) {
        _clickSound.Play();
        Clicked?.Invoke();
      }
    }
  }
}
