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
        Manager.PositionDynamics.Set(FolderTransform.position);
        Manager.RotationDynamics.Set(FolderTransform.rotation.AsVector());
      }
    }

    protected virtual void Awake() {
      Manager = GetComponent<OverlayManager>();
    }
  }
}
