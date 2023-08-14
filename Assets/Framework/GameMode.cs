using System;
using System.Collections;
using UnityEngine;

namespace Framework {
  public class GameMode : ScriptableObject {
    protected GameManager Manager;

    public void Quit() {
      Manager.Quit();
    }

    public virtual void OnEditorStart() { }

    public virtual void Setup(GameManager manager) {
      Manager = manager;
    }

    public virtual IEnumerator OnStart() {
      yield break;
    }

    public virtual IEnumerator OnEnd() {
      yield break;
    }
  }
}
