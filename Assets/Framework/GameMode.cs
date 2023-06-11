using System;
using System.Collections;
using UnityEngine;

namespace Framework {
  public class GameMode : ScriptableObject {
    [NonSerialized] public GameManager Manager;

    public void Quit() {
      Manager.Quit();
    }

    public virtual void OnEditorStart() { }

    public virtual IEnumerator OnStart() {
      yield break;
    }

    public virtual IEnumerator OnEnd() {
      yield break;
    }
  }
}
