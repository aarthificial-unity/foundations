using System;
using Player.States;

namespace Player.ManagerStates {
  [Serializable]
  public class ManagerState : BaseState {
    protected PlayerManager Manager;

    private void Awake() {
      Manager = GetComponent<PlayerManager>();
    }
  }
}
