using System;
using UnityEngine;
using View.Dialogue;

namespace View.Overlay {
  public class OverlayChannel : ScriptableObject {
    [NonSerialized] public bool IsReady;
    [NonSerialized] public HUDView HUD;
    [NonSerialized] public DialogueView Dialogue;

    private event Action<CameraManager> _cameraChanged;
    public event Action<CameraManager> CameraChanged {
      add {
        _cameraChanged += value;
        if (CameraManager != null) {
          value(CameraManager);
        }
      }
      remove => _cameraChanged -= value;
    }

    private CameraManager _cameraManager;
    public CameraManager CameraManager {
      get => _cameraManager;
      set {
        _cameraManager = value;
        IsReady = true;
        _cameraChanged?.Invoke(value);
      }
    }
  }
}
