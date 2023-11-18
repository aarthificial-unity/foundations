using UnityEngine;
using UnityEngine.InputSystem;

namespace Input {
  public class InputActions : ScriptableObject {
    [Header("Gameplay")]
    public InputActionReference GameplayPause;
    [Header("UI")]
    public InputActionReference UICancel;
    [Header("Pointing")]
    public InputActionReference PointingContinue;
    public InputActionReference PointingClick;
  }
}
