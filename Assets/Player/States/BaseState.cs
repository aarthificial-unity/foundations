using System;
using UnityEngine;

namespace Player.States {
  public abstract class BaseState : MonoBehaviour {
    [NonSerialized] public bool IsActive;

    public virtual void OnUpdate() { }

    public virtual void OnEnter() {
      IsActive = true;
    }

    public virtual void OnExit() {
      IsActive = false;
    }
  }
}
