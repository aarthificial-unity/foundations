using System;
using Player.States;

namespace Player.ManagerStates {
  [Serializable]
  public class ManagerState : BaseState {
    protected PlayerManager Manager;

    protected virtual void Awake() {
      Manager = GetComponent<PlayerManager>();
    }
  }
}
