using Cinemachine;
using Player.States;
using UnityEngine;

namespace View.Office.States {
  public class MenuState : BaseState {
    [SerializeField] protected CinemachineVirtualCamera Camera;
    protected MenuManager Manager;

    protected virtual void Awake() {
      Manager = GetComponent<MenuManager>();
    }

    public override void OnEnter() {
      base.OnEnter();
      Camera.Priority = 100;
    }

    public override void OnExit() {
      base.OnExit();
      Camera.Priority = 0;
    }
  }
}
