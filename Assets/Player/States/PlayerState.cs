using System;

namespace Player.States {
  public class PlayerState : BaseState {
    protected PlayerController Player;
    protected PlayerController Other => Player.Other;

    protected virtual void Awake() {
      Player = GetComponent<PlayerController>();
    }
  }
}
