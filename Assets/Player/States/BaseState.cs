using System;
using UnityEngine;

namespace Player.States {
  public abstract class BaseState : MonoBehaviour {
    public event Action Entered;
    public event Action Exited;

    [NonSerialized] public bool IsActive;

    public virtual void OnUpdate() { }

    public virtual void OnEnter() {
      IsActive = true;
      Entered?.Invoke();
    }

    public virtual void OnExit() {
      IsActive = false;
      Exited?.Invoke();
    }
  }
}
