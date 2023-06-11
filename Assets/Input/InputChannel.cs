using System;
using UnityEngine;

namespace Input {
  public class InputChannel : ScriptableObject {
    [NonSerialized] public string CurrentMap = "UI";
    public event Action<string> MapChanged;

    public void SwitchToGameplay() {
      CurrentMap = "Gameplay";
      MapChanged?.Invoke(CurrentMap);
    }

    public void SwitchToUI() {
      CurrentMap = "UI";
      MapChanged?.Invoke(CurrentMap);
    }
  }
}
