using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input {
  public class InputChannel : ScriptableObject {
    [NonSerialized] public string CurrentMap = "UI";
    public event Action<string> MapChanged;

    [Header("Gameplay Actions")]
    public InputActionReference GameplayPause;

    [Header("UI Actions")]
    public InputActionReference UINavigate;
    public InputActionReference UISubmit;
    public InputActionReference UICancel;
    public InputActionReference UIScrollWheel;
    public InputActionReference UIPoint;
    public InputActionReference UIClick;
    public InputActionReference UIRightClick;
    public InputActionReference UIMiddleClick;

    public void SwitchToGameplay() {
      SetMap("Gameplay");
    }

    public void SwitchToUI() {
      SetMap("UI");
    }

    public void SetMap(string map) {
      CurrentMap = map;
      MapChanged?.Invoke(CurrentMap);
    }
  }
}
