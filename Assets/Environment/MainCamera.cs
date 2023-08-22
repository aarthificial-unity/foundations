using Settings;
using System;
using UnityEngine;
using Utils;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using View.Overlay;
using View.Settings;

namespace Environment {
  public class MainCamera : MonoBehaviour {
    [FormerlySerializedAs("_settings")] [Inject] [SerializeField] private OverlayChannel _overlay;
    private Camera _camera;
    private Camera _overlayCamera;

    private void Awake() {
      _camera = GetComponent<Camera>();
      _overlay.CameraChanged += HandleCameraChanged;
    }

    private void OnDestroy() {
      _overlay.CameraChanged -= HandleCameraChanged;
    }

    private void HandleCameraChanged(OverlayCamera overlayCamera) {
      var data = _camera.GetUniversalAdditionalCameraData();
      if (_overlayCamera != null) {
        data.cameraStack.Remove(_overlayCamera);
      }
      if (overlayCamera != null) {
        _overlayCamera = overlayCamera.NativeCamera;
        data.cameraStack.Add(_overlayCamera);
      }
    }

    private void Start() { }
  }
}
