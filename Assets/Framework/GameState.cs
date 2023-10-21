using System;
using System.Collections;
using UnityEngine;

namespace Framework {
  public class GameState : MonoBehaviour {
    protected GameManager Manager;

    protected virtual void Awake() {
      Manager = GetComponent<GameManager>();
    }

    [NonSerialized] public bool IsActive;

    public virtual IEnumerator OnUpdate() {
      yield break;
    }

    public virtual IEnumerator OnEnter() {
      IsActive = true;
      yield break;
    }

    public virtual IEnumerator OnExit() {
      IsActive = false;
      yield break;
    }

    protected virtual void OnDestroy() {
      if (IsActive) {
        OnExit();
      }
    }
  }
}
