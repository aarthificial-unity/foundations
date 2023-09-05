using Interactions;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using View.Overlay;

namespace View.Dialogue {
  [DefaultExecutionOrder(300)]
  public class DialogueButton : Selectable, IPointerClickHandler {
    public enum ActionType {
      None,
      Play,
      Skip,
      Cancel,
    }

    public event Action Clicked;

    public Vector3 ButtonPosition =>
      _overlay.CameraManager.MainCamera.ScreenToWorldPoint(
        transform.position + Vector3.forward
      );

    [Inject] [SerializeField] private OverlayChannel _overlay;
    private InteractionGizmo _gizmo;
    private bool _hasGizmo;
    private ActionType _actionType;

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      UpdateGizmo();
    }

    public void SetAction(ActionType actionType) {
      _actionType = actionType;
      UpdateGizmo();
    }

    public void SetGizmo(InteractionGizmo gizmo) {
      if (_gizmo != null) {
        _gizmo.MoveTo(_gizmo.DefaultPosition);
      }
      _gizmo = gizmo;
      if (gizmo != null) {
        _hasGizmo = true;
        gizmo.MoveTo(ButtonPosition);
        UpdateGizmo();
      } else {
        _hasGizmo = false;
      }
    }

    private void LateUpdate() {
      if (_hasGizmo) {
        _gizmo.DesiredPosition = ButtonPosition;
      }
    }

    private void UpdateGizmo() {
      if (!_hasGizmo) {
        return;
      }

      _gizmo.Icon = _actionType switch {
        ActionType.Play => InteractionGizmo.PlayIcon,
        ActionType.Skip => InteractionGizmo.SkipIcon,
        ActionType.Cancel => InteractionGizmo.CancelIcon,
        _ => InteractionGizmo.DialogueIcon,
      };

      _gizmo.IsExpanded = true;
      _gizmo.IsFocused = true;
      _gizmo.IsHovered = currentSelectionState == SelectionState.Highlighted;
      _gizmo.IsDisabled = currentSelectionState == SelectionState.Disabled;
    }

    public void OnPointerClick(PointerEventData eventData) {
      Clicked?.Invoke();
    }
  }
}
