using Settings;
using System;
using UnityEngine;
using Utils;

namespace View.Overlay {
  public class OverlayCamera : MonoBehaviour {
    [Inject] [SerializeField] private OverlayChannel _channel;
    [SerializeField] private float _ratio;
    [NonSerialized] public Camera NativeCamera;
    private CameraFitter _fitter;

    private void Awake() {
      NativeCamera = GetComponent<Camera>();
      _fitter = new CameraFitter(_ratio, NativeCamera.fieldOfView);
    }

    private void OnEnable() {
      _channel.Camera = this;
    }

    private void OnDisable() {
      _channel.Camera = null;
    }

    private void Update() {
      if (_fitter.TryUpdate(out var fov)) {
        NativeCamera.fieldOfView = fov;
      }
    }
  }
}
