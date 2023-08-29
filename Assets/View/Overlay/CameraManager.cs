using UnityEngine;
using Utils;

namespace View.Overlay {
  public class CameraManager : MonoBehaviour {
    [Inject] [SerializeField] private OverlayChannel _channel;
    [SerializeField] private float _ratio;

    public Camera MainCamera;
    public Camera UICamera;
    public Camera OverlayCamera;
    private CameraFitter _fitter;

    private void Awake() {
      _fitter = new CameraFitter(_ratio, OverlayCamera.fieldOfView);
    }

    private void OnEnable() {
      _channel.CameraManager = this;
    }

    private void OnDisable() {
      _channel.CameraManager = null;
    }

    private void Update() {
      if (_fitter.TryUpdate(out var fov)) {
        OverlayCamera.fieldOfView = fov;
      }
    }
  }
}
