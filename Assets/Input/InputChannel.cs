using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input {
  public class InputChannel : ScriptableObject {
    [NonSerialized] public string CurrentMap = "UI";
    public event Action<string> MapChanged;

    public InputActionReference UINavigate;
    public InputActionReference UISubmit;
    public InputActionReference UICancel;
    public InputActionReference UIScrollWheel;
    public InputActionReference UIPoint;
    public InputActionReference UIClick;
    public InputActionReference UIRightClick;
    public InputActionReference UIMiddleClick;

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
