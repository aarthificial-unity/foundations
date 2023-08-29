using Player.States;
using UnityEngine;
using Utils;

namespace View.Overlay.States {
  public class OverlayState : BaseState {
    public Transform FolderTransform;
    protected OverlayManager Manager;

    public override void OnEnter() {
      base.OnEnter();
      if (FolderTransform != null) {
        Manager.PositionDynamics.Set(FolderTransform.localPosition);
        Manager.RotationDynamics.Set(FolderTransform.localRotation.AsVector());
      }
    }

    protected virtual void Awake() {
      Manager = GetComponent<OverlayManager>();
    }
  }
}
