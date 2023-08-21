using System;
using UnityEngine;
using View.Dialogue;

namespace View.Overlay {
  public class OverlayChannel : ScriptableObject {
    [NonSerialized] public HUDView HUD;
    [NonSerialized] public DialogueView Dialogue;

    private event Action<OverlayCamera> _cameraChanged;
    public event Action<OverlayCamera> CameraChanged {
      add {
        _cameraChanged += value;
        if (Camera != null) {
          value(Camera);
        }
      }
      remove => _cameraChanged -= value;
    }

    private OverlayCamera _camera;
    public OverlayCamera Camera {
      get => _camera;
      set {
        _camera = value;
        _cameraChanged?.Invoke(value);
      }
    }
  }
}
